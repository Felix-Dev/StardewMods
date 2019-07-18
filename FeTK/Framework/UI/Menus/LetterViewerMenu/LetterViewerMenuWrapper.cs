using FelixDev.StardewMods.FeTK.Framework.Helpers;
using FelixDev.StardewMods.FeTK.Framework.UI.Parsers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Text;

namespace FelixDev.StardewMods.FeTK.UI.Menus
{
    /// <summary>
    /// This class is a wrapper around the <see cref="LetterViewerMenu"/> class to provide an extended API 
    /// such as:
    ///     - A <see cref="MenuClosed"/> event
    ///     - Programmatically settable attached items
    ///     - A text coloring API - see <see cref="StringColorParser"/>
    ///     
    /// It also fixes an in-game bug (up to 1.3.36 at least) which only displays the last attached item 
    /// in a collection of attached items.
    /// </summary>
    public class LetterViewerMenuWrapper
    {
        /// <summary>Provides access to the <see cref="IReflectionHelper"/> API provided by SMAPI.</summary>
        private static readonly IReflectionHelper reflectionHelper = ToolkitMod.ModHelper.Reflection;

        /// <summary>The <see cref="LetterViewerMenuEx"/> instance used to display the mail.</summary>
        private readonly LetterViewerMenuEx letterMenu;

        /// <summary>Raised when the letter viewer menu has been closed.</summary>
        public event EventHandler<LetterViewerMenuClosedEventArgs> MenuClosed;

