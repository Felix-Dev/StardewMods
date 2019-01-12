using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

namespace StardewMods.ArchaeologyHouseContentManagementHelper.Framework.Services
{
    /// <summary>
    /// This class is responsible for injecting the extended collections page into the game.
    /// </summary>
    internal class CollectionPageExMenuService
    {
        private IMonitor monitor;
        private bool running;

        private bool ignoreMenuChanged;
        private bool switchBackToCollectionsMenu;

        private IClickableMenu savedGameMenu;

        /// <summary>
        /// The index of the collections-page tab in the game menu.
        /// </summary>
        private int collectionsPageTabIndex;

        public CollectionPageExMenuService()
        {
            monitor = ModEntry.CommonServices.Monitor;

            collectionsPageTabIndex = -1;

            running = false;
        }

        public void Start(IModEvents events)
        {
            if (running)
            {
                monitor.Log("[CollectionPageExMenuService] is already running!", LogLevel.Info);
                return;
            }

            running = true;

            events.Display.MenuChanged += OnMenuChanged;
        }

        public void Stop(IModEvents events)
        {
            if (!running)
            {
                monitor.Log("[LostBookFoundDialogService] is not running or has already been stopped!", LogLevel.Info);
                return;
            }

            events.Display.MenuChanged -= OnMenuChanged;

            running = false;
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // menu closed
            if (e.NewMenu == null)
            {
                ignoreMenuChanged = false;

                if (e.OldMenu is LetterViewerMenu && switchBackToCollectionsMenu)
                {
                    ignoreMenuChanged = true;
                    Game1.activeClickableMenu = savedGameMenu;
                }

                switchBackToCollectionsMenu = false;
                return;
            }

            // menu changed or opened
            if (e.NewMenu is GameMenu gameMenu && !ignoreMenuChanged)
            {
                List<IClickableMenu> pages = ModEntry.CommonServices.ReflectionHelper.GetField<List<IClickableMenu>>(gameMenu, "pages").GetValue();

                if (collectionsPageTabIndex == -1)
                {
                    collectionsPageTabIndex = pages.Replace(tab => tab is CollectionsPage,
                        new CollectionsPageEx(gameMenu.xPositionOnScreen, gameMenu.yPositionOnScreen, gameMenu.width - 64 - 16, gameMenu.height));
                }
                else
                {
                    pages[collectionsPageTabIndex] = new CollectionsPageEx(gameMenu.xPositionOnScreen, gameMenu.yPositionOnScreen, gameMenu.width - 64 - 16, gameMenu.height);
                }
            }

            else if (e.NewMenu is LetterViewerMenu && e.OldMenu is GameMenu gameMenu2)
            {
                switchBackToCollectionsMenu = true;
                savedGameMenu = gameMenu2;
            }

            ignoreMenuChanged = false;
        }
    }
}
