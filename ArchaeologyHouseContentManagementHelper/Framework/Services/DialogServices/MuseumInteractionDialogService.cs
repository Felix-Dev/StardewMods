﻿using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.ArchaeologyHouseContentManagementHelper.Framework.Menus;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewMods.Common.StardewValley;
using System;

using Translation = StardewMods.ArchaeologyHouseContentManagementHelper.Common.Translation;

namespace StardewMods.ArchaeologyHouseContentManagementHelper.Framework.Services
{
    /// <summary>
    /// This class is responsible for setting up and injecting the updated [Gunther_Museum_Interaction_Dialog]
    /// into the game.
    /// </summary>
    internal class MuseumInteractionDialogService
    {
        private readonly NPC gunther;

        private const string DialogOption_Donate = "Donate";
        private const string DialogOption_Rearrange = "Rearrange";
        private const string DialogOption_Collect = "Collect";
        private const string DialogOption_Status = "Status";
        private const string DialogOption_Leave = "Leave";

        private bool running;

        private ITranslationHelper translationHelper;
        private IMonitor monitor;

        public MuseumInteractionDialogService()
        {
            gunther = Game1.getCharacterFromName(StardewMods.Common.StardewValley.Constants.NPC_GUNTHER_NAME);
            if (gunther == null)
            {
                ModEntry.CommonServices.Monitor.Log("Error: NPC [Gunther] not found!", LogLevel.Error);
                throw new Exception("Error: NPC [Gunther] not found!");
            }

            translationHelper = ModEntry.CommonServices.TranslationHelper;
            monitor = ModEntry.CommonServices.Monitor;

            running = false;
        }

        public void Start(IModEvents events)
        {
            if (running)
            {
                monitor.Log("[MuseumInteractionDialogService] is already running!", LogLevel.Info);
                return;
            }

            running = true;
            events.Input.ButtonPressed += OnButtonPressed;
        }

        public void Stop(IModEvents events)
        {
            if (!running)
            {
                monitor.Log("[MuseumInteractionDialogService] is not running or has already been stopped!", LogLevel.Info);
                return;
            }

            events.Input.ButtonPressed -= OnButtonPressed;
            running = false;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.IsActionButton() && Context.IsPlayerFree && LibraryMuseumHelper.IsPlayerAtCounter(Game1.player))
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
                            ShowDialog(MuseumInteractionDialogType.DonateRearrangeCollect);
                        }

