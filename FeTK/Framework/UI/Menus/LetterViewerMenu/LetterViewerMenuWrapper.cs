using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Text;

namespace FelixDev.StardewMods.FeTK.UI.Menus
{
    public class LetterViewerMenuWrapper
    {
        private LetterViewerMenuEx letterMenu;

        /// <summary>
        /// Raised after the letter menu is closed. Exposes information such as the selected item, if any.
        /// </summary>
        public event EventHandler<LetterViewerMenuClosedEventArgs> MenuClosed;

        public LetterViewerMenuWrapper(IReflectionHelper reflectionHelper, string mailTitle, string mailContent, List<Item> attachedItems = null)
        {
            letterMenu = new LetterViewerMenuEx(reflectionHelper, mailTitle, mailContent, attachedItems)
            {
                exitFunction = new IClickableMenu.onExit(OnExit)
            };
        }

        public void Show()
        {
            Game1.activeClickableMenu = letterMenu;
        }

        private void OnExit()
        {
            MenuClosed?.Invoke(this, new LetterViewerMenuClosedEventArgs(letterMenu.MailTitle, letterMenu.SelectedItems));
        }

        private class LetterViewerMenuEx : LetterViewerMenu
        {
            public string MailTitle { get; private set; }

            public List<Item> SelectedItems { get; private set; }

            public LetterViewerMenuEx(IReflectionHelper reflectionHelper, string title, string content, List<Item> attachedItems = null) : base(content)
            {
                reflectionHelper
                    .GetField<bool>(this, "isMail")
                    .SetValue(true);
                reflectionHelper
                    .GetField<string>(this, "mailTitle")
                    .SetValue(title);

                MailTitle = title;
                SelectedItems = new List<Item>();

                if (attachedItems == null || attachedItems.Count == 0)
                {
                    return;
                }

                // Add item(s) to mail
                foreach (var item in attachedItems)
                {
                    this.itemsToGrab.Add(
                        new ClickableComponent(
                            new Rectangle(this.xPositionOnScreen + this.width / 2 - 48, this.yPositionOnScreen + this.height - 32 - 96, 96, 96),
                            item)
                        {
                            myID = region_itemGrabButton,
                            leftNeighborID = region_backButton,
                            rightNeighborID = region_forwardButton
                        });
                }

                this.backButton.rightNeighborID = region_itemGrabButton;
                this.forwardButton.leftNeighborID = region_itemGrabButton;

                this.populateClickableComponentList();
                this.snapToDefaultClickableComponent();
            }

            public override void receiveLeftClick(int x, int y, bool playSound = true)
            {
                foreach (ClickableComponent clickableComponent in this.itemsToGrab)
                {
                    if (clickableComponent.containsPoint(x, y) && clickableComponent.item != null)
                    {
                        // Add the clicked item to the list of user-selected items.
                        SelectedItems.Add(clickableComponent.item);

                        Game1.playSound("coin");
                        clickableComponent.item = null;

                        return;
                    }
                }

                base.receiveLeftClick(x, y, playSound);
            }
        }
    }
}
