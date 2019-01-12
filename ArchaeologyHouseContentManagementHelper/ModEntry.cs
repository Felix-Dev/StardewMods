﻿using System;

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

            // Set services and mod configurations
            CommonServices = new CommonServices(Monitor, helper.Translation, helper.Reflection, helper.Content);
            ModConfig = Helper.ReadConfig<ModConfig>();

            // Apply game patches
            var harmony = HarmonyInstance.Create(Constants.MOD_ID);
            var addItemToInventoryBoolPatch = new AddItemToInventoryBoolPatch();
            var couldInventoryAcceptThisObjectPatch = new CouldInventoryAcceptThisObjectPatch();

            addItemToInventoryBoolPatch.Apply(harmony);
            couldInventoryAcceptThisObjectPatch.Apply(harmony);

            collectionPageExMenuService = new CollectionPageExMenuService();
            collectionPageExMenuService.Start(helper.Events);

            helper.Events.GameLoop.SaveLoaded += Bootstrap;
        }

        private void Bootstrap(object sender, SaveLoadedEventArgs e)
        {
            // Start remaining services
            menuInteractDialogService = new MuseumInteractionDialogService();
            lostBookFoundDialogService = new LostBookFoundDialogService();

            menuInteractDialogService.Start(this.Helper.Events);
            lostBookFoundDialogService.Start(this.Helper.Events);
        }
    }
}
