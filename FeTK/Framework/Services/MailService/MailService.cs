using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using FelixDev.StardewMods.FeTK.ModHelpers;
using FelixDev.StardewMods.FeTK.Serialization;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FelixDev.StardewMods.FeTK.Framework.Helpers;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// Provides an API to add mails to the player's mailbox.
    /// </summary>
    public class MailService : IMailSender
    {
        /// <summary>The prefix of the key used to identify the save data created by this mail service.</summary>
        private const string SAVE_DATA_KEY_PREFIX = "FelixDev.StardewMods.FeTK.Framework.Services.MailService";

        /// <summary>Provides access to the <see cref="IModEvents"/> API provided by SMAPI.</summary>
        private static readonly IModEvents events = ToolkitMod.ModHelper.Events;

        /// <summary>Provides access to the <see cref="IMonitor"/> API provided by SMAPI.</summary>
        private static readonly IMonitor monitor = ToolkitMod._Monitor;

        /// <summary>The ID of the mod which uses this mail service.</summary>
        private readonly string modId;

        /// <summary>The key used to identify the save data created by this mail service.</summary>
        private readonly string saveDataKey;

        /// <summary>The mail manager used to add mails to the game and provide mail events.</summary>
        private readonly IMailManager mailManager;

        /// <summary>The save data manager for this mail service.</summary>
        private readonly ModSaveDataHelper saveDataHelper;

        /// <summary>A helper to write and retrieve the save data for this mail service.</summary>
        private readonly SaveDataBuilder saveDataBuilder;

        /// <summary>
        /// Contains all mails added with this mail service which have not been read by the player yet. 
        /// For each day a collection of mails with that arrival day is stored (using a mapping [mail ID] -> [mail]).
        /// </summary>
        private IDictionary<int, IDictionary<string, Mail>> mailList = new Dictionary<int, IDictionary<string, Mail>>();

        /// <summary>
        /// Raised when a mail begins to open. The mail content can still be changed at this point.
        /// </summary>
        public event EventHandler<MailOpeningEventArgs> MailOpening;

        /// <summary>
        /// Raised when a mail has been closed.
        /// </summary>
        public event EventHandler<MailClosedEventArgs> MailClosed;

        /// <summary>
        /// Create a new instance of the <see cref="MailService"/> class.
        /// </summary>
        /// <param name="modId">The ID of the mod for which this mail service will be created for.</param>
        /// <param name="mailManager">The <see cref="IMailManager"/> instance which will be used by this service to add mails to the game.</param>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="modId"/> is <c>null</c> or does not contain at least one 
        /// non-whitespace character.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="mailManager"/> is <c>null</c>.</exception>
        internal MailService(string modId, IMailManager mailManager)
        {
            if (string.IsNullOrWhiteSpace(modId))
            {
                throw new ArgumentException("The mod ID needs to contain at least one non-whitespace character!", nameof(modId));
            }

            this.mailManager = mailManager ?? throw new ArgumentNullException(nameof(mailManager));

            this.saveDataKey = SAVE_DATA_KEY_PREFIX + "." + modId;

            this.saveDataHelper = ModSaveDataHelper.GetSaveDataHelper(modId);
            this.saveDataBuilder = new SaveDataBuilder();

            events.GameLoop.Saving += OnSaving;
            events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        /// <summary>
        /// Add a mail to the player's mailbox.
        /// </summary>
        /// <param name="daysFromNow">The day offset when the mail will arrive in the mailbox.</param>
        /// <param name="id">The ID of the mail.</param>
        /// <param name="content">The mail content.</param>
        /// <param name="attachedItem">The mail's attached item. Can be <c>null</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="daysFromNow"/> is less than or equal to <c>0</c>.</exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="id"/> is <c>null</c> or does not contain at least one non-whitespace character.
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> cannot be <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// A mail with the specified <paramref name="id"/> has already been added for the day specified by <paramref name="daysFromNow"/>.
        /// </exception>
        public void AddMail(int daysFromNow, string id, string content, Item attachedItem = null)
        {
            AddMail(daysFromNow, id, content, attachedItem != null ? new List<Item>() { attachedItem } : null);
        }

        /// <summary>
        /// Add a mail to the player's mailbox.
        /// </summary>
        /// <param name="arrivalDay">The day when the mail will arrive in the mailbox.</param>
        /// <param name="id">The ID of the mail.</param>
        /// <param name="content">The mail content.</param>
        /// <param name="attachedItem">The mail's attached item. Can be <c>null</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="arrivalDay"/> is in the past.</exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="id"/> is <c>null</c> or does not contain at least one non-whitespace character.
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> cannot be <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// A mail with the specified <paramref name="id"/> already exists for the specified <paramref name="arrivalDay"/>.
        /// </exception>
        public void AddMail(SDate arrivalDay, string id, string content, Item attachedItem = null)
        {
            if (arrivalDay == null)
            {
                throw new ArgumentNullException(nameof(arrivalDay));
            }

            AddMail(SDateHelper.GetCurrentDayOffsetFromDate(arrivalDay), id, content, attachedItem);
        }

        /// <summary>
        /// Add a mail to the player's mailbox.
        /// </summary>
        /// <param name="daysFromNow">The day offset when the mail will arrive in the mailbox.</param>
        /// <param name="id">The ID of the mail.</param>
        /// <param name="content">The mail content.</param>
        /// <param name="attachedItems">The mail's attached items. Can be <c>null</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="daysFromNow"/> has to be greater than or equal to <c>0</c>.</exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="id"/> is <c>null</c> or does not contain at least one non-whitespace character.
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// A mail with the specified <paramref name="id"/> already exists for the day specified by <paramref name="daysFromNow"/>.
        /// </exception>
        public void AddMail(int daysFromNow, string id, string content, List<Item> attachedItems)
        {
            if (daysFromNow < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(daysFromNow), "The day offset cannot be a negative number!");
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("The mail ID needs to contain at least one non-whitespace character!", nameof(id));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var arrivalDate = SDate.Now().AddDays(daysFromNow);
            var arrivalGameDay = arrivalDate.DaysSinceStart;

            if (HasMailForDayCore(arrivalGameDay, id))
            {
                string message = $"A mail with the ID \"{id}\" already exists for the date {arrivalDate}!";

                monitor.Log(message + " Please use a different mail ID!");
                throw new InvalidOperationException(message);
            }

            // Add the mail to the mail manager. Surface exceptions, if any, as they will indicate
            // errors with the user supplied arguments.
            mailManager.Add(this.modId, id, arrivalDate);
           
            var mail = new Mail(id, content, arrivalDate)
            {
                AttachedItems = attachedItems
            };

            if (!mailList.ContainsKey(arrivalGameDay))
            {
                mailList[arrivalGameDay] = new Dictionary<string, Mail>();
            }

            mailList[arrivalGameDay].Add(id, mail);
        }

        /// <summary>
        /// Add a mail to the player's mailbox.
        /// </summary>
        /// <param name="arrivalDay">The day when the mail will arrive in the mailbox.</param>
        /// <param name="id">The ID of the mail.</param>
        /// <param name="content">The mail content.</param>
        /// <param name="attachedItems">The mail's attached items. Can be <c>null</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="arrivalDay"/> is in the past.</exception>
        /// <exception cref="ArgumentException">The <paramref name="id"/> is an invalid mod ID.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> cannot be <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// A mail with the specified <paramref name="id"/> already exists for the specified <paramref name="arrivalDay"/>.
        /// </exception>
        public void AddMail(SDate arrivalDay, string id, string content, List<Item> attachedItems)
        {
            if (arrivalDay == null)
            {
                throw new ArgumentNullException(nameof(arrivalDay));
            }

            AddMail(SDateHelper.GetCurrentDayOffsetFromDate(arrivalDay), id, content, attachedItems);
        }

        /// <summary>
        /// Determine if a mail added by this mail service already exists for a day.
        /// </summary>
        /// <param name="day">The day to check for.</param>
        /// <param name="mailId">The ID of the mail.</param>
        /// <returns>
        /// <c>true</c> if a mail with the specified <paramref name="mailId"/> has already been added for the specified <paramref name="day"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="day"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// The specified <paramref name="mailId"/> is <c>null</c> or does not contain at least one 
        /// non-whitespace character.
        /// </exception>
        public bool HasMailForDay(SDate day, string mailId)
        {
            if (day == null)
            {
                throw new ArgumentNullException(nameof(day));
            }

            if (string.IsNullOrWhiteSpace(mailId))
            {
                throw new ArgumentException("The mail ID needs to contain at least one non-whitespace character!", nameof(mailId));
            }

            var gameDay = day.DaysSinceStart;
            return HasMailForDayCore(gameDay, mailId);
        }

        /// <summary>
        /// Determine if a mail with the given <paramref name="mailId"/> added by this mail service is currently in the mailbox.
        /// </summary>
        /// <param name="mailId">The ID of the mail.</param>
        /// <returns><c>true</c> if a mail with the specified <paramref name="mailId"/> has already been registered and 
        /// is currently in the mailbox; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">
        /// The specified <paramref name="mailId"/> is <c>null</c> or does not contain at least one 
        /// non-whitespace character.
        /// </exception>
        public bool HasRegisteredMailInMailbox(string mailId)
        {
            if (string.IsNullOrWhiteSpace(mailId))
            {
                throw new ArgumentException("The mail ID needs to contain at least one non-whitespace character!", nameof(mailId));
            }

            return mailManager.HasMailInMailbox(this.modId, mailId);
        }

        /// <summary>
        /// Determine if a mail has already been added by this mail service for a specific day.
        /// </summary>
        /// <param name="gameDay">The day to check for.</param>
        /// <param name="mailId">The ID of the mail.</param>
        /// <returns>
        /// <c>True</c>, if a mail with the specified <paramref name="mailId"/> has already been added for the specified <paramref name="gameDay"/>, 
        /// otherwise <c>false</c>.
        /// </returns>
        private bool HasMailForDayCore(int gameDay, string mailId)
        {
            return this.mailList.ContainsKey(gameDay) && this.mailList[gameDay].ContainsKey(mailId);
        }

        /// <summary>
        /// Notify an observer that a mail is being opened.
        /// </summary>
        /// <param name="e">Information about the mail being opened.</param>
        void IMailObserver.OnMailOpening(MailOpeningEventArgs e)
        {
            // Raise the mail-opening event.
            this.MailOpening?.Invoke(this, e);
        }

        /// <summary>
        /// Notify an observer that a mail has been closed.
        /// </summary>
        /// <param name="e">Information about the closed mail.</param>
        void IMailObserver.OnMailClosed(MailClosedCoreEventArgs e)
        {
            // Remove the mail from the service. 
            // We don't need to do key checks here because the service is only notified 
            // for closed mails belonging to it.
            this.mailList[e.ArrivalDay.DaysSinceStart].Remove(e.MailId);

            // Raise the mail-closed event.
            this.MailClosed?.Invoke(this, new MailClosedEventArgs(e.MailId, e.SelectedItems));
        }

        /// <summary>
        /// Retrieve a mail by its ID and arrival day.
        /// </summary>
        /// <param name="mailId">The ID of the mail. Needs to contain at least one non-whitespace character.</param>
        /// <param name="arrivalDay">The mail's arrival day in the mailbox of the receiver.</param>
        /// <returns>
        /// A <see cref="Mail"/> instance with the specified <paramref name="mailId"/> and <paramref name="arrivalDay"/> on success,
        /// othewise <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentException">The specified <paramref name="mailId"/> is an invalid mod ID.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="arrivalDay"/> is <c>null</c>.</exception>
        Mail IMailSender.GetMailFromId(string mailId, SDate arrivalDay)
        {
            if (string.IsNullOrWhiteSpace(mailId))
            {
                throw new ArgumentException("The mail ID needs to contain at least one non-whitespace character!", nameof(mailId));
            }

            if (arrivalDay == null)
            {
                throw new ArgumentNullException(nameof(arrivalDay));
            }

            int arrivalGameDay = arrivalDay.DaysSinceStart;
            return !mailList.TryGetValue(arrivalGameDay, out IDictionary<string, Mail> mailForDay)
                || !mailForDay.TryGetValue(mailId, out Mail mail)
                ? null
                : mail;
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            var saveData = saveDataBuilder.Construct(this.mailList);
            saveDataHelper.WriteData2(this.saveDataKey, saveData);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            var saveData = saveDataHelper.ReadData2<List<MailSaveData>>(this.saveDataKey);

            mailList = saveData != null
                ? saveDataBuilder.Reconstruct(saveData)
                : new Dictionary<int, IDictionary<string, Mail>>();
        }

        private class MailSaveData
        {
            public MailSaveData() { }

            public MailSaveData(string id, string content, int arrivalDay)
            {
                Id = id;
                AbsoluteArrivalDay = arrivalDay;
                Content = content;
            }

            public string Id { get; set; }

            public string Content { get; set; }

            public int AbsoluteArrivalDay { get; set; }

            public List<Dictionary<string, string>> AttachedItemsSaveData { get; set; }
        }

        private class SaveDataBuilder
        {
            private readonly IItemSerializeHelper<Item> itemSerializeHelper;

            public SaveDataBuilder()
            {
                itemSerializeHelper = new ItemSerializeHelper();
            }

            public IList<MailSaveData> Construct(IDictionary<int, IDictionary<string, Mail>> mailList)
            {
                var mailSaveDataList = new List<MailSaveData>();

                foreach (var mailsForDay in mailList.Values)
                {
                    foreach (var mail in mailsForDay.Values)
                    {
                        var mailSaveData = new MailSaveData(mail.Id, mail.Content, mail.ArrivalDay.DaysSinceStart);

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

            public IDictionary<int, IDictionary<string, Mail>> Reconstruct(IList<MailSaveData> mailSaveDataList)
            {
                var mailList = new Dictionary<int, IDictionary<string, Mail>>();

                foreach (var mailSaveData in mailSaveDataList)
                {
                    if (!mailList.ContainsKey(mailSaveData.AbsoluteArrivalDay))
                    {
                        mailList[mailSaveData.AbsoluteArrivalDay] = new Dictionary<string, Mail>();
                    }

                    var mailItem = new Mail(mailSaveData.Id, mailSaveData.Content, SDateHelper.GetDateFromDay(mailSaveData.AbsoluteArrivalDay));

                    if (mailSaveData.AttachedItemsSaveData != null && mailSaveData.AttachedItemsSaveData.Count > 0)
                    {
                        var attachedItems = new List<Item>();
                        foreach (var itemSaveData in mailSaveData.AttachedItemsSaveData)
                        {
                            attachedItems.Add(itemSerializeHelper.Construct(itemSaveData));
                        }

                        mailItem.AttachedItems = attachedItems;
                    }

                    mailList[mailSaveData.AbsoluteArrivalDay].Add(mailSaveData.Id, mailItem);
                }

                return mailList;
            }
        }
    }
}
