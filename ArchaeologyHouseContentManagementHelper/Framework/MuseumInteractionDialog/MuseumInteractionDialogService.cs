using StardewModdingAPI;
using StardewMods.ArchaeologyHouseContentManagementHelper.Framework.Menus;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewMods.ArchaeologyHouseContentManagementHelper.Framework
{
    public class MuseumInteractionDialogService
    {
        private NPC gunther;

        private const string DialogOption_Donate = "Donate";
        private const string DialogOption_Rearrange = "Rearrange";
        private const string DialogOption_Collect = "Collect";
        private const string DialogOption_Status = "Status";
        private const string DialogOption_Leave = "Leave";      

        public MuseumInteractionDialogService()
        {
            gunther = Game1.getCharacterFromName(Constants.NPC_NAME_GUNTHER);
            if (gunther == null)
            {
                ModEntry.CommonServices.Monitor.Log("Error: NPC [Gunther] not found!", LogLevel.Error);
                throw new Exception("Error: NPC [Gunther] not found!");
            }
        }

        public void ShowDialog(MuseumInteractionDialogType dialogType)
        {
            switch (dialogType)
            {
                case MuseumInteractionDialogType.Donate:
                    Game1.player.currentLocation.createQuestionDialogue(
                            "",
                            new Response[3] {
                                new Response(DialogOption_Donate, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Donate")),
                                new Response(DialogOption_Status, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Status")),
                                new Response(DialogOption_Leave, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Leave"))
                            },
                            MuseumDialogAnswerHandler
                            );
                    break;

                case MuseumInteractionDialogType.DonateCollect:
                    Game1.player.currentLocation.createQuestionDialogue(
                            "",
                            new Response[4] {
                                new Response(DialogOption_Donate, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Donate")),
                                new Response(DialogOption_Collect, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Collect")),
                                new Response(DialogOption_Status, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Status")),
                                new Response(DialogOption_Leave, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Leave"))
                            },
                            MuseumDialogAnswerHandler
                            );
                    break;

                case MuseumInteractionDialogType.Rearrange:
                    Game1.player.currentLocation.createQuestionDialogue(
                           "",
                           new Response[3] {
                                new Response(DialogOption_Rearrange, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Rearrange")),
                                new Response(DialogOption_Status, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Status")),
                                new Response(DialogOption_Leave, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Leave"))
                           },
                           MuseumDialogAnswerHandler
                           );
                    break;

                case MuseumInteractionDialogType.RearrangeCollect:
                    Game1.player.currentLocation.createQuestionDialogue(
                           "",
                           new Response[4] {
                                new Response(DialogOption_Rearrange, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Rearrange")),
                                new Response(DialogOption_Collect, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Collect")),
                                new Response(DialogOption_Status, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Status")),
                                new Response(DialogOption_Leave, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Leave"))
                           },
                           MuseumDialogAnswerHandler
                           );
                    break;

                case MuseumInteractionDialogType.DonateRearrange:
                    Game1.player.currentLocation.createQuestionDialogue(
                        "",
                        new Response[4] {
                            new Response(DialogOption_Donate, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Donate")),
                            new Response(DialogOption_Rearrange, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Rearrange")),
                            new Response(DialogOption_Status, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Status")),
                            new Response(DialogOption_Leave, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Leave"))
                            },
                        MuseumDialogAnswerHandler
                        );
                    break;

                case MuseumInteractionDialogType.DonateRearrangeCollect:
                    Game1.player.currentLocation.createQuestionDialogue(
                        "",
                        new Response[5] {
                            new Response(DialogOption_Donate, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Donate")),
                            new Response(DialogOption_Rearrange, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Rearrange")),
                            new Response(DialogOption_Collect, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Collect")),
                            new Response(DialogOption_Status, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Status")),
                            new Response(DialogOption_Leave, ModEntry.CommonServices.TranslationHelper.Get("Gunther_MuseumInteractionMenu_Leave"))
                            },
                        MuseumDialogAnswerHandler
                        );
                    break;

                default:
                    throw new ArgumentException("Error: The [dialogType] is invalid!", nameof(dialogType));
            }
        }

        private void MuseumDialogAnswerHandler(Farmer farmer, string whichAnswer)
        {
            switch (whichAnswer)
            {
                case DialogOption_Donate:
                    Game1.activeClickableMenu = new MuseumMenuEx();
                    break;
                case DialogOption_Rearrange:
                    Game1.activeClickableMenu = new MuseumMenuNoInventory();
                    break;
                case DialogOption_Collect:
                    Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu((IList<Item>)LibraryMuseumHelper.GetRewardsForPlayer(Game1.player), 
                        false, true, (InventoryMenu.highlightThisItem)null, (ItemGrabMenu.behaviorOnItemSelect)null, 
                        "Rewards", new ItemGrabMenu.behaviorOnItemSelect(LibraryMuseumHelper.CollectedReward), 
                        false, false, false, false, false, 0, (Item)null, -1, (object)this);
                    break;
                case DialogOption_Status:
                    if (LibraryMuseumHelper.HasCollectedAllBooks && LibraryMuseumHelper.HasDonatedAllMuseumPieces)
                    {
                        Game1.drawDialogue(gunther, ModEntry.CommonServices.TranslationHelper.Get("Gunther_ArchaeologyHouseStatus_Completed"));
                    }
                    else
                    {
                        // Work-around to create newlines
                        string statusIntroLinePadding = ModEntry.CommonServices.TranslationHelper.Get("Gunther_ArchaeologyHouse_StatusIntroLinePadding");
                        if (statusIntroLinePadding.StartsWith("(no translation:"))
                        {
                            statusIntroLinePadding = "";
                        }

                        string libraryStatusLinePadding = ModEntry.CommonServices.TranslationHelper.Get("Gunther_ArchaeologyHouse_LibraryStatusLinePadding");
                        if (libraryStatusLinePadding.StartsWith("(no translation:"))
                        {
                            libraryStatusLinePadding = "";
                        }

                        Game1.drawDialogue(gunther, ModEntry.CommonServices.TranslationHelper.Get("Gunther_ArchaeologyHouse_StatusIntro") + statusIntroLinePadding +
                            ModEntry.CommonServices.TranslationHelper.Get("Gunther_ArchaeologyHouse_LibraryStatus") + $"{ LibraryMuseumHelper.LibraryBooks}/{LibraryMuseumHelper.TotalLibraryBooks}" + libraryStatusLinePadding +
                            ModEntry.CommonServices.TranslationHelper.Get("Gunther_ArchaeologyHouse_MuseumStatus") + $"{LibraryMuseumHelper.MuseumPieces}/{LibraryMuseumHelper.TotalMuseumPieces} ");
                    }                  
                    break;
                case DialogOption_Leave:
                    break;
            }
        }
    }
}