                        // Can donate and rearrange museum
                        else
                        {
                            ShowDialog(MuseumInteractionDialogType.DonateRearrange);
                        }
                    }

                    // Can donate & collect rewards & no item donated yet (cannot rearrange museum)
                    else if (LibraryMuseumHelper.HasPlayerCollectibleRewards(Game1.player))
                    {
                        ShowDialog(MuseumInteractionDialogType.DonateCollect);
                    }

                    // Can donate & no item donated yet (cannot rearrange)
                    else
                    {
                        ShowDialog(MuseumInteractionDialogType.Donate);
                    }
                }

                // No item to donate, donated at least one item and can potentially collect a reward
                else if (donatedItems > 0)
                {
                    // Can rearrange and collect a reward
                    if (LibraryMuseumHelper.HasPlayerCollectibleRewards(Game1.player))
                    {
                        ShowDialog(MuseumInteractionDialogType.RearrangeCollect);
                    }

                    // Can rearrange and no rewards available
                    else
                    {
                        ShowDialog(MuseumInteractionDialogType.Rearrange);
                    }
                }

                else
                {
                    // Show original game message. Currently in the following cases:
                    //  - When no item has been donated yet
                }
            }
        }

        private void ShowDialog(MuseumInteractionDialogType dialogType)
        {
            switch (dialogType)
            {
                case MuseumInteractionDialogType.Donate:
                    Game1.player.currentLocation.createQuestionDialogue(
                            "",
                            new Response[3] {
                                new Response(DialogOption_Donate, translationHelper.Get(Translation.GUNTHER_INTERACTION_DONATE)),
                                new Response(DialogOption_Status, translationHelper.Get(Translation.GUNTHER_INTERACTION_STATUS)),
                                new Response(DialogOption_Leave, translationHelper.Get(Translation.GUNTHER_INTERACTION_LEAVE))
                            },
                            this.MuseumDialogAnswerHandler
                            );
                    break;

                case MuseumInteractionDialogType.DonateCollect:
                    Game1.player.currentLocation.createQuestionDialogue(
                            "",
                            new Response[4] {
                                new Response(DialogOption_Donate, translationHelper.Get(Translation.GUNTHER_INTERACTION_DONATE)),
                                new Response(DialogOption_Collect, translationHelper.Get(Translation.GUNTHER_INTERACTION_COLLECT)),
                                new Response(DialogOption_Status, translationHelper.Get(Translation.GUNTHER_INTERACTION_STATUS)),
                                new Response(DialogOption_Leave, translationHelper.Get(Translation.GUNTHER_INTERACTION_LEAVE))
                            },
                            this.MuseumDialogAnswerHandler
                            );
                    break;

                case MuseumInteractionDialogType.Rearrange:
                    Game1.player.currentLocation.createQuestionDialogue(
                           "",
                           new Response[3] {
                                new Response(DialogOption_Rearrange, translationHelper.Get(Translation.GUNTHER_INTERACTION_REARRANGE)),
                                new Response(DialogOption_Status, translationHelper.Get(Translation.GUNTHER_INTERACTION_STATUS)),
                                new Response(DialogOption_Leave, translationHelper.Get(Translation.GUNTHER_INTERACTION_LEAVE))
                           },
                           this.MuseumDialogAnswerHandler
                           );
                    break;

                case MuseumInteractionDialogType.RearrangeCollect:
                    Game1.player.currentLocation.createQuestionDialogue(
                           "",
                           new Response[4] {
                                new Response(DialogOption_Rearrange, translationHelper.Get(Translation.GUNTHER_INTERACTION_REARRANGE)),
                                new Response(DialogOption_Collect, translationHelper.Get(Translation.GUNTHER_INTERACTION_COLLECT)),
                                new Response(DialogOption_Status, translationHelper.Get(Translation.GUNTHER_INTERACTION_STATUS)),
                                new Response(DialogOption_Leave, translationHelper.Get(Translation.GUNTHER_INTERACTION_LEAVE))
                           },
                           this.MuseumDialogAnswerHandler
                           );
                    break;

                case MuseumInteractionDialogType.DonateRearrange:
                    Game1.player.currentLocation.createQuestionDialogue(
                        "",
                        new Response[4] {
                            new Response(DialogOption_Donate, translationHelper.Get(Translation.GUNTHER_INTERACTION_DONATE)),
                            new Response(DialogOption_Rearrange, translationHelper.Get(Translation.GUNTHER_INTERACTION_REARRANGE)),
                            new Response(DialogOption_Status, translationHelper.Get(Translation.GUNTHER_INTERACTION_STATUS)),
                            new Response(DialogOption_Leave, translationHelper.Get(Translation.GUNTHER_INTERACTION_LEAVE))
                            },
                        this.MuseumDialogAnswerHandler
                        );
                    break;

                case MuseumInteractionDialogType.DonateRearrangeCollect:
                    Game1.player.currentLocation.createQuestionDialogue(
                        "",
                        new Response[5] {
                            new Response(DialogOption_Donate, translationHelper.Get(Translation.GUNTHER_INTERACTION_DONATE)),
                            new Response(DialogOption_Rearrange, translationHelper.Get(Translation.GUNTHER_INTERACTION_REARRANGE)),
                            new Response(DialogOption_Collect, translationHelper.Get(Translation.GUNTHER_INTERACTION_COLLECT)),
                            new Response(DialogOption_Status, translationHelper.Get(Translation.GUNTHER_INTERACTION_STATUS)),
                            new Response(DialogOption_Leave, translationHelper.Get(Translation.GUNTHER_INTERACTION_LEAVE))
                            },
                        this.MuseumDialogAnswerHandler
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
                    Game1.activeClickableMenu = new ItemGrabMenu(LibraryMuseumHelper.GetRewardsForPlayer(Game1.player), 
                        false, true, null, null, 
                        "Rewards", new ItemGrabMenu.behaviorOnItemSelect(LibraryMuseumHelper.CollectedReward), 
                        false, false, false, false, false, 0, null, -1, this);
                    break;
                case DialogOption_Status:
                    if (LibraryMuseumHelper.HasCollectedAllBooks && LibraryMuseumHelper.HasDonatedAllMuseumPieces)
                    {
                        Game1.drawDialogue(gunther, translationHelper.Get(Translation.GUNTHER_ARCHAEOLOGY_HOUSE_STATUS_COMPLETED));
                    }
                    else
                    {
                        DialogHelper.DrawDialog(translationHelper.Get(Translation.GUNTHER_ARCHAEOLOGY_HOUSE_STATUS_INTRO) + DialogHelper.DIALOG_NEWLINE +
                            translationHelper.Get(Translation.GUNTHER_ARCHAEOLOGY_HOUSE_LIBRARY_STATUS) + LibraryMuseumHelper.LibraryBooks + "/" + LibraryMuseumHelper.TotalLibraryBooks + DialogHelper.DIALOG_NEWLINE +
                            translationHelper.Get(Translation.GUNTHER_ARCHAEOLOGY_HOUSE_MUSEUM_STATUS) + LibraryMuseumHelper.MuseumPieces + "/" + LibraryMuseumHelper.TotalMuseumPieces,
                            gunther);
                    }                
                    
                    break;
                case DialogOption_Leave:
                    break;
            }
        }
    }
}
