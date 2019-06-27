using FelixDev.StardewMods.FeTK.Services;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FelixDev.StardewMods.ToolUpgradeDeliveryService.Compatibility;

using Microsoft.Xna.Framework;

using SObject = StardewValley.Object;
using Constants = FelixDev.StardewMods.ToolUpgradeDeliveryService.Common.Constants;
using Translation = FelixDev.StardewMods.ToolUpgradeDeliveryService.Common.Translation;

namespace FelixDev.StardewMods.ToolUpgradeDeliveryService.Framework
{
    /// <summary>
    /// This class is responsible for sending Clint's [upgraded-tool] mail to the player and adding the tool
    /// to the player's inventory.
    /// </summary>
    internal class MailDeliveryService
    {
        private const string TOOL_MAIL_ID_PREFIX = "ToolUpgrade_";
        private const string MOD_RUSH_ORDERS_MOD_ID = "spacechase0.RushOrders";

        private readonly IMonitor monitor;
        private readonly IModEvents events;
        private readonly ITranslationHelper translationHelper;
        private readonly IModRegistry modRegistry;

        private readonly MailManager mailManager;

        private bool running;

        private bool isPlayerUsingRushedOrders;
        private IRushOrdersApi rushOrdersApi;

        public MailDeliveryService()
        {
            events = ModEntry.CommonServices.Events;
            translationHelper = ModEntry.CommonServices.TranslationHelper;
            monitor = ModEntry.CommonServices.Monitor;

            modRegistry = ModEntry.ModHelper.ModRegistry;

            mailManager = ServiceFactory.GetFactory(Constants.MOD_ID, ModEntry.ModHelper).GetMailManager();
            ModEntry.ModHelper.Content.AssetEditors.Add(mailManager);

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

            mailManager.MailOpening += OnMailOpening;
            mailManager.MailClosed += OnMailClosed;

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

            mailManager.MailOpening -= OnMailOpening;
            mailManager.MailClosed -= OnMailClosed;

            events.GameLoop.GameLaunched -= OnGameLaunched;
            events.GameLoop.DayStarted -= OnDayStarted;

            running = false;
        }

        /// <summary>
        /// Setup compatibility with external mods by consuming their APIs.
        /// </summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            AddModCompatibility();
        }

        /// <summary>
        /// Called after the game begins a new day (including when the player loads a save).
        /// Checks, if a mail with the upgraded tool should be sent to the player for the next day.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // TODO: MailDeliveryService: Remove testing code
            Game1.player.Money = 100000;
            Game1.player.addItemToInventoryBool(new SObject(Vector2.Zero, 334, 100));
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
        /// Called when a mail is being opened by the player. Checks if the atatched tool should be displayed
        /// (i.e. in cases where the tool has already been received by Clint, the tool won't be attached to the mail)
        /// and sets the mail content accordingly (adding a hint that the tool was already received, if applicable).
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">ontent information about the opened mail.</param>
        private void OnMailOpening(object sender, MailOpeningEventArgs e)
        {
            // If the mail is not a tool uprade mail by Clint -> do nothing
            if (!IsToolMail(e.Mail.Id))
            {
                return;
            }

            Tool attachedTool = (Tool)e.Mail.AttachedItems[0];
            Tool currentToolUpgrade = Game1.player.toolBeingUpgraded.Value;

            /*
             * Check if the current upgrade tool matches the tool which was assigned to this mail.
             * 
             * Since the upgrade-mail content is generated when the mail is opened, the current upgrade tool 
             * could have changed in the meantime. In this case, the originally attached tool will NOT be included in the mail.
             */
            bool toolMatches = currentToolUpgrade != null
                && currentToolUpgrade.BaseName == attachedTool.BaseName
                && currentToolUpgrade.UpgradeLevel == attachedTool.UpgradeLevel;
            if (!toolMatches)
            {
                e.Mail.AttachedItems = null;
                e.Mail.Content += translationHelper.Get(Translation.MAIL_TOOL_UPGRADE_TOOL_ALREADY_RECEIVED);

                return;
            }

            // Bonus: Set the water level to full for the upgraded watering can.
            if (attachedTool is WateringCan can)
            {
                can.WaterLeft = can.waterCanMax;
            }
        }

