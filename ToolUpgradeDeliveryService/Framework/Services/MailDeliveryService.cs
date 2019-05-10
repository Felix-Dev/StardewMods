using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.StardewValley.LetterMenu;
using StardewMods.ToolUpgradeDeliveryService.Compatibility;

using SObject = StardewValley.Object;
using Microsoft.Xna.Framework;

namespace StardewMods.ToolUpgradeDeliveryService.Framework
{
    /// <summary>
    /// This class is responsible for sending Clint's [upgraded-tool] mail to the player and adding the tool
    /// to the player's inventory.
    /// </summary>
    internal class MailDeliveryService
    {
        private const string MOD_RUSH_ORDERS_MOD_ID = "spacechase0.RushOrders";

        private bool running;

        private readonly IMonitor monitor;
        private readonly IModEvents events;
        private readonly IReflectionHelper reflectionHelper;
        private readonly IModRegistry modRegistry;

        private MailGenerator mailGenerator;

        private bool isPlayerUsingRushedOrders;
        private IRushOrdersApi rushOrdersApi;

        public MailDeliveryService(MailGenerator generator, IModRegistry registry)
        {
            events = ModEntry.CommonServices.Events;
            reflectionHelper = ModEntry.CommonServices.ReflectionHelper;
            monitor = ModEntry.CommonServices.Monitor;

            modRegistry = registry ?? throw new ArgumentNullException(nameof(registry));

            mailGenerator = generator ?? throw new ArgumentNullException(nameof(generator));

            running = false;
        }

        public void Start()
        {
            if (running)
            {
                monitor.Log("[MailDeliveryService] is already running!", LogLevel.Info);
                return;
            }

            running = true;

            events.GameLoop.GameLaunched += OnGameLaunched;
            events.GameLoop.DayStarted += OnDayStarted;
            events.Display.MenuChanged += OnMenuChanged;

            if (isPlayerUsingRushedOrders)
            {
                rushOrdersApi.ToolRushed += OnPlacedRushOrder;
            }
        }

        public void Stop()
        {
            if (!running)
            {
                monitor.Log("[MailDeliveryService] is not running or has already been stopped!", LogLevel.Info);
                return;
            }

            if (isPlayerUsingRushedOrders)
            {
                rushOrdersApi.ToolRushed -= OnPlacedRushOrder;
            }

            events.GameLoop.GameLaunched -= OnGameLaunched;
            events.GameLoop.DayStarted -= OnDayStarted;
            events.Display.MenuChanged -= OnMenuChanged;

            running = false;
        }

        /// <summary>
        /// Raised after the game is launched, right before the first update tick. 
        /// All mods are loaded and initialised at this point.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (modRegistry != null)
            {
                AddModCompatibility();
            }
            else
            {
                monitor.Log("No mod registry provided => mod compatibility not available for certain mods!", LogLevel.Warn);
            }
        }

        /// <summary>
        /// Called after the game begins a new day (including when the player loads a save).
        /// Checks, if a mail with the upgraded tool should be sent to the player for the next day.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            Game1.player.Money = 100000;
            Game1.player.addItemToInventoryBool(new SObject(Vector2.Zero, 336, 100));

