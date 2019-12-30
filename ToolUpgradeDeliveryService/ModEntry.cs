using FelixDev.StardewMods.ToolUpgradeDeliveryService.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace FelixDev.StardewMods.ToolUpgradeDeliveryService
{
    /// <summary>
    /// Represents the entry point for the Tool-Upgrade Delivery Service mod. Initializes and starts all the needed
    /// services (such as the mail delivery service).
    /// </summary>
    internal class ModEntry : Mod
    {
        /// <summary>The mail-delivery service to use.</summary>
        private MailDeliveryService mailDeliveryService;

        /// <summary>The mod configuration from the player.</summary>
        public static ModConfig ModConfig { get; private set; }

        /// <summary>Provides access to the simplified APIs for writing mods provided by SMAPI.</summary>
        public static IModHelper ModHelper { get; private set; }

        /// <summary>Provides access to the <see cref="IMonitor"/> API provided by SMAPI.</summary>
        public static IMonitor _Monitor { get; private set; }

        /// <summary>Provides access to the <see cref="IManifest"/> API provided by SMAPI.</summary>
        public static IManifest _ModManifest { get; private set; }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            _Monitor = this.Monitor;
            _ModManifest = this.ModManifest;

            ModConfig = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialized at this point, so this is a good time to set up mod integrations.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Start services
            mailDeliveryService = new MailDeliveryService();
            
            mailDeliveryService.Start();
        }
    }
}
