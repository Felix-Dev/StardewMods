﻿using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

using Translation = StardewMods.ArchaeologyHouseContentManagementHelper.Common.Translation;

namespace StardewMods.ArchaeologyHouseContentManagementHelper.Framework.Services
{
    /// <summary>
    /// This class is responsible for firing the [All Lost Books found] message.
    /// </summary>
    internal class LostBookFoundDialogService
    {
        private bool showMessage;

        private bool running;

        private IMonitor monitor;

        public LostBookFoundDialogService()
        {
            monitor = ModEntry.CommonServices.Monitor;

            running = false;
        }

        public void Start(IModEvents events)
        {
            if (running)
            {
                monitor.Log("[LostBookFoundDialogService] is already running!", LogLevel.Info);
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
            // menu opened or changed
            if (e.NewMenu is DialogueBox box)
            {
                var mostRecentlyGrabbed = Game1.player.mostRecentlyGrabbedItem;
                if (mostRecentlyGrabbed != null && mostRecentlyGrabbed.ParentSheetIndex == StardewMods.Common.StardewValley.Constants.ID_GAME_OBJECT_LOST_BOOK)
                {
                    List<string> dialogues = ModEntry.CommonServices.ReflectionHelper.GetField<List<string>>(box, "dialogues").GetValue();
                    if (dialogues.Count == 1 && dialogues[0].Equals(mostRecentlyGrabbed.checkForSpecialItemHoldUpMeessage()) 
                        && LibraryMuseumHelper.LibraryBooks == LibraryMuseumHelper.TotalLibraryBooks)
                    {
                        showMessage = true;
                    }
                }
            }

            // menu closed
            else if (e.NewMenu == null && e.OldMenu is DialogueBox && showMessage)
            {
                Game1.drawObjectDialogue(ModEntry.CommonServices.TranslationHelper.Get(Translation.MESSAGE_LIBRARY_BOOKS_COMPLETED));
                showMessage = false;
            }
        }
    }
}
