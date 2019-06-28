using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using FelixDev.StardewMods.FeTK.UI.Menus;
using FelixDev.StardewMods.FeTK.ModHelpers;
using FelixDev.StardewMods.FeTK.Serialization;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class MailManager : IAssetEditor
    {
        private const string SAVE_DATA_KEY_MAIL_MANAGER_PREFIX = "SAVE_DATA_MAIL_MANAGER";

        private const string MAIL_ID_USER_ID_PREFIX = "\\muid:";
        private const string MAIL_ID_ARRIVAL_DAY_PREFIX = "\\ad:";

        private const string STARDEW_VALLEY_MAIL_DATA = "Data/mail";

        private readonly IModEvents events;
        private readonly IMonitor monitor;
        private readonly IReflectionHelper reflectionHelper;
        private readonly IContentHelper contentHelper;

        private readonly string modId;
        private readonly string saveDataKey;

        private readonly ModSaveDataHelper saveDataHelper;
        private readonly SaveDataBuilder saveDataBuilder;

        private readonly List<MailCore> mailsRead = new List<MailCore>();

        private Dictionary<int, Dictionary<string, MailCore>> mailList = new Dictionary<int, Dictionary<string, MailCore>>();

        private MailCore currentlyOpenedMail;

        /// <summary>
        /// Occurs when a mail begins to open.
        /// </summary>
        public event EventHandler<MailOpeningEventArgs> MailOpening;

        /// <summary>
        /// Occurs when a mail has been closed.
        /// </summary>
        public event EventHandler<MailClosedEventArgs> MailClosed;

        private static readonly string[] MAIL_USER_ID_BLACKLIST =
        {
            MAIL_ID_USER_ID_PREFIX,
        };

        internal MailManager(string modId, IModEvents events, IDataHelper dataHelper, IContentHelper contentHelper, IMonitor monitor, IReflectionHelper reflectionHelper)
        {
            this.modId = modId ?? throw new ArgumentNullException(nameof(modId));

            this.events = events ?? throw new ArgumentNullException(nameof(events));
            this.contentHelper = contentHelper ?? throw new ArgumentNullException(nameof(contentHelper));
            this.monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            this.reflectionHelper = reflectionHelper ?? throw new ArgumentNullException(nameof(reflectionHelper));

            this.saveDataKey = SAVE_DATA_KEY_MAIL_MANAGER_PREFIX + "_" + modId;

            this.saveDataHelper = new ModSaveDataHelper(dataHelper);
            this.saveDataBuilder = new SaveDataBuilder();

            events.GameLoop.Saving += OnSaving;
            events.GameLoop.SaveLoaded += OnSaveLoaded;

            events.GameLoop.DayStarted += OnDayStarted;
            events.GameLoop.DayEnding += OnDayEnding;

            events.Display.MenuChanged += OnMenuChanged;
        }

        /// <summary>
        /// Add a mail to the player's mailbox.
        /// </summary>
        /// <param name="daysFromNow">The day offset when the mail will arrive in the mailbox.</param>
        /// <param name="id">The ID of the mail.</param>
        /// <param name="content">The mail content.</param>
        /// <param name="attachedItem">The mail's attached item. Can be <c>null</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="daysFromNow"/> has to greater than <c>0</c>.</exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="id"/> has to be a valid mod ID OR
        /// a mail with the <paramref name="id"/> has already been registered for the same day for the calling mod.
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> cannot be <c>null</c>.</exception>
        public void AddMail(int daysFromNow, string id, string content, Item attachedItem = null)
        {
            AddMail(daysFromNow, id, content, attachedItem != null ? new List<Item>() { attachedItem } : null);
        }

        /// <summary>
        /// Add a mail to the player's mailbox.
        /// </summary>
        /// <param name="daysFromNow">The day offset when the mail will arrive in the mailbox.</param>
        /// <param name="id">The ID of the mail.</param>
        /// <param name="content">The mail content.</param>
        /// <param name="attachedItems">The mail's attached items. Can be <c>null</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="daysFromNow"/> has to greater than <c>0</c>.</exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="id"/> has to be a valid mod ID OR
        /// a mail with the <paramref name="id"/> has already been registered for the same day for the calling mod.
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> cannot be <c>null</c>.</exception>
        public void AddMail(int daysFromNow, string id, string content, List<Item> attachedItems)
        {
            if (daysFromNow <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(daysFromNow));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(nameof(id), "The mail ID needs to contain at least one non-whitespace character!");
            }

            string blacklistEntry = MAIL_USER_ID_BLACKLIST.Where(s => id.Contains(s)).First();
            if (blacklistEntry != null)
            {
                throw new ArgumentException($"The mail ID cannot contain the string \"{blacklistEntry}\"!", nameof(id));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var arrivalDate = SDate.Now().AddDays(daysFromNow);
            var arrivalDay = arrivalDate.DaysSinceStart;

            /*
             * Components for the internal mail ID: MOD_ID + user ID + Arrival Day.
             * 
             * Multiple mods can add mails with the same IDs for the same day, so in order to have
             * a straightforward relation between mail and the mod which added it, we need to add the mod id 
             * to the internal mail ID.
             * 
             * We also add the arrival day to the internal ID so that you can use the same user IDs for different mails
             * scheduled for different days. The user cannot have two mails with the same ID for any given day.
             */
            var mail = new MailCore(modId + MAIL_ID_USER_ID_PREFIX + id + MAIL_ID_ARRIVAL_DAY_PREFIX + arrivalDay, id, arrivalDay, content)
            {
                AttachedItems = attachedItems
            };

            if (!mailList.ContainsKey(arrivalDay))
            {
                mailList.Add(arrivalDay, new Dictionary<string, MailCore>());
            }

            if (mailList[arrivalDay].ContainsKey(id))
            {
                string message = $"A mail with the ID {id} already exists for the date {arrivalDate}!";

                monitor.Log(message + " Please use a different mail ID!");
                throw new ArgumentException(message, nameof(id));
            }

            mailList[arrivalDay].Add(id, mail);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event parameters.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!(e.OldMenu is LetterViewerMenu) && e.NewMenu is LetterViewerMenu letterMenu)
            {
                // TODO: Improve MailManager so that we only land in this block when a custom mail has been opened!
                // Perhaps make e.NewMenu == LetterViewerMenuWrapper?
                // Might require creating a prefix method to GameLocation.mailbox()

                var mailId = reflectionHelper.GetField<string>(letterMenu, "mailTitle").GetValue();
                if (mailId != null)
                {
                    // Retrieve the registered mail from its ID. Do nothing if there is no mail with such an ID.
                    var cMail = GetMailFromId(mailId);
                    if (cMail == null)
                    {                       
                        return;
                    }

                    var mail = new Mail(cMail.UserId, cMail.Content)
                    {
                        AttachedItems = cMail.AttachedItems
                    };

                    // Raise the Mail Opening event for this mail
                    MailOpening?.Invoke(this, new MailOpeningEventArgs(mail));

                    // Create the letter viewer menu for this mail.
                    var nLetterMenu = new LetterViewerMenuWrapper(reflectionHelper, mail.Id, mail.Content, mail.AttachedItems);                 
                    nLetterMenu.MenuClosed += OnLetterMenuClosed;

                    // Show the menu.
                    nLetterMenu.Show();
                    currentlyOpenedMail = cMail;
                }
            }
        }

        private MailCore GetMailFromId(string id)
        {
            int index = id.LastIndexOf(MAIL_ID_USER_ID_PREFIX);
            if (index < 1)
            {
                return null;
            }

            string consumingModId = id.Substring(0, index);
            if (!consumingModId.Equals(modId))
            {
                return null;
            }

            index = id.LastIndexOf(MAIL_ID_ARRIVAL_DAY_PREFIX);
            if (index == -1)
            {
                return null;
            }

            var userId = id.Substring(0, index);

            index += MAIL_ID_ARRIVAL_DAY_PREFIX.Length;
            if (!int.TryParse(id.Substring(index), out int day))
            {
                return null;
            }

            if (!mailList.TryGetValue(day, out Dictionary<string, MailCore> mailsForDay))
            {
                return null;
            }

            return mailsForDay.TryGetValue(userId, out MailCore mail)
                ? mail
                : null;
        }

        private void OnLetterMenuClosed(object sender, LetterViewerMenuClosedEventArgs e)
        {
            // Mark mail as "read" so we can remove it when the day is over
            mailsRead.Add(currentlyOpenedMail);

            MailClosed?.Invoke(this, new MailClosedEventArgs(currentlyOpenedMail.Id, e.SelectedItems));

            currentlyOpenedMail = null;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            contentHelper.InvalidateCache(STARDEW_VALLEY_MAIL_DATA);
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            var nextDay = SDate.Now().AddDays(1).DaysSinceStart;

            if (!mailList.TryGetValue(nextDay, out Dictionary<string, MailCore> mailsForDay))
            {
                return;
            }

            foreach (var mail in mailsForDay.Values)
            {
                Game1.addMailForTomorrow(mail.Id);
                monitor.Log($"MailManager: Added the mail with the id {mail.UserId} to tomorrow's inbox!");
            }
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {          
            foreach (var mail in mailsRead)
            {
                // No dictionary checks below because all mails in the [mailsRead] list are in 
                // the [mailList]. If not, an unexpected error occured and we just surface the exception.
                var mailsForDay = mailList[mail.AbsoluteArrivalDay];

                mailsForDay.Remove(mail.UserId);

                if (mailsForDay.Count == 0)
                {
                    mailList.Remove(mail.AbsoluteArrivalDay);
                }
            }

            mailsRead.Clear();

            var saveData = saveDataBuilder.Construct(mailList);
            saveDataHelper.WriteData(this.saveDataKey, saveData);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            var saveData = saveDataHelper.ReadData<List<MailSaveData>>(this.saveDataKey);

            mailList = saveData != null
                ? saveDataBuilder.Reconstruct(saveData)
                : new Dictionary<int, Dictionary<string, MailCore>>();
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(STARDEW_VALLEY_MAIL_DATA);
        }

        public void Edit<T>(IAssetData asset)
        {
            IDictionary<string, string> mails = asset.AsDictionary<string, string>().Data;

            var currentDay = SDate.Now().DaysSinceStart;

            foreach (var day in mailList.Keys)
            {
                if (day > currentDay)
                {
                    // the list of keys is not guaranteed to be sorted from [earlier] to [later], 
                    // so we have to iterate through all entries. 
                    continue;
                }

                foreach (var mail in mailList[day].Values)
                {
                    mails.Add(mail.Id, mail.Content);
                }
            }
        }

        /// <summary>
        /// Represents an internal mail object containing word data for the mail manager.
        /// </summary>
        private class MailCore
        {
            public MailCore(string id, string userId, int absoluteArrivalDay, string content)
            {
                Id = id;
                UserId = userId;
                AbsoluteArrivalDay = absoluteArrivalDay;
                Content = content;
            }

            /// <summary>
            /// The internal ID for the mail.
            /// </summary>
            public string Id { get; }

            /// <summary>
            /// The user-provided ID for the mail.
            /// </summary>
            public string UserId { get; }

            /// <summary>
            /// The day of arrival of the mail in the receiver's mailbox. Absolute in-game day since starting to play.
            /// </summary>
            public int AbsoluteArrivalDay { get; }

            /// <summary>
            /// The content of the mail.
            /// </summary>
            public string Content { get; }

            /// <summary>
            /// The items, if any, attached to the mail.
            /// </summary>
            public List<Item> AttachedItems { get; set; }
        }

        private class MailSaveData
        {
            public MailSaveData() { }

            public MailSaveData(string id, string userId, int arrivalDay, string content)
            {
                Id = id;
                UserId = userId;
                AbsoluteArrivalDay = arrivalDay;
                Content = content;
            }

            public string Id { get; set; }

            public string UserId { get; set; }

            public int AbsoluteArrivalDay { get; set; }

            public string Content { get; set; }

            public List<Dictionary<string, string>> AttachedItemsSaveData { get; set; }
        }

        private class SaveDataBuilder
        {
            private readonly IItemSerializeHelper<Item> itemSerializeHelper;

            public SaveDataBuilder()
            {
                itemSerializeHelper = new ItemSerializeHelper();
            }

            public List<MailSaveData> Construct(Dictionary<int, Dictionary<string, MailCore>> mailList)
            {
                var mailSaveDataList = new List<MailSaveData>();

                foreach (var mailsForDay in mailList.Values)
                {
                    foreach (var mail in mailsForDay.Values)
                    {
                        var mailSaveData = new MailSaveData(mail.Id, mail.UserId, mail.AbsoluteArrivalDay, mail.Content);

                        if (mail.AttachedItems != null && mail.AttachedItems.Count > 0) 
                        {
                            var attachedItemsSaveData = new List<Dictionary<string, string>>();
                            
                            foreach (var item in mail.AttachedItems)
                            {
                                attachedItemsSaveData.Add(itemSerializeHelper.Deconstruct(item));
                            }

                            mailSaveData.AttachedItemsSaveData = attachedItemsSaveData;
                        }

                        mailSaveDataList.Add(mailSaveData);
                    }
                }

                return mailSaveDataList;
            }

            public Dictionary<int, Dictionary<string, MailCore>> Reconstruct(IList<MailSaveData> mailSaveDataList)
            {
                var mailList = new Dictionary<int, Dictionary<string, MailCore>>();

                foreach (var mailSaveData in mailSaveDataList)
                {
                    if (!mailList.ContainsKey(mailSaveData.AbsoluteArrivalDay))
                    {
                        mailList.Add(mailSaveData.AbsoluteArrivalDay, new Dictionary<string, MailCore>());
                    }

                    var mailItem = new MailCore(mailSaveData.Id, mailSaveData.UserId, mailSaveData.AbsoluteArrivalDay, mailSaveData.Content);

                    if (mailSaveData.AttachedItemsSaveData != null && mailSaveData.AttachedItemsSaveData.Count > 0)
                    {
                        var attachedItems = new List<Item>();
                        foreach (var itemSaveData in mailSaveData.AttachedItemsSaveData)
                        {
                            attachedItems.Add(itemSerializeHelper.Construct(itemSaveData));
                        }

                        mailItem.AttachedItems = attachedItems;
                    }

                    mailList[mailSaveData.AbsoluteArrivalDay].Add(mailSaveData.UserId, mailItem);
                }

                return mailList;
            }
        }
    }
}
