using FelixDev.StardewMods.FeTK.Framework.Data.Parsers;
using FelixDev.StardewMods.FeTK.Framework.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Text;

namespace FelixDev.StardewMods.FeTK.Framework.UI
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
        /// Create a new instance of the <see cref="LetterViewerMenuWrapper"/> class.
        /// </summary>
        /// <param name="mailTitle">The title of the mail.</param>
        /// <param name="mailContent">The content of the mail.</param>
        /// <returns>The created <see cref="LetterViewerMenuWrapper"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="mailTitle"/> is <c>null</c> -or-
        /// the specified <paramref name="mailContent"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Use this function if the content of the mail is defined by the game's mail content format.
        /// Example: "mail content %item object 388 50 %%"
        /// </remarks>
        public static LetterViewerMenuWrapper CreateMenuForGameMail(string mailTitle, string mailContent)
        {
            if (mailTitle == null)
            {
                throw new ArgumentNullException(nameof(mailTitle));
            }

            if (mailContent == null)
            {
                throw new ArgumentNullException(nameof(mailContent));
            }

            return new LetterViewerMenuWrapper(true, mailTitle, mailContent);
        }

        /// <summary>
        /// Create a new instance of the <see cref="LetterViewerMenuWrapper"/> class.
        /// </summary>
        /// <param name="mailTitle">The title of the mail.</param>
        /// <param name="mailContent">The content of the mail.</param>
        /// <param name="attachedItems">The attached items of the mail, if any. Can be <c>null</c>.</param>
        /// <returns>The created <see cref="LetterViewerMenuWrapper"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="mailTitle"/> is <c>null</c> -or-
        /// the specified <paramref name="mailContent"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Use this function if the content of the mail is to be specified independently from the
        /// game's mail content format.
        /// </remarks>
        public static LetterViewerMenuWrapper CreateMenuForFrameworkMail(string mailTitle, string mailContent, List<Item> attachedItems = null)
        {
            if (mailTitle == null)
            {
                throw new ArgumentNullException(nameof(mailTitle));
            }

            if (mailContent == null)
            {
                throw new ArgumentNullException(nameof(mailContent));
            }

            return new LetterViewerMenuWrapper(false, mailTitle, mailContent, attachedItems);
        }

        /// <summary>
        /// Create a new instance of the <see cref="LetterViewerMenuWrapper"/> class./>
        /// </summary>
        /// <param name="usesGameFormat">
        /// Specifies whether the mail's content is specified by following the game's mail format or
        /// specified independently from it.
        /// </param>
        /// <param name="mailTitle">The title of mail to display.</param>
        /// <param name="mailContent">The content of the mail to display.</param>
        /// <param name="attachedItems">The attached items of the mail to display. May be <c>null</c>.</param>
        private LetterViewerMenuWrapper(bool usesGameFormat, string mailTitle, string mailContent, List<Item> attachedItems = null)
        {
            letterMenu = usesGameFormat
                ? new LetterViewerMenuEx(mailTitle, mailContent)
                : new LetterViewerMenuEx(mailTitle, mailContent.Equals(string.Empty) ? " " : mailContent, attachedItems);

            letterMenu.exitFunction = new IClickableMenu.onExit(OnExit);
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

            private IReflectedMethod getTextColorRef;

            #endregion // LetterViewerMenu Reflection Fields

            /// <summary>Contains a collection of <see cref="TextColorInfo"/> objects, if any, for the specified mail content.</summary>
            private List<List<TextColorInfo>> textColorDataPerPage;

            /// <summary>The default text color to use for the mail's text content.</summary>
            private Color textColor;

            /// <summary>
            /// Create an instance of the <see cref="LetterViewerMenuEx"/> class.
            /// </summary>
            /// <param name="title">The title of the mail.</param>
            /// <param name="content">the content of the mail.</param>
            /// <remarks>
            /// Use this constructor if the content of the mail is defined by the game's mail content format.
            /// Example: "mail content %item object 388 50 %%"
            /// </remarks>
            public LetterViewerMenuEx(string title, string content)
            : base(content, title)
            {
                SetupReflectionAndContent(null);
            }

            /// <summary>
            /// Create an instance of the <see cref="LetterViewerMenuEx"/> class.
            /// </summary>
            /// <param name="title">The title of the mail.</param>
            /// <param name="content">The content of the mail.</param>
            /// <param name="attachedItems">The items attached to the mail. Can be <c>null</c>.</param>
            /// <remarks>
            /// Use this constructor if the content of the mail is to be specified independently from the
            /// game's mail content format.
            /// </remarks>
            public LetterViewerMenuEx(string title, string content, List<Item> attachedItems) 
                : base(content)
            {
                SetupReflectionAndContent(content);

                reflectionHelper
                    .GetField<bool>(this, "isMail")
                    .SetValue(true);
                reflectionHelper
                    .GetField<string>(this, "mailTitle")
                    .SetValue(title);

                MailTitle = title;

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
            }

            /// <summary>The title of the mail.</summary>
            public string MailTitle { get; private set; }

            /// <summary>A list containing the selected items.</summary>
            public List<Item> SelectedItems { get; private set; }

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
                        SelectedItems?.Add(clickableComponent.item);

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
                            color: textColor, characterPosition: 999999, 
                            width: this.width - 64, height: 999999, alpha: 0.75f, layerDepth: 0.865f,
                            drawBGScroll: -1, placeHolderScrollWidthText: "", textColorDataPerPage?[page]);
                    }

                    // Draw the attached items, if any.
                    foreach (ClickableComponent clickableComponent in this.itemsToGrab)
                    {
                        b.Draw(this.letterTexture, clickableComponent.bounds, new Rectangle?(new Rectangle(whichBG * 24, 180, 24, 24)), Color.White);
                        if (clickableComponent.item != null)
                        {
                            Vector2 itemMailLocation = new Vector2(clickableComponent.bounds.X + 16, clickableComponent.bounds.Y + 16);
                            clickableComponent.item.drawInMenu(b, itemMailLocation, clickableComponent.scale);

                            // Missing "break" in original game code (at least up to version 1.3.36). Without it, attached items will overdraw each other 
                            // from first to last, resulting in only the last attached item to be visible in the mail as long as there are any remaining
                            // attached items.
                            break;
                        }
                    }

                    // Draw the amount of attached money, if any.
                    if (moneyIncluded > 0)
                    {
                        string s = Game1.content.LoadString("Strings\\UI:LetterViewer_MoneyIncluded", moneyIncluded);
                        SpriteText.drawString(b, s, this.xPositionOnScreen + this.width / 2 - SpriteText.getWidthOfString(s, 999999) / 2, this.yPositionOnScreen + this.height - 96, 999999, -1, 9999, 0.75f, 0.865f, false, -1, "", -1);
                    }

                    // Draw the attached recipe, if any. 
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

                    // Draw the [Accept Quest] button if this is a quest mail.
                    if (questID != -1)
                    {
                        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.acceptQuestButton.bounds.X, this.acceptQuestButton.bounds.Y, this.acceptQuestButton.bounds.Width, this.acceptQuestButton.bounds.Height, (double)this.acceptQuestButton.scale > 1.0 ? Color.LightPink : Color.White, 4f * this.acceptQuestButton.scale, true);
                        Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2((float)(this.acceptQuestButton.bounds.X + 12), (float)(this.acceptQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12))), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
                    }
                }

                if (Game1.options.hardwareCursor)
                    return;

                // Draw the mouse cursor.
                b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()),
                    new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16)),
                    Color.White, 0.0f, Vector2.Zero, (float)(4.0 + Game1.dialogueButtonScale / 150.0),
                    SpriteEffects.None, 1f);
            }

            private void SetupReflectionAndContent(string content = null)
            {
                void SetupReflection()
                {
                    // private fields

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

                    // private methods

                    getTextColorRef = reflectionHelper
                        .GetMethod(this, "getTextColor");
                }
                SetupReflection();

                // Retrieve the game-parsed mail content if content is not set. Since the mail content has already
                // been potentially sliced up into multiple pages, we need to combine those pages again to get the
                // complete mail content so we can successfully parse the content for the text coloring API.
                if (content == null)
                {
                    List<string> mailMessage = mailMessageRef.GetValue();
                    if (mailMessage.Count > 1)
                    {
                        StringBuilder contentBuilder = new StringBuilder();
                        mailMessage.ForEach(page => contentBuilder.Append(page));

                        content = contentBuilder.ToString();
                    }
                    else
                    {
                        content = mailMessage[0];
                    }
                }

                // Check if the mail content uses the text coloring API and parse it accordingly.
                bool couldParse = StringColorParser.TryParse(content, SpriteTextHelper.GetColorFromIndex(getTextColorRef.Invoke<int>()), out List<TextColorInfo> textColorData);
                if (couldParse)
                {
                    // Construct the new mail content with all <color> tags removed.
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

                    List<string> mailMessage = mailMessageRef.GetValue();

                    // If the mail content did not contain any <color> tags, we set the default mail-content text color 
                    // based on the mail's background.
                    if (parsedString.Length == content.Length)
                    {
                        textColor = SpriteTextHelper.GetColorFromIndex(getTextColorRef.Invoke<int>());
                        return;
                    }

                    // If the mail content contained a single pair of <color> tags which enclosed the entire actual 
                    // mail content, the entire mail content will be drawn in the same color, that is the color specified
                    // by the color tag.
                    // Example: <color=#0000FF>mail content</color>
                    else if (textColorData.Count == 1)
                    {
                        textColor = textColorData[0].Color;
                    }

                    // If the mail content is to be drawn in at least two different colors and the mail content has been sliced up
                    // into multiple pages, we also need to "slice up" our TextColorInfo data so that each content page will only 
                    // contain the TextColorInfo data relevant to it.
                    else
                    {
                        textColorDataPerPage = new List<List<TextColorInfo>>(mailMessage.Count);
                        for (int i = 0; i < mailMessage.Count; i++)
                        {
                            textColorDataPerPage.Add(new List<TextColorInfo>());
                        }

                        int currentBlockIndex = 0; // The current TextColorInfo block we are assigning to a content page.
                        int currentIndexInBlock = 0; // The current index into the current TextColorInfo block.

                        for (int i = 0; i < mailMessage.Count; i++)
                        {
                            // As long as there are still page characters left which have not yet been assigned a TextColorInfo (sub)block to,
                            // we continue to assign TextColorInfo blocks to the page content.
                            int remainingCharsPerPage = mailMessage[i].Length;
                            while (remainingCharsPerPage > 0)
                            {
                                // A TextColorInfo block can contain a string which is shorter, of same length or longer than the remaining 
                                // unassigned content of the current mail page. In the first two cases (shorter or of same length) we assign 
                                // all the unassigned content of the current TextColorInfo block (some of its text data could have already  
                                // been assigned to a page -- see second case below) to the current page.
                                //
                                // In the second case, the current TextColorInfo block spans multiple content pages and we thus have to split it 
                                // up into multiple sub TextColorInfo blocks (one block per page the TextColorInfo block is spanning). 
                                // Splitting up means that one part of the TextColorInfo block will be assigned to a different page than the rest of 
                                // the TextColorInfo block. We keep track of the TextColorInfo parts which are unassgined yet using "currentIndexInBlock".

                                // First case, the unassigned part of the current TextColorInfo block fits into the remaining content
                                // of the current page.
                                // Note: Since a TextColorInfo block can potentially span more than two pages, we also have to make 
                                // sure to ignore any already assigned parts of the current TextColorInfo block.
                                if (textColorData[currentBlockIndex].Text.Length - currentIndexInBlock <= remainingCharsPerPage)
                                {
                                    string blockText = (currentIndexInBlock > 0)
                                        ? textColorData[currentBlockIndex].Text.Substring(currentIndexInBlock)
                                        : textColorData[currentBlockIndex].Text;

                                    textColorDataPerPage[i].Add(new TextColorInfo(blockText, textColorData[currentBlockIndex].Color));

                                    remainingCharsPerPage -= textColorData[currentBlockIndex].Text.Length - currentIndexInBlock;

                                    currentBlockIndex++;
                                    currentIndexInBlock = 0;
                                }

                                // Second case, the unassigned part of the current TextColorInfo block spans at least two pages:
                                // Split it up into an unassigned part for the current page and a remaining unassigned part for
                                // the next page(s). Then assign the first part to the current page.
                                // Note: Since a TextColorInfo block can potentially span more than two pages, we also have to make 
                                // sure to ignore any already assigned parts of the current TextColorInfo block.
                                else
                                {
                                    string splitBlockText = textColorData[currentBlockIndex].Text.Substring(currentIndexInBlock, remainingCharsPerPage);

                                    textColorDataPerPage[i].Add(new TextColorInfo(splitBlockText, textColorData[currentBlockIndex].Color));

                                    currentIndexInBlock += remainingCharsPerPage;
                                    remainingCharsPerPage = 0;                                
                                }
                            }
                        }
                    }

                    // We potentially changed the number of pages the mail content has been split up
                    // after the content was parsed by the text coloring parser. Hence we might need to
                    // update the [Back Button] and [Forward Button] settings. 
                    if (Game1.options.SnappyMenus && mailMessage?.Count <= 1)
                    {
                        this.backButton.myID = -100;
                        this.forwardButton.myID = -100;
                    }
                }
            }

            /// <summary>
            /// The <see cref="IClickableMenu.draw(SpriteBatch)"/> function.
            /// </summary>
            /// <param name="b">The sprite batch used to draw the content.</param>
            private void BaseDraw(SpriteBatch b)
            {
                this.upperRightCloseButton?.draw(b);
            }
        }
    }
}