        /// <summary>
        /// Create a new instance of the <see cref="LetterViewerMenuWrapper"/> class./>
        /// </summary>
        /// <param name="mailTitle">The title of mail to display.</param>
        /// <param name="mailContent">The content of the mail to display.</param>
        /// <param name="attachedItems">The attached items of the mail to display. May be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">
        /// the specified <paramref name="mailTitle"/> is <c>null</c> -or-
        /// the specified <paramref name="mailContent"/> is <c>null</c>.
        /// </exception>
        public LetterViewerMenuWrapper(string mailTitle, string mailContent, List<Item> attachedItems = null)
        {
            if (mailTitle == null)
            {
                throw new ArgumentNullException(nameof(mailTitle));
            }

            if (mailContent == null)
            {
                throw new ArgumentNullException(nameof(mailContent));
            }

            letterMenu = new LetterViewerMenuEx(mailTitle, mailContent.Equals(string.Empty) ? " " : mailContent, attachedItems)
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
        internal class LetterViewerMenuEx : LetterViewerMenu
        {
            #region LetterViewerMenu Reflection Fields

            private IReflectedField<float> scaleRef;
            private IReflectedField<int> whichBGRef;

            private IReflectedField<int> moneyIncludedRef;

            private IReflectedField<List<string>> mailMessageRef;
            private IReflectedField<int> pageRef;

            private IReflectedField<string> learnedRecipeRef;
            private IReflectedField<string> cookingOrCraftingRef;
            private IReflectedField<int> questIdRef;
            private IReflectedField<int> secretNoteImageRef;

            #endregion // LetterViewerMenu Reflection Fields

            /// <summary>Contains a collection of <see cref="TextColorInfo"/> objects, if any, for the specified mail content.</summary>
            private readonly List<TextColorInfo> textColorData;

            /// <summary>The title of the mail.</summary>
            public string MailTitle { get; private set; }

            /// <summary>A list containing the selected items.</summary>
            public List<Item> SelectedItems { get; private set; }

            /// <summary>
            /// Create an instance of the <see cref="LetterViewerMenuEx"/> class.
            /// </summary>
            /// <param name="title">The title of the mail.</param>
            /// <param name="content">The content of the mail.</param>
            /// <param name="attachedItems">The items attached to the mail. Can be <c>null</c>.</param>
            public LetterViewerMenuEx(string title, string content, List<Item> attachedItems = null) : base(content)
            {
                void SetupReflection()
                {
                    scaleRef = reflectionHelper
                    .GetField<float>(this, "scale");

                    whichBGRef = reflectionHelper
                        .GetField<int>(this, "whichBG");

                    mailMessageRef = reflectionHelper
                        .GetField<List<string>>(this, "mailMessage");

                    pageRef = reflectionHelper
                        .GetField<int>(this, "page");

                    moneyIncludedRef = reflectionHelper
                        .GetField<int>(this, "moneyIncluded");

                    learnedRecipeRef = reflectionHelper
                            .GetField<string>(this, "learnedRecipe");

                    cookingOrCraftingRef = reflectionHelper
                            .GetField<string>(this, "cookingOrCrafting");

                    questIdRef = reflectionHelper
                            .GetField<int>(this, "questID");

                    secretNoteImageRef = reflectionHelper
                            .GetField<int>(this, "secretNoteImage");
                }
                SetupReflection();

                reflectionHelper
                    .GetField<bool>(this, "isMail")
                    .SetValue(true);
                reflectionHelper
                    .GetField<string>(this, "mailTitle")
                    .SetValue(title);

                MailTitle = title;

                // Check if the mail content uses the text coloring API and parse it accordingly.
                bool couldParse = StringColorParser.TryParse(content, SpriteTextHelper.GetColorFromIndex(GetTextColor(whichBGRef.GetValue())), out textColorData);
                if (couldParse)
                {
                    StringBuilder parsedStringBuilder = new StringBuilder();
                    textColorData.ForEach(mapping => parsedStringBuilder.Append(mapping.Text));

                    var parsedString = parsedStringBuilder.ToString();

                    // The mail content was parsed successfully. The original mail content might have contained pairs of <color></color> tags 
                    // which then were removed in the resulting parsed string output. Hence the length of the resulting parsed string and the
                    // length of the original mail content string might differ (the former being shorter) which requires a new run to break up
                    // the resulting mail content into different mail pages. The previous run worked on the original mail message whch might have
                    // contained now removed <color> tags.
                    if (parsedString.Length < content.Length)
                    {
                        mailMessageRef.SetValue(SpriteText.getStringBrokenIntoSectionsOfHeight(parsedString, this.width - 64, this.height - 128));
                    }
                }

                // If the mail has attached items, add them to the LetterViewerMenu so they will be shown when the
                // mail is drawn to the screen.
                if (attachedItems?.Count > 0)
                {
                    SelectedItems = new List<Item>();

                    foreach (var item in attachedItems)
                    {
                        var attachedItemComponent = new ClickableComponent(
                            new Rectangle(this.xPositionOnScreen + this.width / 2 - 48, this.yPositionOnScreen + this.height - 32 - 96, 96, 96),
                            item)
                        {
                            myID = region_itemGrabButton,
                            leftNeighborID = region_backButton,
                            rightNeighborID = region_forwardButton
                        };

                        this.itemsToGrab.Add(attachedItemComponent);
                    }

                    this.backButton.rightNeighborID = region_itemGrabButton;
                    this.forwardButton.leftNeighborID = region_itemGrabButton;
                }

                if (!Game1.options.SnappyMenus)
                    return;

                this.populateClickableComponentList();
                this.snapToDefaultClickableComponent();

                // We potentially changed the number of pages the mail content has been split up
                // after the content was parsed by the text coloring parser. Hence we might need to
                // update the [Back Button] and [Forward Button] settings. 
                var mailMessage = mailMessageRef.GetValue();
                if (mailMessage == null || mailMessage.Count > 1)
                    return;

                this.backButton.myID = -100;
                this.forwardButton.myID = -100;
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

            /// <summary>
            /// Draw the letter menu. Our implementation fixes a bug in the game where only the last item
            /// of the attached mail items is always drawn.
            /// </summary>
            /// <param name="b"></param>
            public override void draw(SpriteBatch b)
            {
                #region Setup local variables with reflection

                int whichBG = whichBGRef.GetValue();
                float scale = scaleRef.GetValue();

                int page = pageRef.GetValue();
                List<string> mailMessage = mailMessageRef.GetValue();

                int moneyIncluded = moneyIncludedRef.GetValue();

                string learnedRecipe = learnedRecipeRef.GetValue();
                string cookingOrCrafting = cookingOrCraftingRef.GetValue();

                int secretNoteImage = secretNoteImageRef.GetValue();

                int questID = questIdRef.GetValue();

                #endregion // Setup local variables with reflection

                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);

                // Draw the letter background.
                b.Draw(letterTexture, new Vector2((float)(xPositionOnScreen + this.width / 2), (float)(this.yPositionOnScreen + this.height / 2)), new Rectangle?(new Rectangle(whichBG * 320, 0, 320, 180)), Color.White, 0.0f, new Vector2(160f, 90f), 4f * scale, SpriteEffects.None, 0.86f);

                if (scale == 1.0)
                {
                    if (secretNoteImage != -1)
                    {
                        b.Draw(this.secretNoteImageTexture, new Vector2((float)(this.xPositionOnScreen + this.width / 2 - 128 - 4), (float)(this.yPositionOnScreen + this.height / 2 - 128 + 8)), new Rectangle?(new Rectangle(secretNoteImage * 64 % this.secretNoteImageTexture.Width, secretNoteImage * 64 / this.secretNoteImageTexture.Width * 64, 64, 64)), Color.Black * 0.4f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
                        b.Draw(this.secretNoteImageTexture, new Vector2((float)(this.xPositionOnScreen + this.width / 2 - 128), (float)(this.yPositionOnScreen + this.height / 2 - 128)), new Rectangle?(new Rectangle(secretNoteImage * 64 % this.secretNoteImageTexture.Width, secretNoteImage * 64 / this.secretNoteImageTexture.Width * 64, 64, 64)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
                        b.Draw(this.secretNoteImageTexture, new Vector2((float)(this.xPositionOnScreen + this.width / 2 - 40), (float)(this.yPositionOnScreen + this.height / 2 - 192)), new Rectangle?(new Rectangle(193, 65, 14, 21)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.867f);
                    }
                    else
                    {
                        // Draw the mail content for the current mail page.
                        SpriteTextHelper.DrawString(b: b, s: mailMessage[page], x: this.xPositionOnScreen + 32, y: this.yPositionOnScreen + 32,
                            color: SpriteTextHelper.GetColorFromIndex(GetTextColor(whichBG)), characterPosition: 999999, 
                            width: this.width - 64, height: 999999, alpha: 0.75f, layerDepth: 0.865f,
                            drawBGScroll: -1, placeHolderScrollWidthText: "", textColorData);
                    }

                    // Draw the attached items, if any.
                    foreach (ClickableComponent clickableComponent in this.itemsToGrab)
                    {
                        b.Draw(this.letterTexture, clickableComponent.bounds, new Rectangle?(new Rectangle(whichBG * 24, 180, 24, 24)), Color.White);
                        if (clickableComponent.item != null)
                        {
                            Vector2 itemMailLocation = new Vector2(clickableComponent.bounds.X + 16, clickableComponent.bounds.Y + 16);
                            clickableComponent.item.drawInMenu(b, itemMailLocation, clickableComponent.scale);

                            // Missing "break" in original game code (at least up to version 1.3.36). Without it, attached items overdraw each other from left to right, 
                            // resulting in only the last attached item to be visible in the mail.
                            break;
                        }
                    }

                    if (moneyIncluded > 0)
                    {
                        string s = Game1.content.LoadString("Strings\\UI:LetterViewer_MoneyIncluded", moneyIncluded);
                        SpriteText.drawString(b, s, this.xPositionOnScreen + this.width / 2 - SpriteText.getWidthOfString(s, 999999) / 2, this.yPositionOnScreen + this.height - 96, 999999, -1, 9999, 0.75f, 0.865f, false, -1, "", -1);
                    }
                    else if (learnedRecipe != null && learnedRecipe.Length > 0)
                    {
                        string s = Game1.content.LoadString("Strings\\UI:LetterViewer_LearnedRecipe", cookingOrCrafting);
                        SpriteText.drawStringHorizontallyCenteredAt(b, s, this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen + this.height - 32 - SpriteText.getHeightOfString(s, 999999) * 2, 999999, this.width - 64, 9999, 0.65f, 0.865f, false, -1, 99999);
                        SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:LetterViewer_LearnedRecipeName", learnedRecipe), this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen + this.height - 32 - SpriteText.getHeightOfString("t", 999999), 999999, this.width - 64, 9999, 0.9f, 0.865f, false, -1, 99999);
                    }

                    //base.draw(b);
                    BaseDraw(b);

                    if (page < mailMessage.Count - 1)
                        this.forwardButton.draw(b);

                    if (page > 0)
                        this.backButton.draw(b);

                    if (questID != -1)
                    {
                        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.acceptQuestButton.bounds.X, this.acceptQuestButton.bounds.Y, this.acceptQuestButton.bounds.Width, this.acceptQuestButton.bounds.Height, (double)this.acceptQuestButton.scale > 1.0 ? Color.LightPink : Color.White, 4f * this.acceptQuestButton.scale, true);
                        Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2((float)(this.acceptQuestButton.bounds.X + 12), (float)(this.acceptQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12))), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
                    }
                }

                if (Game1.options.hardwareCursor)
                    return;

                b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), 
                    new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16)), 
                    Color.White, 0.0f, Vector2.Zero, (float)(4.0 + Game1.dialogueButtonScale / 150.0), 
                    SpriteEffects.None, 1f);
            }