        /// <summary>
        /// Called after a mail has been closed. Checks if the closed mail contained a tool upgrade 
        /// and adds the selected tool - if any - to the player's inventory.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMailClosed(object sender, MailClosedEventArgs e)
        {
            // If the mail is not a tool uprade mail by Clint or no tool was selected -> do nothing
            if (!IsToolMail(e.MailId) || e.SelectedItems.Count == 0)
            {
                return;
            }

            var selectedTool = (Tool)e.SelectedItems[0];

            /*
             * Check if tools of the same tool class (Axe, Hoe,...) should be removed from the player's inventory.
             * For example, this adds compatibility for the mod [Rented Tools] (i.e. rented tools will be removed).
             */
            if (ModEntry.ModConfig.RemoveToolDuplicates)
            {
                var removableItems = Game1.player.Items.Where(item => (item is Tool) && (item as Tool).BaseName.Equals(selectedTool.BaseName));
                foreach (var item in removableItems)
                {
                    Game1.player.removeItemFromInventory(item);
                }
            }

            // Add selected tool item to the player's inventory
            Game1.player.addItemByMenuIfNecessary(selectedTool);

            // Mark the tool upgrade process as finished, so that Clint won't hand it out when visiting him.
            Game1.player.toolBeingUpgraded.Value = null;
        }

        /// <summary>
        /// Checks if the mail with the specified ID is a tool-upgrade mail.
        /// </summary>
        /// <param name="mailId">The mail ID.</param>
        /// <returns>True, if the mail is a tool-upgrade mail, otherwise false.</returns>
        private bool IsToolMail(string mailId)
        {
            return mailId.StartsWith(TOOL_MAIL_ID_PREFIX);
        }

        /// <summary>
        /// Adds a mail with the specified tool included to the player's mailbox for the next day.
        /// </summary>
        /// <param name="tool">The tool to include in the mail.</param>
        private void AddToolMailForTomorrow(Tool tool)
        {
            string mailId = TOOL_MAIL_ID_PREFIX + tool.BaseName + tool.UpgradeLevel;

            string content = GetTranslatedMailContent(tool);
            content = content.Replace("@", Game1.player.Name);

            mailManager.AddMail(1, mailId, content, tool);
        }

        /// <summary>
        /// Get the translated mail content for a tool.
        /// </summary>
        /// <param name="tool">The tool to get the translated mail content for.</param>
        /// <returns>The translated mail content.</returns>
        private string GetTranslatedMailContent(Tool tool)
        {
            string translationKey;
            switch (tool)
            {
                case Axe _:
                    translationKey = Translation.MAIL_TOOL_UPGRADE_AXE;
                    break;
                case Pickaxe _:
                    translationKey = Translation.MAIL_TOOL_UPGRADE_PICKAXE;
                    break;
                case Hoe _:
                    translationKey = Translation.MAIL_TOOL_UPGRADE_HOE;
                    break;
                case WateringCan _:
                    translationKey = Translation.MAIL_TOOL_UPGRADE_WATERING_CAN;
                    break;
                default:
                    return null;
            }

            return translationHelper.Get(translationKey);
        }

        /// <summary>
        /// Sets up compatibility with other external mods.
        /// </summary>
        private void AddModCompatibility()
        {
            // Setup compatibility for the mod [Rush Orders].

            isPlayerUsingRushedOrders = false;

            // Check if the player uses the mod [Rush Orders]. 
            bool isLoaded = modRegistry.IsLoaded(MOD_RUSH_ORDERS_MOD_ID);
            if (!isLoaded)
            {
                return;
            }

            // The API we consume is only available starting with Rush Orders 1.1.4.
            IModInfo mod = modRegistry.Get(MOD_RUSH_ORDERS_MOD_ID);
            if (mod.Manifest.Version.IsOlderThan("1.1.4"))
            {
                monitor.Log($"You are running an unsupported version of the mod [{mod.Manifest.Name}]! " +
                    $"Please use at least [{mod.Manifest.Name} 1.1.4] for compatibility!", LogLevel.Info);
                return;
            }

            rushOrdersApi = modRegistry.GetApi<IRushOrdersApi>(MOD_RUSH_ORDERS_MOD_ID);
            if (rushOrdersApi == null)
            {
                monitor.Log($"Could not add compatibility for the mod [{mod.Manifest.Name}]! " +
                    $"A newer version of the mod [ToolUpgradeDeliveryService] might be needed.", LogLevel.Error);
                return;
            }

            rushOrdersApi.ToolRushed += OnPlacedRushOrder;
            isPlayerUsingRushedOrders = true;
        }
    }
}
