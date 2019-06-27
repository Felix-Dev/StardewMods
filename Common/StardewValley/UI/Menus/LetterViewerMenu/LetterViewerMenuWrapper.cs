using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewMods.Common.StardewValley.LetterMenu;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Text;

namespace StardewMods.Common.StardewValley.UI
{
    //public class LetterViewerMenuWrapper
    //{
    //    private LetterViewerMenuEx letterMenu;

    //    /// <summary>
    //    /// Raised after the letter menu is closed. Exposes information such as the selected item, if any.
    //    /// </summary>
    //    public event EventHandler<LetterViewerMenuClosedEventArgs> MenuClosed;

    //    public LetterViewerMenuWrapper(IReflectionHelper reflectionHelper, string mailTitle, string mailContent, Item attachedItem = null)
    //    {
    //        letterMenu = new LetterViewerMenuEx(reflectionHelper, mailTitle, mailContent, attachedItem)
    //        {
    //            exitFunction = new IClickableMenu.onExit(OnExit)
    //        };
    //    }

    //    public void Show()
    //    {
    //        Game1.activeClickableMenu = letterMenu;
    //    }

    //    private void OnExit()
    //    {
    //        MenuClosed?.Invoke(this, new LetterViewerMenuClosedEventArgs(letterMenu.MailTitle, letterMenu.SelectedItem));
    //    }

    //    private class LetterViewerMenuEx : LetterViewerMenu
    //    {
    //        public string MailTitle { get; private set; }

    //        public Item SelectedItem { get; private set; }

    //        public LetterViewerMenuEx(IReflectionHelper reflectionHelper, string title, string content, Item attachedItem = null) : base(content)
    //        {
    //            var isMailRef = reflectionHelper
    //                .GetField<bool>(this, "isMail");
    //            var mailTitleRef = reflectionHelper
    //                .GetField<string>(this, "mailTitle");

    //            isMailRef?.SetValue(true);
    //            mailTitleRef?.SetValue(title);

    //            MailTitle = title;

    //            if (attachedItem == null)
    //            {
    //                return;
    //            }

    //            // Add item to mail
    //            this.itemsToGrab.Add(
    //                new ClickableComponent(
    //                    new Rectangle(this.xPositionOnScreen + this.width / 2 - 48, this.yPositionOnScreen + this.height - 32 - 96, 96, 96),
    //                    attachedItem)
    //                {
    //                    myID = region_itemGrabButton,
    //                    leftNeighborID = region_backButton,
    //                    rightNeighborID = region_forwardButton
    //                });

    //            this.backButton.rightNeighborID = region_itemGrabButton;
    //            this.forwardButton.leftNeighborID = region_itemGrabButton;

    //            this.populateClickableComponentList();
    //            this.snapToDefaultClickableComponent();
    //        }

    //        public override void receiveLeftClick(int x, int y, bool playSound = true)
    //        {
    //            foreach (ClickableComponent clickableComponent in this.itemsToGrab)
    //            {
    //                if (clickableComponent.containsPoint(x, y) && clickableComponent.item != null)
    //                {
    //                    // Set the selected item
    //                    SelectedItem = clickableComponent.item;

    //                    Game1.playSound("coin");
    //                    clickableComponent.item = null;

    //                    return;
    //                }
    //            }

    //            base.receiveLeftClick(x, y, playSound);
    //        }
    //    }
    //}
}
