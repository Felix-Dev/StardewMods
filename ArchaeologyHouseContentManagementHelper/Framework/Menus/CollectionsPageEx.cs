using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewMods.ArchaeologyHouseContentManagementHelper.Framework
{
    internal class CollectionsPageEx : CollectionsPage
    {
        public const int region_sideTabLostBooks = 7008;
        public new const int region_sideTabSecretNotes = 7007;

        public const int lostBooksTab = 7;

        private readonly int lostBooksTabPageIndex;

        // The number of side tabs of the Collection Page.
        private readonly int numTabs = 8;

        private readonly bool showSecretNotesTab;

        private const int BOOK_PREVIEW_LENGTH = 22;

        private readonly IReflectedField<string> hoverTextRef;
        private readonly IReflectedField<string> descriptionTextRef;
        private readonly IReflectedField<int> valueRef;

        public CollectionsPageEx(int x, int y, int width, int height, int selectedTab = organicsTab) : base(x, y, width, height)
        {
            hoverTextRef = ModEntry.CommonServices.ReflectionHelper.GetField<string>(this, "hoverText");
            descriptionTextRef = ModEntry.CommonServices.ReflectionHelper.GetField<string>(this, "descriptionText");
            valueRef = ModEntry.CommonServices.ReflectionHelper.GetField<int>(this, "value");

            showSecretNotesTab = Game1.player.secretNotesSeen.Count > 0;
            lostBooksTabPageIndex = showSecretNotesTab ? lostBooksTab : lostBooksTab - 1;

            Texture2D bookTabTexture = ModEntry.CommonServices.ContentHelper.Load<Texture2D>("Assets/CollectionTab_LostBook.png", ContentSource.ModFolder);


            // side-tab [Lost Books]
            ClickableTextureComponent stLostBooks = new ClickableTextureComponent(
                name: "", 
                bounds: new Rectangle(this.xPositionOnScreen - 48, this.yPositionOnScreen + (showSecretNotesTab ? 576 : 512), 64, 64), 
                label: "", 
                hoverText: ModEntry.CommonServices.TranslationHelper.Get("GameMenu_CollectionsPage_LostBookTabLabel"), 
                texture: bookTabTexture, 
                sourceRect: new Rectangle(0, 0, 16, 16),
                scale: 4f, 
                drawShadow: false)
            {
                myID = region_sideTabLostBooks,
                upNeighborID = showSecretNotesTab ? region_sideTabSecretNotes : region_sideTabAchivements,
                downNeighborID = 0,
                rightNeighborID = 0
            };

            this.sideTabs.Add(stLostBooks);
            this.collections.Add(lostBooksTabPageIndex, new List<List<ClickableTextureComponent>>());

            var prevSideTab = this.sideTabs[showSecretNotesTab ? secretNotesTab : achievementsTab];
            prevSideTab.downNeighborID = region_sideTabLostBooks;

            // Fill [Lost Book] collection

            this.collections[lostBooksTabPageIndex].Add(new List<ClickableTextureComponent>());

            var lostBooksIndices = LibraryMuseumHelper.GetLostBookIndexList();

            // Add lost books to the [Lost Books] side tab.

            int booksInCurrentPage = 0;
            int startPosX = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
            int startPosY = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;
            int num3 = 8; // The number of lost books per row

            for (int i = 0; i < LibraryMuseumHelper.TotalLibraryBooks; i++)
            {
                bool drawShadow = false;

                // If the current Lost Book has already been discovered by the player, "enable" it in the collection
                if (lostBooksIndices[i] <= LibraryMuseumHelper.LibraryBooks)
                {
                    drawShadow = true;
                }

                // Start a new page if the current page has already been filled completely
                int x1 = startPosX + booksInCurrentPage % num3 * 85;
                int y1 = startPosY + booksInCurrentPage / num3 * 85;
                if (y1 > this.yPositionOnScreen + height - 128)
                {
                    this.collections[lostBooksTabPageIndex].Add(new List<ClickableTextureComponent>());
                    booksInCurrentPage = 0;
                    x1 = startPosX;
                    y1 = startPosY;
                }

                // Add the lost book texture to the collection
                var textureComponentList = this.collections[lostBooksTabPageIndex].Last();
                ClickableTextureComponent lostBookTextureObject = new ClickableTextureComponent(
                    name: Constants.GAME_OBJECT_LOST_BOOK_ID.ToString() + " " + drawShadow.ToString() + " " + lostBooksIndices[i], 
                    bounds: new Rectangle(x1, y1, 64, 64), 
                    label: (string)null, 
                    hoverText: "", 
                    texture: Game1.objectSpriteSheet, 
                    sourceRect: Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, Constants.GAME_OBJECT_LOST_BOOK_ID, 16, 16), 
                    scale: 4f, 
                    drawShadow: drawShadow)
                {
                    myID = this.collections[lostBooksTabPageIndex].Last().Count,
                    rightNeighborID = (this.collections[lostBooksTabPageIndex].Last().Count + 1) % num3 == 0 ? -1 : this.collections[lostBooksTabPageIndex].Last().Count + 1,
                    leftNeighborID = this.collections[lostBooksTabPageIndex].Last().Count % num3 == 0 ? 7001 : this.collections[lostBooksTabPageIndex].Last().Count - 1,
                    downNeighborID = y1 + 85 > this.yPositionOnScreen + height - 128 ? -7777 : this.collections[lostBooksTabPageIndex].Last().Count + num3,
                    upNeighborID = this.collections[lostBooksTabPageIndex].Last().Count < num3 ? 12345 : this.collections[lostBooksTabPageIndex].Last().Count - num3,
                    fullyImmutable = true
                };

                textureComponentList.Add(lostBookTextureObject);
                ++booksInCurrentPage;
            }

            SetCurrentSidetab();

            void SetCurrentSidetab()
            {
                // Set the current side tab
                if (selectedTab < 0 || selectedTab >= numTabs)
                {
                    selectedTab = 0;
                }

                // [Lost Books] tab has a different Collection Page index depending on the visibility
                // of the [Secret Notes] tab.
                if (selectedTab == lostBooksTab)
                {
                    selectedTab = lostBooksTabPageIndex;
                }

                ModEntry.CommonServices.ReflectionHelper.GetField<int>(this, "currentTab").SetValue(selectedTab);

                /* 
                 * On default creation, the selected side tab is the first side tab. Our custom CollectionsPage
                 * sometimes needs to set a different tab as the selected tab (i.e. when returning from reading a book).
                 */
                if (selectedTab != 0)
                {
                    this.sideTabs[0].bounds.X -= CollectionsPage.widthToMoveActiveTab;
                    this.sideTabs[selectedTab].bounds.X += CollectionsPage.widthToMoveActiveTab;
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            IReflectedField<int> secretNoteImageRef = ModEntry.CommonServices.ReflectionHelper.GetField<int>(this, "secretNoteImage");

            int currentTab = ModEntry.CommonServices.ReflectionHelper.GetField<int>(this, "currentTab").GetValue();
            int currentPage = ModEntry.CommonServices.ReflectionHelper.GetField<int>(this, "currentPage").GetValue();

            descriptionTextRef.SetValue("");
            hoverTextRef.SetValue("");
            valueRef.SetValue(-1);
            secretNoteImageRef.SetValue(-1);

            if (currentTab == lostBooksTabPageIndex)
            {
                foreach (ClickableTextureComponent textureComponent in this.collections[currentTab][currentPage])
                {
                    if (textureComponent.containsPoint(x, y))
                    {
                        textureComponent.scale = Math.Min(textureComponent.scale + 0.02f, textureComponent.baseScale + 0.1f);

                        // Draw [unknown] tooltip if item hasn't been encountered yet
                        if (!Convert.ToBoolean(textureComponent.name.Split(' ')[1]))
                        {
                            hoverTextRef.SetValue("???");
                            continue;
                        }

                        // Book has already been found -> show book preview
                        string index = textureComponent.name.Split(' ')[2];
                        string message = Game1.content.LoadString("Strings\\Notes:" + index).Replace('\n', '^');

                        string title = message.Split('^')[0].Trim();
                        if (title.Length > BOOK_PREVIEW_LENGTH)
                        {
                            title = title.Substring(0, BOOK_PREVIEW_LENGTH) + "...";
                        }

                        // Set hover text to book content preview.
                        hoverTextRef.SetValue(title);
                        continue;
                    }
                    else
                    {
                        textureComponent.scale = Math.Max(textureComponent.scale - 0.02f, textureComponent.baseScale);
                    }
                }
            }
            else
            {
                base.performHoverAction(x, y);
            }
        }

        //public string createDescription(int index)
        //{
        //    string str1 = "";
        //    if (this.currentTab == 5)
        //    {
        //        string[] strArray = Game1.achievements[index].Split('^');
        //        str1 = str1 + strArray[0] + Environment.NewLine + Environment.NewLine + strArray[1];
        //    }
        //    else if (this.currentTab == 6)
        //    {
        //        var secretNotesData = ModEntry.CommonServices.ReflectionHelper.GetField<Dictionary<int, string>>(this, "secretNotesData").GetValue();
        //        var secretNoteImageRef = ModEntry.CommonServices.ReflectionHelper.GetField<int>(this, "secretNoteImage");

        //        if (secretNotesData != null)
        //        {
        //            str1 = str1 + Game1.content.LoadString("Strings\\Locations:Secret_Note_Name") + " #" + (object)index;
        //            if (secretNotesData[index][0] == '!')
        //            {
        //                secretNoteImageRef.SetValue(Convert.ToInt32(secretNotesData[index].Split(' ')[1]));
        //            }
        //            else
        //            {
        //                str1 = str1 + Environment.NewLine + Environment.NewLine +
        //                    Game1.parseText(secretNotesData[index].TrimStart(' ', '^').Replace("^", Environment.NewLine).Replace("@", Game1.player.Name), Game1.smallFont, 512);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        string[] strArray = Game1.objectInformation[index].Split('/');
        //        string str2 = strArray[4];
        //        string str3 = str1 + str2 + Environment.NewLine + Environment.NewLine + Game1.parseText(strArray[5], Game1.smallFont, 256) + Environment.NewLine + Environment.NewLine;
        //        if (strArray[3].Contains("Arch"))
        //        {
        //            str1 = str3 +
        //                (Game1.player.archaeologyFound.ContainsKey(index) ?
        //                    Game1.content.LoadString("Strings\\UI:Collections_Description_ArtifactsFound", (object)Game1.player.archaeologyFound[index][0])
        //                    : "");
        //        }
        //        else if (strArray[3].Contains("Cooking"))
        //        {
        //            str1 = str3 +
        //                (Game1.player.recipesCooked.ContainsKey(index) ?
        //                    Game1.content.LoadString("Strings\\UI:Collections_Description_RecipesCooked", (object)Game1.player.recipesCooked[index])
        //                    : "");
        //        }
        //        else if (strArray[3].Contains("Fish"))
        //        {
        //            str1 = str3 +
        //                Game1.content.LoadString("Strings\\UI:Collections_Description_FishCaught", (object)(Game1.player.fishCaught.ContainsKey(index) ?
        //                Game1.player.fishCaught[index][0]
        //                : 0));

        //            if (Game1.player.fishCaught.ContainsKey(index) && Game1.player.fishCaught[index][1] > 0)
        //                str1 = str1 + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Collections_Description_BiggestCatch", (object)(LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en ? Math.Round((double)Game1.player.fishCaught[index][1] * 2.54) : (double)Game1.player.fishCaught[index][1]));
        //        }
        //        else
        //        {
        //            str1 = strArray[3].Contains("Minerals") || strArray[3].Substring(strArray[3].Length - 3).Equals("-2") ? str3 + Game1.content.LoadString("Strings\\UI:Collections_Description_MineralsFound", (object)(Game1.player.mineralsFound.ContainsKey(index) ? Game1.player.mineralsFound[index] : 0)) : str3 + Game1.content.LoadString("Strings\\UI:Collections_Description_NumberShipped", (object)(Game1.player.basicShipped.ContainsKey(index) ? Game1.player.basicShipped[index] : 0));
        //        }

        //        this.value = Convert.ToInt32(strArray[1]);
        //    }
        //    return str1;
        //}

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            var currentTabRef = ModEntry.CommonServices.ReflectionHelper.GetField<int>(this, "currentTab");
            var currentPageRef = ModEntry.CommonServices.ReflectionHelper.GetField<int>(this, "currentPage");

            int currentTab = currentTabRef.GetValue();
            int currentPage = currentPageRef.GetValue();

            for (int index = 0; index < this.sideTabs.Count; ++index)
            {
                if (this.sideTabs[index].containsPoint(x, y) && currentTab != index)
                {
                    Game1.playSound("smallSelect");
                    this.sideTabs[currentTab].bounds.X -= CollectionsPage.widthToMoveActiveTab;

                    currentTabRef.SetValue(index);
                    currentPageRef.SetValue(0);
                    currentTab = index;
                    currentPage = 0;

                    this.sideTabs[index].bounds.X += CollectionsPage.widthToMoveActiveTab;

                    return;
                }
            }

            // Open a book when it has been clicked by the player.
            if (currentTab == lostBooksTabPageIndex)
            {
                foreach (ClickableTextureComponent textureComponent in collections[lostBooksTabPageIndex][currentPage])
                {
                    if (textureComponent.containsPoint(x, y))
                    {
                        // Only open a book if it has already been found by the player.
                        if (int.TryParse(textureComponent.name.Split(' ')[2], out int index) 
                            && index <= LibraryMuseumHelper.LibraryBooks)
                        {
                            string message = Game1.content.LoadString("Strings\\Notes:" + index).Replace('\n', '^');
                            Game1.drawLetterMessage(message);
                        }
                        return;
                    }
                }
            }           

            if (currentPage > 0 && this.backButton.containsPoint(x, y))
            {
                --currentPage;
                currentPageRef.SetValue(currentPage);

                Game1.playSound("shwip");
                this.backButton.scale = this.backButton.baseScale;
                if (Game1.options.snappyMenus && Game1.options.gamepadControls && currentPage == 0)
                {
                    this.currentlySnappedComponent = (ClickableComponent)this.forwardButton;
                    Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
                }
            }

            if (currentPage >= this.collections[currentTab].Count - 1 || !this.forwardButton.containsPoint(x, y))
            {
                return;
            }

            ++currentPage;
            currentPageRef.SetValue(currentPage);

            Game1.playSound("shwip");
            this.forwardButton.scale = this.forwardButton.baseScale;
            if (!Game1.options.snappyMenus || !Game1.options.gamepadControls || currentPage != this.collections[currentTab].Count - 1)
            {
                return;
            }

            this.currentlySnappedComponent = (ClickableComponent)this.backButton;
            Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
        }

        //public override void draw(SpriteBatch b)
        //{
        //    int value = ModEntry.CommonServices.ReflectionHelper.GetField<int>(this, "value").GetValue();
        //    int secretNoteImage = ModEntry.CommonServices.ReflectionHelper.GetField<int>(this, "secretNoteImage").GetValue();

        //    Texture2D secretNoteImageTexture = ModEntry.CommonServices.ReflectionHelper.GetField<Texture2D>(this, "secretNoteImageTexture").GetValue();

        //    foreach (ClickableTextureComponent sideTab in sideTabs)
        //    {
        //        sideTab.draw(b);
        //    }

        //    if (currentPage > 0)
        //    {
        //        this.backButton.draw(b);
        //    }

        //    if (currentPage < collections[currentTab].Count - 1)
        //    {
        //        this.forwardButton.draw(b);
        //    }

        //    b.End();
        //    b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);

        //    foreach (ClickableTextureComponent textureComponent in collections[currentTab][currentPage])
        //    {
        //        bool boolean = Convert.ToBoolean(textureComponent.name.Split(' ')[1]);
        //        textureComponent.draw(b, boolean ? Color.White : Color.Black * 0.2f, 0.86f);
        //        if (currentTab == 5 & boolean)
        //        {
        //            int num = new Random(Convert.ToInt32(textureComponent.name.Split(' ')[0])).Next(12);
        //            b.Draw(Game1.mouseCursors, new Vector2((float)(textureComponent.bounds.X + 16 + 16), (float)(textureComponent.bounds.Y + 20 + 16)), new Rectangle?(new Rectangle(256 + num % 6 * 64 / 2, 128 + num / 6 * 64 / 2, 32, 32)), Color.White, 0.0f, new Vector2(16f, 16f), textureComponent.scale, SpriteEffects.None, 0.88f);
        //        }
        //    }

        //    b.End();
        //    b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);

        //    if (this.hoverText.Equals(""))
        //    {
        //        return;
        //    }

        //    IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, value, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);
        //    if (secretNoteImage == -1)
        //    {
        //        return;
        //    }

        //    IClickableMenu.drawTextureBox(b, Game1.getOldMouseX(), Game1.getOldMouseY() + 64 + 32, 288, 288, Color.White);
        //    b.Draw(secretNoteImageTexture, new Vector2((float)(Game1.getOldMouseX() + 16), (float)(Game1.getOldMouseY() + 64 + 32 + 16)), new Rectangle?(new Rectangle(secretNoteImage * 64 % secretNoteImageTexture.Width, secretNoteImage * 64 / secretNoteImageTexture.Width * 64, 64, 64)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
        //}
    }
}