            /// <summary>
            /// The <see cref="IClickableMenu.draw(SpriteBatch)"/> function.
            /// </summary>
            /// <param name="b">The sprite batch used to draw the content.</param>
            private void BaseDraw(SpriteBatch b)
            {
                this.upperRightCloseButton?.draw(b);
            }

            /// <summary>
            /// Draw the letter menu. Our implementation fixes a bug in the game where only the last item
            /// of the attached mail items is always drawn.
            /// </summary>
            /// <param name="b"></param>
            public /*override*/ void draw2(SpriteBatch b)
            {
                // Instead of copying over the complete original LetterViewerMenu.draw() function, we "overdraw" it 
                // with our fix applied. The reduces our dependency on the game code ( we would have to change our code
                // when the game code changes) and we also save a bunch of reflection calls (to access private class members).

                base.draw(b);

                var whichBG = this.whichBGRef.GetValue();
                var scale = this.scaleRef.GetValue();
                if (scale == 1.0)
                {
                    // The original game code (up to 1.3.36 at least) has a bug where, if there are multiple
                    // attached items, only the last item is always displayed. While the other items are in fact
                    // properly attached, they won't get drawn. We fix that bug here.
                    foreach (ClickableComponent clickableComponent in this.itemsToGrab)
                    {
                        if (clickableComponent.item != null)
                        {
                            b.Draw(this.letterTexture, clickableComponent.bounds, new Rectangle?(new Rectangle(whichBG * 24, 180, 24, 24)), Color.White);
                            clickableComponent.item.drawInMenu(b, new Vector2(clickableComponent.bounds.X + 16, clickableComponent.bounds.Y + 16), clickableComponent.scale);

                            // The original game code misses this "break" statement and thus overdraws all previously drawn items.
                            break;
                        }
                    }
                }

                // Since we "overdraw" the letter viewer menu, we also need to re-draw the game cursor as otherwise 
                // it would be behind the attached item spot. That means two cursors are being drawn now for the letter viewer menu
                // but visual testing still gave acceptable visual results (only really fast cursor movement shows a bit of a trailing
                // second cursor.)

                if (Game1.options.hardwareCursor)
                    return;

                b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), 
                    new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16)), 
                    Color.White, 0.0f, Vector2.Zero, (float)(4.0 + Game1.dialogueButtonScale / 150.0), SpriteEffects.None, 1f);
            }

            private int GetTextColor(int whichBG)
            {
                switch (whichBG)
                {
                    case 1:
                        return 8;
                    case 2:
                        return 7;
                    default:
                        return -1;
                }
            }
        }
    }
}
