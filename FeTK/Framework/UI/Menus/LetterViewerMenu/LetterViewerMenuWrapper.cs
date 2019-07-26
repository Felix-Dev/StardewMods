using FelixDev.StardewMods.FeTK.Framework.Data.Parsers;
using FelixDev.StardewMods.FeTK.Framework.Services;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

        /// <summary>The <see cref="LetterViewerMenuEx2"/> instance used to display the mail.</summary>
        private readonly LetterViewerMenuEx2 letterMenu;

        /// <summary>Raised when the letter viewer menu has been closed.</summary>
        public event EventHandler<LetterViewerMenuClosedEventArgs> MenuClosed;

        /// <summary>
        /// Create a new instance of the <see cref="LetterViewerMenuWrapper"/> class.
        /// </summary>
        /// <param name="mail">The mail to create the menu for.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="mail"/> is <c>null</c>.</exception>
        public LetterViewerMenuWrapper(Mail mail)
        {
            if (mail == null)
            {
                throw new ArgumentNullException(nameof(mail));
            }

            string textContent = mail.Text.Equals(string.Empty) ? " " : mail.Text;

            switch (mail)
            {
                case ItemMail itemMail:
                    this.letterMenu =  LetterViewerMenuEx2.CreateItemMailMenu(itemMail.Id, textContent, itemMail.AttachedItems);
                    break;
                case MoneyMail moneyMail:
                    this.letterMenu = LetterViewerMenuEx2.CreateMoneyMailMenu(moneyMail.Id, textContent, moneyMail.AttachedMoney);
                    break;
                case RecipeMail recipeMail:
                    this.letterMenu = LetterViewerMenuEx2.CreateRecipeMailMenu(recipeMail.Id, textContent, recipeMail.RecipeName, recipeMail.RecipeType);
                    break;
                case QuestMail questMail:
                    this.letterMenu = LetterViewerMenuEx2.CreateQuestMailMenu(questMail.Id, textContent, questMail.QuestId, questMail.IsAutomaticallyAccepted);
                    break;
                default:
                    this.letterMenu = new LetterViewerMenuEx2(mail.Id, textContent);
                    break;
            }

            this.letterMenu.exitFunction = new IClickableMenu.onExit(OnExit);
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
            MenuClosed?.Invoke(this, new LetterViewerMenuClosedEventArgs(letterMenu.MailId, letterMenu.InteractionRecord));
        }

        /// <summary>
        /// This class extends the <see cref="LetterViewerMenuEx"/> class with additional functionality such as 
        ///         - keeping track of user interaction with the mail's content
        /// </summary>
        private class LetterViewerMenuEx2 : LetterViewerMenuEx
        {
            /// <summary>Provides access to the <see cref="IMonitor"/> API provided by SMAPI.</summary>
            private static readonly IMonitor monitor = ToolkitMod._Monitor;

            /// <summary>The type of the mail visualized by this menu.</summary>
            private MailType mailType;

            /// <summary>The ID of the quest included in the mail.</summary>
            /// <remarks>
            /// This differs from <see cref="LetterViewerMenuEx.QuestId"/> in that it will always hold the ID of the quest
            /// this menu was created with.
            /// </remarks>
            private int attachedQuestId = QUEST_ID_NO_QUEST;

            /// <summary>Indicates whether the quest included in the mail was accepted or not.</summary>
            private bool questAccepted = false;

            /// <summary>Contains the attached items which were selected by the player.</summary>
            private List<Item> selectedItems;

            /// <summary>
            /// Create a new instance of the <see cref="LetterViewerMenuEx2"/> class.
            /// </summary>
            /// <param name="id">The ID of the mail.</param>
            /// <param name="text">The text content of the mail.</param>
            public LetterViewerMenuEx2(string id, string text)
                : base(text)
            {
                reflectionHelper
                    .GetField<bool>(this, "isMail")
                    .SetValue(true);
                reflectionHelper
                    .GetField<string>(this, "mailTitle")
                    .SetValue(id);

                MailId = id;

                this.mailType = MailType.PlainMail;
            }

            /// <summary>
            /// Create a new instance of the <see cref="LetterViewerMenuEx2"/> class.
            /// </summary>
            /// <param name="id">The ID of the mail.</param>
            /// <param name="text">The text content of the mail.</param>
            /// <param name="attachedItems">The items attached to the mail. Can be <c>null</c>.</param>
            /// <returns>The created <see cref="LetterViewerMenuEx2"/> instance.</returns>
            public static LetterViewerMenuEx2 CreateItemMailMenu(string id, string text, List<Item> attachedItems)
            {
                var menu = new LetterViewerMenuEx2(id, text)
                {
                    selectedItems = new List<Item>(),
                    mailType = MailType.ItemMail
                };

                // If the mail has attached items, add them to the LetterViewerMenu so they will be shown when the
                // mail is drawn to the screen.
                if (attachedItems?.Count > 0)
                {
                    foreach (var item in attachedItems)
                    {
                        var attachedItemComponent = new ClickableComponent(
                            new Rectangle(menu.xPositionOnScreen + menu.width / 2 - 48, menu.yPositionOnScreen + menu.height - 32 - 96, 96, 96),
                            item)
                        {
                            myID = region_itemGrabButton,
                            leftNeighborID = region_backButton,
                            rightNeighborID = region_forwardButton
                        };

                        menu.itemsToGrab.Add(attachedItemComponent);
                    }

                    menu.backButton.rightNeighborID = region_itemGrabButton;
                    menu.forwardButton.leftNeighborID = region_itemGrabButton;

                    if (!Game1.options.SnappyMenus)
                        return menu;

                    menu.populateClickableComponentList();
                    menu.snapToDefaultClickableComponent();
                }

                return menu;
            }

            /// <summary>
            /// Create a new instance of the <see cref="LetterViewerMenuEx2"/> class.
            /// </summary>
            /// <param name="id">The ID of the mail.</param>
            /// <param name="text">The text content of the mail.</param>
            /// <param name="money">The money attached to the mail.</param>
            /// <returns>The created <see cref="LetterViewerMenuEx2"/> instance.</returns>
            public static LetterViewerMenuEx2 CreateMoneyMailMenu(string id, string text, int money)
            {
                var menu = new LetterViewerMenuEx2(id, text)
                {
                    mailType = MailType.MoneyMail
                };

                // Attach money to the mail and add it to the player's account.
                menu.MoneyIncluded = money;
                Game1.player.Money += money;

                return menu;
            }

            /// <summary>
            /// Create a new instance of the <see cref="LetterViewerMenuEx2"/> class.
            /// </summary>
            /// <param name="id">The ID of the mail.</param>
            /// <param name="text">The text content of the mail.</param>
            /// <param name="questId">The ID of the quest included in the mail.</param>
            /// <param name="isAutomaticallyAccepted">
            /// Indicates whether the included quest is automatically accepted when the mail is opened or if the 
            /// player needs to manually accept it.
            /// </param>
            /// <returns>The created <see cref="LetterViewerMenuEx2"/> instance.</returns>
            public static LetterViewerMenuEx2 CreateQuestMailMenu(string id, string text, int questId, bool isAutomaticallyAccepted)
            {
                var menu = new LetterViewerMenuEx2(id, text)
                {
                    mailType = MailType.QuestMail,
                    attachedQuestId = questId < 1 ? QUEST_ID_NO_QUEST : questId,
                };

                // If the ID does not represent an existing quest, we don't include it in the mail.
                if (menu.attachedQuestId == QUEST_ID_NO_QUEST)
                {
                    menu.QuestId = QUEST_ID_NO_QUEST;
                    return menu;
                }

                // Add the quest to the player's quest log if it is an automatically accepted quest.
                if (isAutomaticallyAccepted)
                {
                    Game1.player.addQuest(questId);

                    menu.questAccepted = true;
                    menu.QuestId = QUEST_ID_NO_QUEST;

                    return menu;
                }

                // Specified quest has to be manually accepted by the player -> setup [quest accept] button in the menu.

                menu.QuestId = questId;

                string label = Game1.content.LoadString("Strings\\UI:AcceptQuest");
                menu.acceptQuestButton = new ClickableComponent(
                    new Rectangle(menu.xPositionOnScreen + menu.width / 2 - 128, menu.yPositionOnScreen + menu.height - 128, 
                                 (int)Game1.dialogueFont.MeasureString(label).X + 24, 
                                 (int)Game1.dialogueFont.MeasureString(label).Y + 24),
                                 "")
                {
                    myID = region_acceptQuestButton,
                    rightNeighborID = region_forwardButton,
                    leftNeighborID = region_backButton
                };

                menu.backButton.rightNeighborID = region_acceptQuestButton;
                menu.forwardButton.leftNeighborID = region_acceptQuestButton;

                if (!Game1.options.SnappyMenus)
                    return menu;

                menu.populateClickableComponentList();
                menu.snapToDefaultClickableComponent();

                return menu;
            }

            /// <summary>
            /// Create a new instance of the <see cref="LetterViewerMenuEx2"/> class.
            /// </summary>
            /// <param name="id">The ID of the mail.</param>
            /// <param name="text">The text content of the mail.</param>
            /// <param name="recipeName">The name of the recipe attached to the mail.</param>
            /// <param name="recipeType">The type of the recipe attached to the mail.</param>
            /// <returns>The created <see cref="LetterViewerMenuEx2"/> instance.</returns>
            public static LetterViewerMenuEx2 CreateRecipeMailMenu(string id, string text, string recipeName, RecipeType recipeType)
            {
                var menu = new LetterViewerMenuEx2(id, text)
                {
                    mailType = MailType.RecipeMail,
                };

                // If there is no recipe attached to the mail we are done.
                if (recipeName == null)
                {
                    return menu;
                }

                // Load the relevant recipe game asset to obtain the recipe's data.
                Dictionary<string, string> recipes;
                switch (recipeType)
                {
                    case RecipeType.Cooking:
                        // If the player already received the recipe -> don't attach the recipe to the mail.
                        if (Game1.player.cookingRecipes.ContainsKey(recipeName))
                        {
                            monitor.Log($"The player already learned the recipe \"{recipeType}\"!");
                            return menu;
                        }

                        recipes = Game1.content.Load<Dictionary<string, string>>("Data\\CookingRecipes");
                        break;
                    case RecipeType.Crafting:
                        // If the player already received the recipe -> don't attach the recipe to the mail.
                        if (Game1.player.craftingRecipes.ContainsKey(recipeName))
                        {
                            monitor.Log($"The player already learned the recipe \"{recipeType}\"!");
                            return menu;
                        }

                        recipes = Game1.content.Load<Dictionary<string, string>>("Data\\CraftingRecipes");
                        break;
                    default:
                        throw new ArgumentException($"Invalid value \"{recipeType}\" for the recipe type!", nameof(recipeType));
                }

                // If the specified recipe is not available in the loaded recipe game asset, we throw an error
                if (!recipes.TryGetValue(recipeName, out string recipeData))
                {
                    monitor.Log($"A recipe with the name \"{recipeName}\" was not found!", LogLevel.Warn);
                    throw new ArgumentException($"Could not find a recipe with the specified recipeName \"{recipeName}\"!");
                }

                // Add the recipe to the recipes the player already obtained.
           
                int translatedNameIndex;
                if (recipeType == RecipeType.Cooking)
                {
                    translatedNameIndex = 4; // See Data/CookingRecipes{.lg-LG}.xnb files
                    menu.CookingOrCrafting = Game1.content.LoadString("Strings\\UI:LearnedRecipe_cooking");
                    Game1.player.cookingRecipes.Add(recipeName, 0);
                }
                else 
                {
                    translatedNameIndex = 5; // See Data/CraftingRecipes{.lg-LG}.xnb files
                    menu.CookingOrCrafting = Game1.content.LoadString("Strings\\UI:LearnedRecipe_crafting");
                    Game1.player.craftingRecipes.Add(recipeName, 0);
                }
                
                // Set the name of the recipe depending on the currently selected display language.
                string[] recipeParams = recipeData.Split('/');
                if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
                {
                    if (recipeParams.Length < translatedNameIndex + 1)
                    {
                        menu.LearnedRecipe = recipeName;
                        monitor.Log($"There is no translated name for the recipe \"{recipeName}\" available! Using the recipe name as a fallback name.", 
                            LogLevel.Warn);
                    }
                    else
                    {
                        // Read the translated name field from the recipe asset.
                        menu.LearnedRecipe = recipeParams[translatedNameIndex];
                    }               
                }
                else
                {
                    // Language is English -> use the supplied recipe name.
                    menu.LearnedRecipe = recipeName;
                }

                return menu;
            }

            /// <summary>
            /// The ID of the mail.
            /// </summary>
            public string MailId { get; private set; }

            /// <summary>
            /// Contains information about how the player interacted with the content of the mail.
            /// </summary>
            public MailInteractionRecord InteractionRecord { get; private set; }

            /// <summary>
            /// Add functionality to add a clicked item to the <see cref="selectedItems"/> list or 
            /// to accept a quest./>
            /// </summary>
            /// <param name="x">X-coordinate of the click.</param>
            /// <param name="y">Y-coordinate of the click.</param>
            /// <param name="playSound">Not used.</param>
            public override void receiveLeftClick(int x, int y, bool playSound = true)
            {
                // Handle [attached item] click
                foreach (ClickableComponent clickableComponent in this.itemsToGrab)
                {
                    if (clickableComponent.containsPoint(x, y) && clickableComponent.item != null)
                    {
                        // Add the clicked item to the list of user-selected items.
                        selectedItems.Add(clickableComponent.item);

                        Game1.playSound("coin");
                        clickableComponent.item = null;

                        return;
                    }
                }

                // Handle [quest accept button] click
                if (this.QuestId != QUEST_ID_NO_QUEST && this.acceptQuestButton.containsPoint(x, y))
                {
                    Game1.player.addQuest(this.QuestId);

                    this.questAccepted = true;
                    this.QuestId = QUEST_ID_NO_QUEST;

                    Game1.playSound("newArtifact");
                    return;
                }

                base.receiveLeftClick(x, y, playSound);
            }

            /// <summary>
            /// Called before the menu is exited. We use this function to setup any <see cref="MailInteractionRecord"/> 
            /// data we might need later.
            /// </summary>
            protected override void cleanupBeforeExit()
            {
                switch (mailType)
                {                      
                    case MailType.ItemMail:
                        // Grab all attached items which weren't selected by the player.
                        var unselectedItems = this.itemsToGrab.Where(component => component.item != null).Select(component => component.item).ToList();

                        InteractionRecord = new ItemMailInteractionRecord(this.selectedItems, unselectedItems);
                        break;
                    case MailType.MoneyMail:
                        InteractionRecord = new MoneyMailInteractionRecord(this.MoneyIncluded);
                        break;
                    case MailType.RecipeMail:
                        InteractionRecord = new RecipeMailInteractionRecord(this.LearnedRecipe);
                        break;
                    case MailType.QuestMail:
                        InteractionRecord = new QuestMailInteractionRecord(this.attachedQuestId, this.questAccepted);
                        break;
                    default:
                        InteractionRecord = new MailInteractionRecord();
                        break;
                }

                base.cleanupBeforeExit();
            }
        }
    }
}
