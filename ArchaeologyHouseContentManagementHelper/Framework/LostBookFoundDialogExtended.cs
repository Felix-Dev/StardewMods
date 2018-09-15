using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewMods.ArchaeologyHouseContentManagementHelper.Framework
{
    public static class LostBookFoundDialogExtended
    {
        private static bool ShowMessage;

        private static bool calledSetup;

        public static void Setup()
        {
            if (calledSetup)
            {
                return;
            }

            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            MenuEvents.MenuClosed += MenuEvents_MenuClosed;

            calledSetup = true;
        }       

        private static void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (e.NewMenu is DialogueBox box)
            {
                var mostRecentlyGrabbed = Game1.player.mostRecentlyGrabbedItem;
                if (mostRecentlyGrabbed != null && mostRecentlyGrabbed.ParentSheetIndex == Framework.Constants.GAME_OBJECT_LOST_BOOK_ID)
                {
                    List<string> dialogues = ModEntry.CommonServices.ReflectionHelper.GetField<List<string>>(box, "dialogues").GetValue();
                    if (dialogues.Count == 1 && dialogues[0].Equals(mostRecentlyGrabbed.checkForSpecialItemHoldUpMeessage()) 
                        && LibraryMuseumHelper.LibraryBooks == LibraryMuseumHelper.TotalLibraryBooks)
                    {
                        ShowMessage = true;
                    }
                }
            }
        }

        private static void MenuEvents_MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            if (e.PriorMenu is DialogueBox box && ShowMessage)
            {
                Game1.drawObjectDialogue("Congratulations! You have found all lost books!");
                ShowMessage = false;
            }
        }
    }
}
