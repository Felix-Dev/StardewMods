using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.ToolUpgradeDeliveryService.Framework.Menus;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace StardewMods.ToolUpgradeDeliveryService.Framework
{
    /// <summary>
    /// This class is responsible for sending Clint's [upgraded-tool] mail to the player.
    /// </summary>
    internal class MailDeliveryService
    {
        private bool running;

        private IMonitor monitor;
        private IReflectionHelper reflectionHelper;
        private MailGenerator mailGenerator;

        public MailDeliveryService(MailGenerator generator)
        {
            reflectionHelper = ModEntry.CommonServices.ReflectionHelper;
            monitor = ModEntry.CommonServices.Monitor;

            mailGenerator = generator ?? throw new ArgumentNullException(nameof(generator));

            running = false;
        }

        public void Start(IModEvents events)
        {
            if (running)
            {
                monitor.Log("[MuseumDeliveryService] is already running!", LogLevel.Info);
                return;
            }

            running = true;
            events.GameLoop.DayStarted += OnDayStarted;
            events.Display.MenuChanged += OnMenuChanged;
        }

        public void Stop(IModEvents events)
        {
            if (!running)
            {
                monitor.Log("[MuseumDeliveryService] is not running or has already been stopped!", LogLevel.Info);
                return;
            }

            events.GameLoop.DayStarted -= OnDayStarted;
            events.Display.MenuChanged -= OnMenuChanged;
            running = false;
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.player.daysLeftForToolUpgrade.Value == 1)
            {
                string mailKey = mailGenerator.GenerateMailKey(Game1.player.toolBeingUpgraded.Value);
                if (mailKey == null)
                {
                    monitor.Log("Failed to generate mail for upgraded tool!", LogLevel.Error);
                    return;
                }

                Game1.addMailForTomorrow(mailKey);
                monitor.Log("Added [tool upgrade] mail to tomorrow's mailbox.", LogLevel.Info);
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
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

                // Set the water level to full for the upgraded watering can.
                if (toolForMail is WateringCan can)
                {
                    can.WaterLeft = can.waterCanMax;
                }

                var mailMessage = reflectionHelper.GetField<List<string>>(letterViewerMenu, "mailMessage").GetValue();
                Game1.activeClickableMenu = new LetterViewerMenuForToolUpgrade(mailMessage[0], toolForMail);
            }
        }
    }
}
