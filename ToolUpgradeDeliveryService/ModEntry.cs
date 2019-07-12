using StardewModdingAPI;
using StardewMods.Common;
using FelixDev.StardewMods.ToolUpgradeDeliveryService.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.ToolUpgradeDeliveryService
{
    /// <summary>
    /// Represents the entry point for the Tool-Upgrade Delivery Service mod. Initializes and starts all the needed
    /// services (such as the mail delivery service).
    /// </summary>
    internal class ModEntry : Mod
    {
        public static CommonServices CommonServices { get; private set; }

        /// <summary>The mod configuration from the player.</summary>
        public static ModConfig ModConfig { get; private set; }

        public static IModHelper ModHelper { get; private set; }

        public static IManifest _ModManifest { get; private set; }

        private MailDeliveryService mailDeliveryService;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Add services
            CommonServices = new CommonServices(Monitor, helper.Events, helper.Translation, helper.Reflection, helper.Content, helper.Data);

            ModHelper = helper;
            _ModManifest = this.ModManifest;

            // Setup services & mod configuration
            ModConfig = helper.ReadConfig<ModConfig>();

            mailDeliveryService = new MailDeliveryService();

            // Start services
            mailDeliveryService.Start();
        }
    }
}
