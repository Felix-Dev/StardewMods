using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Text;

namespace FelixDev.StardewMods.FeTK.UI.Menus
{
    /// <summary>
    /// This class is a wrapper around the <see cref="LetterViewerMenu"/> class to provide an extended API, 
    /// such as:
    ///     - A <see cref="MenuClosed"/> event
    ///     - Programmatically settable attached items
    /// </summary>
    public class LetterViewerMenuWrapper
    {
        /// <summary>The <see cref="LetterViewerMenuEx"/> instance used to display the mail.</summary>
        private readonly LetterViewerMenuEx letterMenu;

        /// <summary>Raised when the letter viewer menu has been closed.</summary>
        public event EventHandler<LetterViewerMenuClosedEventArgs> MenuClosed;

        /// <summary>
        /// Create a new instance of the <see cref="LetterViewerMenuWrapper"/> class./>
        /// </summary>
        /// <param name="reflectionHelper">An instance of the <see cref="IReflectionHelper"/> class.</param>
        /// <param name="mailTitle">The title of mail to display.</param>
        /// <param name="mailContent">The content of the mail to display.</param>
        /// <param name="attachedItems">The attached items of the mail to display. May be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="reflectionHelper"/> is <c>null</c> -or-
        /// the specified <paramref name="mailTitle"/> is <c>null</c> -or-
        /// the specified <paramref name="mailContent"/> is <c>null</c>.
        /// </exception>
        public LetterViewerMenuWrapper(IReflectionHelper reflectionHelper, string mailTitle, string mailContent, List<Item> attachedItems = null)
        {
            if (reflectionHelper == null || mailTitle == null || mailContent == null)
            {
                throw new ArgumentNullException($"{nameof(reflectionHelper)}/{nameof(mailTitle)}/{nameof(mailContent)}");
            }

            letterMenu = new LetterViewerMenuEx(reflectionHelper, mailTitle, mailContent.Equals(string.Empty) ? " " : mailContent, attachedItems)
            {
                exitFunction = new IClickableMenu.onExit(OnExit)
            };
        }

        /// <summary>
        /// Display the menu.
        /// </summary>
        public void Show()
        {
            Game1.activeClickableMenu = letterMenu;
        }

        /// <summary>
        /// Called when the letter viewer menu is closed. Raises the <see cref="MenuClosed"/> event./>
        /// </summary>
        private void OnExit()
        {
            MenuClosed?.Invoke(this, new LetterViewerMenuClosedEventArgs(letterMenu.MailTitle, letterMenu.SelectedItems));
        }

        /// <summary>
        /// This class extends the <see cref="LetterViewerMenu"/> class with additional functionality such as 
        /// managing the selected items.
        /// </summary>
        private class LetterViewerMenuEx : LetterViewerMenu
        {
            /// <summary>The title of the mail.</summary>
            public string MailTitle { get; private set; }

            /// <summary>A list containing the selected items.</summary>
            public List<Item> SelectedItems { get; private set; }

            /// <summary>
            /// Create an instance of the <see cref="LetterViewerMenuEx"/> class.
            /// </summary>
            /// <param name="reflectionHelper"></param>
            /// <param name="title">The title of the mail.</param>
            /// <param name="content">The content of the mail.</param>
            /// <param name="attachedItems">The items attached to the mail. Can be <c>null</c>.</param>
            public LetterViewerMenuEx(IReflectionHelper reflectionHelper, string title, string content, List<Item> attachedItems = null) : base(content)
            {
                reflectionHelper
                    .GetField<bool>(this, "isMail")
                    .SetValue(true);
                reflectionHelper
                    .GetField<string>(this, "mailTitle")
                    .SetValue(title);

                MailTitle = title;

                if (attachedItems == null || attachedItems.Count == 0)
                {
                    return;
                }

                SelectedItems = new List<Item>();

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

            /// <summary>
            /// Add functionality to add a clicked item to the <see cref="SelectedItems"/> list./>
            /// </summary>
            /// <param name="x">X-coordinate of the click.</param>
            /// <param name="y">Y-coordinate of the click.</param>
            /// <param name="playSound">Not used.</param>
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
