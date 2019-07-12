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
        private const string SAVE_DATA_KEY_PREFIX = "FelixDev.StardewMods.FeTK.Framework.Services.MailService";

        private static readonly IModEvents events = ToolkitMod.ModHelper.Events;
        private static readonly IMonitor monitor = ToolkitMod._Monitor;

        private readonly string modId;
        private readonly string saveDataKey;

        private readonly IMailManager mailManager;

        private readonly ModSaveDataHelper saveDataHelper;
        private readonly SaveDataBuilder saveDataBuilder;

        private IDictionary<int, IDictionary<string, Mail>> mailList = new Dictionary<int, IDictionary<string, Mail>>();

        /// <summary>
        /// Raised when a mail begins to open. The mail content can still be changed at this point.
        /// </summary>
        public event EventHandler<MailOpeningEventArgs> MailOpening;

        /// <summary>
        /// Raised when a mail has been closed.
        /// </summary>
        public event EventHandler<MailClosedEventArgs> MailClosed;

        internal MailService(string modId, IMailManager mailManager)
        {
            this.modId = modId ?? throw new ArgumentNullException(nameof(modId));
            this.mailManager = mailManager ?? throw new ArgumentNullException(nameof(mailManager));

            this.saveDataKey = SAVE_DATA_KEY_PREFIX + "." + modId;

            //this.saveDataHelper = new ModSaveDataHelper(dataHelper);
            this.saveDataHelper = ModSaveDataHelper.GetSaveDataHelper(modId);

            this.saveDataBuilder = new SaveDataBuilder();

            events.GameLoop.Saving += OnSaving;
            events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        /// <summary>
        /// Add a mail to the player's mailbox.
        /// </summary>
        /// <param name="daysFromNow">The day offset when the mail will arrive in the mailbox.</param>
        /// <param name="id">The ID of the mail. Needs to contain at least one non-whitespace character.</param>
        /// <param name="content">The mail content.</param>
        /// <param name="attachedItem">The mail's attached item. Can be <c>null</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="daysFromNow"/> is less than or equal to <c>0</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="id"/> is an invalid mod ID.</exception>
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
        /// <param name="id">The ID of the mail. Needs to contain at least one non-whitespace character.</param>
        /// <param name="content">The mail content.</param>
        /// <param name="attachedItem">The mail's attached item. Can be <c>null</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="arrivalDay"/> is in the past.</exception>
        /// <exception cref="ArgumentException">The <paramref name="id"/> is an invalid mod ID.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> cannot be <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// A mail with the specified <paramref name="id"/> has already been added for the day specified by <paramref name="arrivalDay"/>.
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
        /// <param name="id">The ID of the mail. Needs to contain at least one non-whitespace character.</param>
        /// <param name="content">The mail content.</param>
        /// <param name="attachedItems">The mail's attached items. Can be <c>null</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="daysFromNow"/> has to be greater than or equal to <c>0</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="id"/> is an invalid mod ID.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> cannot be <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// A mail with the specified <paramref name="id"/> has already been added for the day specified by <paramref name="daysFromNow"/>.
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
        /// <param name="id">The ID of the mail. Needs to contain at least one non-whitespace character.</param>
        /// <param name="content">The mail content.</param>
        /// <param name="attachedItems">The mail's attached items. Can be <c>null</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="arrivalDay"/> is in the past.</exception>
        /// <exception cref="ArgumentException">The <paramref name="id"/> is an invalid mod ID.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> cannot be <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// A mail with the specified <paramref name="id"/> has already been added for the day specified by <paramref name="arrivalDay"/>.
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
        /// Check if a mail has already been added for a specific day.
        /// </summary>
        /// <param name="day">The day to check for.</param>
        /// <param name="mailId">The ID of the mail. Needs to contain at least one non-whitespace character.</param>
        /// <returns>
        /// <c>True</c>, if a mail with the specified <paramref name="mailId"/> has already been added for the specified <paramref name="day"/>, 
        /// otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="day"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The specified <paramref name="mailId"/> is invalid.</exception>
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
        /// Check if a mail registered with the given <paramref name="mailId"/> is already in the mailbox.
        /// </summary>
        /// <param name="mailId">The ID of the mail. Needs to contain at least one non-whitespace character.</param>
        /// <returns><c>True</c> if a mail with the specified <paramref name="mailId"/> has already been registered and 
        /// is currently in the mailbox, <c>False</c> otherwise.</returns>
        /// <exception cref="ArgumentException">The specified <paramref name="mailId"/> is invalid.</exception>
        public bool HasRegisteredMailInMailbox(string mailId)
        {
            if (string.IsNullOrWhiteSpace(mailId))
            {
                throw new ArgumentException("The mail ID needs to contain at least one non-whitespace character!", nameof(mailId));
            }

            return mailManager.HasMailInMailbox(this.modId, mailId);
        }

        /// <summary>
        /// Check if a mail has already been added for a specific day.
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
