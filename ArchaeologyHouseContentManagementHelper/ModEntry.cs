using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewMods.ArchaeologyHouseContentManagementHelper.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using Harmony;
using StardewMods.Common;
using StardewValley.Menus;
using Microsoft.Xna.Framework;

namespace StardewMods.ArchaeologyHouseContentManagementHelper
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        private MuseumInteractionDialogService dialogService;

        public static CommonServices CommonServices { get; private set; }

        /// <summary>The mod configuration from the player.</summary>
        public  static ModConfig ModConfig { get; private set; }

        private bool switchBackToCollectionsMenu;
        private bool ignoreMenuChanged;
        private GameMenu savedGameMenu;

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

            // Patch the game
            var harmony = HarmonyInstance.Create("StardewMods.ArchaeologyHouseContentManagementHelper");
            Patches.Patch.PatchAll(harmony);

            MenuEvents.MenuChanged += MenuEvents_MenuChanged;

            SaveEvents.AfterLoad += Bootstrap;
        }

        private void Bootstrap(object sender, EventArgs e)
        {
            dialogService = new MuseumInteractionDialogService();

            InputEvents.ButtonPressed += InputEvents_ButtonPressed;
            
            MenuEvents.MenuClosed += MenuEvents_MenuClosed;

            LostBookFoundDialogExtended.Setup();
        }

        private void MenuEvents_MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            ignoreMenuChanged = false;

            if (e.PriorMenu is LetterViewerMenu && switchBackToCollectionsMenu)
            {           
                List<IClickableMenu> pages = CommonServices.ReflectionHelper.GetField<List<IClickableMenu>>(savedGameMenu, "pages").GetValue();

                CollectionsPage collectionPage;
                int i = 0;

                foreach (var page in pages)
                {
                    if (page is CollectionsPage)
                    {
                        collectionPage = (CollectionsPage)page;
                        break;
                    }
                    i++;
                }

                pages.RemoveAt(i);
                pages.Insert(i, new CollectionsPageEx(
                        savedGameMenu.xPositionOnScreen, savedGameMenu.yPositionOnScreen,
                        savedGameMenu.width - 64 - 16, savedGameMenu.height, CollectionsPageEx.lostBooksTab));

                ignoreMenuChanged = true;
                Game1.activeClickableMenu = savedGameMenu;
            }

            switchBackToCollectionsMenu = false;
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (e.NewMenu is GameMenu gameMenu && !ignoreMenuChanged)
            {
                List<IClickableMenu> pages = CommonServices.ReflectionHelper.GetField<List<IClickableMenu>>(gameMenu, "pages").GetValue();

                int i = 0;
                CollectionsPage collectionPage;

                foreach (var page in pages)
                {
                    if (page is CollectionsPage)
                    {
                        collectionPage = (CollectionsPage)page;
                        break;
                    }
                    i++;
                }

                pages.RemoveAt(i);
                pages.Insert(i, new CollectionsPageEx(gameMenu.xPositionOnScreen, gameMenu.yPositionOnScreen, gameMenu.width - 64 - 16, gameMenu.height));
            }

            else if (e.NewMenu is LetterViewerMenu && e.PriorMenu is GameMenu gameMenu2)
            {
                switchBackToCollectionsMenu = true;
                savedGameMenu = gameMenu2;
            }

            ignoreMenuChanged = false;
        }


        /// <summary>The method invoked when the player presses a controller, keyboard, or mouse button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {    
            if (e.IsActionButton && Context.IsPlayerFree && LibraryMuseumHelper.IsPlayerAtCounter(Game1.player))
            {
                LibraryMuseum museum = Game1.currentLocation as LibraryMuseum;
                bool canDonate = museum.doesFarmerHaveAnythingToDonate(Game1.player);

                int donatedItems = LibraryMuseumHelper.MuseumPieces;
            
                if (canDonate)
                {
                    if (donatedItems > 0)
                    {
                        // Can donate, rearrange museum and collect rewards
                        if (LibraryMuseumHelper.HasPlayerCollectibleRewards(Game1.player))
                        {
                            dialogService.ShowDialog(MuseumInteractionDialogType.DonateRearrangeCollect);
                        }

                        // Can donate and rearrange museum
                        else
                        {
                            dialogService.ShowDialog(MuseumInteractionDialogType.DonateRearrange);
                        }                        
                    }

                    // Can donate & collect rewards & no item donated yet (cannot rearrange museum)
                    else if (LibraryMuseumHelper.HasPlayerCollectibleRewards(Game1.player))
                    {
                        dialogService.ShowDialog(MuseumInteractionDialogType.DonateCollect);
                    }

                    // Can donate & no item donated yet (cannot rearrange)
                    else
                    {
                        dialogService.ShowDialog(MuseumInteractionDialogType.Donate);
                    }
                }

                // No item to donate, donated at least one item and can potentially collect a reward
                else if (donatedItems > 0)
                {
                    // Can rearrange and collect a reward
                    if (LibraryMuseumHelper.HasPlayerCollectibleRewards(Game1.player))
                    {
                        dialogService.ShowDialog(MuseumInteractionDialogType.RearrangeCollect);
                    }

                    // Can rearrange and no rewards available
                    else
                    {
                        dialogService.ShowDialog(MuseumInteractionDialogType.Rearrange);
                    }                    
                }

                else
                {
                    // Show original game message. Currently in the following cases:
                    //  - When no item has been donated yet
                }
            }
        }
    }
}