            if (Game1.player.daysLeftForToolUpgrade.Value == 1)
            {
                AddToolMailForTomorrow(Game1.player.toolBeingUpgraded.Value);
            }
        }

        /// <summary>
        /// Called after the player placed a rushed order at Clint's for a faster tool upgrade.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The ordered tool.</param>
        /// <remarks>
        /// This handler makes ToolUpgradeDeliveryService compatible with the mod [RushOrders].
        /// </remarks>
        private void OnPlacedRushOrder(object sender, Tool e)
        {
            // A rushed order as provided by the mod [Rush Orders] always finishes in one day or less.
            AddToolMailForTomorrow(e);
        }

        /// <summary>
        /// Called after a game menu is opened, closed, or replaced.
        /// Responsible for displaying the actual content of a [Tool-Upgrade] mail, such as 
        /// whether to show an attached tool, set the attached tool.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!(e.OldMenu is LetterViewerMenu) && e.NewMenu is LetterViewerMenu letterViewerMenu)
            {
                var mailTitle = reflectionHelper.GetField<string>(letterViewerMenu, "mailTitle").GetValue();
                if (mailTitle == null || !mailGenerator.IsToolMail(mailTitle))
                {
                    return;
                }

                ToolUpgradeInfo upgradeInfo = mailGenerator.GetMailAssignedToolUpgrade(mailTitle);
                if (upgradeInfo == null)
                {
                    monitor.Log("Failed to retrive tool data from mail!", LogLevel.Error);
                }

                Tool toolForMail = Game1.player.toolBeingUpgraded.Value;

                /*
                 * Check if the current upgrade tool matches with the tool which was assigned to this mail.
                 * 
                 * Since the upgrade-mail content is generated when the mail is opened, the current upgrade tool 
                 * could have changed in the meantime. In this case, no tool will be included in this mail.
                 */
                if (toolForMail != null && (toolForMail.GetType() != upgradeInfo.ToolType || toolForMail.UpgradeLevel != upgradeInfo.Level))
                {
                    toolForMail = null;
                }

                // Bonus: Set the water level to full for the upgraded watering can.
                if (toolForMail is WateringCan can)
                {
                    can.WaterLeft = can.waterCanMax;
                }

                var mailMessage = reflectionHelper.GetField<List<string>>(letterViewerMenu, "mailMessage").GetValue();

                var itemMenu = new ItemLetterMenuHelper(mailMessage[0], toolForMail);
                itemMenu.MenuClosed += OnToolMailClosed;

                itemMenu.Show();
            }
        }

        /// <summary>
        /// Called after a tool-mail has been closed. Handles adding the selected tool
        /// to the player's inventory.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnToolMailClosed(object sender, ItemLetterMenuClosedEventArgs e)
        {
            // Do nothing if no mail-included tool was selected
            if (e.SelectedItem == null)
            {
                return;
            }

            /*
             * Check if tools of the same tool class (Axe, Hoe,...) should be removed from the player's inventory.
             * For example, this adds compatibility for the mod [Rented Tools] (i.e. rented tools will be removed).
             */
            if (ModEntry.ModConfig.RemoveToolDuplicates)
            {
                var removableItems = Game1.player.Items.Where(item => (item is Tool) && (item as Tool).BaseName.Equals(((Tool)e.SelectedItem).BaseName));
                foreach (var item in removableItems)
                {
                    Game1.player.removeItemFromInventory(item);
                }
            }            

            // Add selected tool item to the player's inventory
            Game1.player.addItemByMenuIfNecessary(e.SelectedItem);

            // Mark the tool upgrade process as finished, so that Clint won't hand it out when visiting him.
            Game1.player.toolBeingUpgraded.Value = null;
        }

        /// <summary>
        /// Adds a mail with the specified tool included to the player's mailbox for the next day.
        /// </summary>
        /// <param name="tool">The tool to include in the mail.</param>
        private void AddToolMailForTomorrow(Tool tool)
        {
            string mailKey = mailGenerator.GenerateMailKey(tool);
            if (mailKey == null)
            {
                monitor.Log("Failed to generate mail for upgraded tool!", LogLevel.Error);
                return;
            }

            Game1.addMailForTomorrow(mailKey);
            monitor.Log("Added [tool upgrade] mail to tomorrow's mailbox.", LogLevel.Info);
        }

        /// <summary>
        /// Sets up compatibility with other external mods.
        /// </summary>
        private void AddModCompatibility()
        {
            isPlayerUsingRushedOrders = false;

            // check if a mod is loaded
            bool isLoaded = modRegistry.IsLoaded(MOD_RUSH_ORDERS_MOD_ID);
            if (!isLoaded)
            {
                return;
            }

            // get info for a mod
            IModInfo mod = modRegistry.Get(MOD_RUSH_ORDERS_MOD_ID);
            if (!mod.Manifest.Version.IsNewerThan("1.1.3"))
            {
                monitor.Log($"You are running an unsupported version of the mod [{mod.Manifest.Name}]! " +
                    $"Please use at least [{mod.Manifest.Name} 1.1.4] for compatibility!", LogLevel.Info);
                return;
            }

            rushOrdersApi = modRegistry.GetApi<IRushOrdersApi>(MOD_RUSH_ORDERS_MOD_ID);
            if (rushOrdersApi == null)
            {
                monitor.Log($"Could not add compatibility for the mod [{mod.Manifest.Name}]! " +
                    $"A new version of the mod [ToolUpgradeDeliveryService] might be needed.", LogLevel.Error);
                return;
            }

            rushOrdersApi.ToolRushed += OnPlacedRushOrder;
            isPlayerUsingRushedOrders = true;

        }
    }
}
