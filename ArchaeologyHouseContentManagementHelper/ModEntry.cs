using System;
using StardewMods.ArchaeologyHouseContentManagementHelper.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Harmony;
using StardewMods.Common;
using StardewMods.ArchaeologyHouseContentManagementHelper.Framework.Services;
using StardewMods.ArchaeologyHouseContentManagementHelper.Patches;
using Constants = StardewMods.ArchaeologyHouseContentManagementHelper.Common.Constants;

namespace StardewMods.ArchaeologyHouseContentManagementHelper
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        private MuseumInteractionDialogService menuInteractDialogService;
        private LostBookFoundDialogService lostBookFoundDialogService;
        private CollectionPageExMenuService collectionPageExMenuService;

        public static CommonServices CommonServices { get; private set; }

        /// <summary>The mod configuration from the player.</summary>
        public static ModConfig ModConfig { get; private set; }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            if (helper == null)
            {
                Monitor.Log("Error: [modHelper] cannot be [null]!", LogLevel.Error);
                throw new ArgumentNullException(nameof(helper), "Error: [modHelper] cannot be [null]!");
            }

            ModConfig = Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += Bootstrap;
        }

        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialized at this point, so this is a good time to set up mod integrations.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            IModHelper helper = this.Helper;

            // Set services
            CommonServices = new CommonServices(Monitor, helper.Events, helper.Translation, helper.Reflection, helper.Content, helper.Data);

            // Apply game patches
            var harmony = HarmonyInstance.Create(Constants.MOD_ID);
            var addItemToInventoryBoolPatch = new AddItemToInventoryBoolPatch();
            var couldInventoryAcceptThisObjectPatch = new CouldInventoryAcceptThisObjectPatch();

            addItemToInventoryBoolPatch.Apply(harmony);
            couldInventoryAcceptThisObjectPatch.Apply(harmony);

            collectionPageExMenuService = new CollectionPageExMenuService();
            collectionPageExMenuService.Start();
        }

        private void Bootstrap(object sender, SaveLoadedEventArgs e)
        {
            // Start remaining services
            menuInteractDialogService = new MuseumInteractionDialogService();
            lostBookFoundDialogService = new LostBookFoundDialogService();

            menuInteractDialogService.Start();
            lostBookFoundDialogService.Start();
        }
    }
}
