using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FelixDev.StardewMods.FeTK.Framework.Helpers;
using FelixDev.StardewMods.FeTK.ModHelpers;
using FelixDev.StardewMods.FeTK.UI.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    internal class MailManager : IMailManager
    {
        private const string MAIL_ID_SEPARATOR = "@@@";

        private const string SAVE_DATA_KEY = "FelixDev.StardewMods.FeTK.Framework.Services.MailManagerCore";

        private static readonly IModHelper toolkitModHelper = ToolkitMod.ModHelper;
        private static readonly IMonitor monitor = ToolkitMod._Monitor;

        private readonly IReflectionHelper reflectionHelper = ToolkitMod.ModHelper.Reflection;
        private readonly MailInjector mailInjector;

        private readonly ModSaveDataHelper saveDataHelper;

        private readonly IDictionary<string, IMailSender> mailSenders = new Dictionary<string, IMailSender>();

        private IDictionary<int, IList<string>> registeredMailsForDay = new Dictionary<int, IList<string>>();

        private IDictionary<string, MailMetaData> registeredMailsMetaData = new Dictionary<string, MailMetaData>();

        /// <summary>
        /// Create a new instance of the <see cref="MailManager"/> class.
        /// </summary>
        public MailManager()
        {
            this.mailInjector = new MailInjector(toolkitModHelper.Content);
            this.saveDataHelper = ModSaveDataHelper.GetSaveDataHelper();

            mailInjector.MailDataLoading += OnMailDataLoading;

            toolkitModHelper.Events.GameLoop.DayStarted += OnDayStarted;
            toolkitModHelper.Events.GameLoop.DayEnding += OnDayEnding;

            toolkitModHelper.Events.Display.MenuChanged += OnMenuChanged;

            toolkitModHelper.Events.GameLoop.Saving += OnSaving;
            toolkitModHelper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        /// <summary>
        /// Add a mail to the game.
        /// </summary>
        /// <param name="modId">The ID of the mod which wants to add the mail.</param>
        /// <param name="mailId">The ID of the mail.</param>
        /// <param name="arrivalDay">The day of arrival of the mail.</param>
        /// <exception cref="ArgumentException">
        /// The specified <paramref name="modId"/> is <c>null</c>, does not contain at least one 
        /// non-whitespace character or contains an invalid character sequence -or-
        /// the specified <paramref name="mailId"/> is <c>null</c>, does not contain at least one 
        /// non-whitespace character or contains an invalid character sequence.
        /// </exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="arrivalDay"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// A mail with the specified <paramref name="mailId"/> provided by the mod with the specified <paramref name="modId"/> 
        /// for the specified <paramref name="arrivalDay"/> already exists.
        /// </exception>
        public void Add(string modId, string mailId, SDate arrivalDay)
        {
            if (string.IsNullOrWhiteSpace(modId) || modId.Contains(MAIL_ID_SEPARATOR))
            {
                throw new ArgumentException($"The mod ID \"{modId}\" has to contain at least one non-whitespace character and cannot " +
                    $"contain the string {MAIL_ID_SEPARATOR}", nameof(modId));
            }

            if (mailId.Contains(MAIL_ID_SEPARATOR) || mailId.Contains(MAIL_ID_SEPARATOR))
            {
                throw new ArgumentException($"The mail ID \"{mailId}\" has to contain at least one non-whitespace character and cannot " +
                    $"contain the string {MAIL_ID_SEPARATOR}", nameof(mailId));
            }

            if (arrivalDay == null)
            {
                throw new ArgumentNullException(nameof(arrivalDay));
            }

            /*
             * Components for the internal mail ID: MOD_ID + user ID + Arrival Day.
             * 
             * Multiple mods can add mails with the same IDs for the same day, so in order to have
             * a straightforward relation between mail and the mod which added it, we need to add the mod ID 
             * to the internal mail ID.
             * 
             * We also add the arrival day to the internal mail ID because for each mod, mails with the 
             * same ID for different arrival days can be added. The user cannot, however, have multiple mails 
             * with the same ID for the same day for the same mod.
             */
            int absoluteArrivalDay = arrivalDay.DaysSinceStart;
            string internalMailId = modId + MAIL_ID_SEPARATOR + mailId + MAIL_ID_SEPARATOR + absoluteArrivalDay;

            if (registeredMailsMetaData.ContainsKey(internalMailId))
            {
                throw new InvalidOperationException($"A mail with the specified ID \"{mailId}\" for the given mod \"{modId}\" for the " +
                    $"specified arrival day \"{arrivalDay}\" already exists!");
            }

            registeredMailsMetaData[internalMailId] = new MailMetaData(modId, mailId, absoluteArrivalDay);

            if (!registeredMailsForDay.ContainsKey(absoluteArrivalDay))
            {
                registeredMailsForDay[absoluteArrivalDay] = new List<string>();
            }
            registeredMailsForDay[absoluteArrivalDay].Add(internalMailId);

            if (arrivalDay.Equals(SDate.Now()))
            {
                Game1.mailbox.Add(internalMailId);

                mailInjector.RequestMailCacheRefresh();
                monitor.Log($"Added the mail with ID \"{mailId}\" to the player's mailbox.");
            }
        }

        /// <summary>
        /// Register a mail sender with the mail manager.
        /// </summary>
        /// <param name="modId">The ID of the mod using the specified mail sender.</param>
        /// <param name="mailSender">The <see cref="IMailSender"/> instance to register.</param>
        /// <exception cref="ArgumentException">
        /// The specified <paramref name="modId"/> is <c>null</c> or does not contain at least one 
        /// non-whitespace character.
        /// </exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="mailSender"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">A mail sender with the same <paramref name="modId"/> has already been registered.</exception>
        public void RegisterMailSender(string modId, IMailSender mailSender)
        {
            if (string.IsNullOrWhiteSpace(modId))
            {
                throw new ArgumentException("The mod ID needs to contain at least one non-whitespace character!", nameof(modId));
            }

            if (mailSenders.ContainsKey(modId))
            {
                throw new InvalidOperationException($"A mail sender for the mod with ID \"{modId}\" has already been registered.");
            }

            mailSenders[modId] = mailSender ?? throw new ArgumentNullException(nameof(mailSender));
        }

        /// <summary>
        /// Determine whether the player's mailbox contains the specified mail.
        /// </summary>
        /// <param name="modId">The ID of the mod which created this mail.</param>
        /// <param name="mailId">The ID of the mail.</param>
        /// <returns>
        /// <c>true</c> if a mail with the specified <paramref name="mailId"/> created by the mod with the 
        /// specified <paramref name="modId"/> is in the player's mailbox; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The specified <paramref name="modId"/> is <c>null</c> or does not contain at least one 
        /// non-whitespace character -or-
        /// the specified <paramref name="mailId"/> is <c>null</c> or does not contain at least one 
        /// non-whitespace character.
        /// </exception>
        public bool HasMailInMailbox(string modId, string mailId)
        {
            if (string.IsNullOrWhiteSpace(modId))
            {
                throw new ArgumentException("The mod ID needs to contain at least one non-whitespace character!", nameof(modId));
            }

            if (string.IsNullOrWhiteSpace(mailId))
            {
                throw new ArgumentException("The mail ID needs to contain at least one non-whitespace character!", nameof(mailId));
            }

            return Game1.mailbox.Any(s => s.StartsWith(modId + MAIL_ID_SEPARATOR + mailId));
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!(e.OldMenu is LetterViewerMenu) && e.NewMenu is LetterViewerMenu letterMenu)
            {
                var mailId = reflectionHelper.GetField<string>(letterMenu, "mailTitle").GetValue();
                if (mailId == null || !this.registeredMailsMetaData.TryGetValue(mailId, out MailMetaData mailMetaData))
                {
                    return;
                }

                if (!this.mailSenders.TryGetValue(mailMetaData.ModId, out IMailSender mailSender))
                {
                    // A mail with this mailId was added to the mailManager at some point, but there is no sender
                    // owning this mail any longer. This can be due to the removal of a mod consuming the mail API of FeTK
                    // by the user. We can thus savely remove this mail from the MailManager on saving, as even if the consuming
                    // mod will be added back, for this save, the mail won't be displayed any longer (because it was already shown).
                    this.registeredMailsMetaData.Remove(mailId);
                    this.registeredMailsForDay[mailMetaData.ArrivalDay].Remove(mailId);

                    monitor.Log($"The mail \"{mailId}\" was added by the mod {mailMetaData.ModId} which seems to be no longer present.");
                    return;
                }

                var arrivalDate = SDateHelper.GetDateFromDay(mailMetaData.ArrivalDay);
                var mail = mailSender.GetMailFromId(mailMetaData.UserId, arrivalDate);
                if (mail == null)
                {
                    return;
                }

                // Raise the mail-opening event for this mail.
                mailSender.OnMailOpening(new MailOpeningEventArgs(mail));

                // Create the UI for this mail.
                var nLetterMenu = new LetterViewerMenuWrapper(reflectionHelper, mailId, mail.Content, mail.AttachedItems);

                // Setup the mail-closed event for this mail.
                nLetterMenu.MenuClosed += (s, e2) =>
                {
                    // Remove the closed mail from the mail manager.
                    RemoveMail(mailId, mailMetaData.ArrivalDay);

                    // Notify its sender that the mail has been read.
                    mailSender.OnMailClosed(new MailClosedCoreEventArgs(mailMetaData.UserId, arrivalDate, e2.SelectedItems));
                };

                monitor.Log($"Opening custom mail with the ID \"{mailMetaData.UserId}\"");

                // Show the letter viewer menu for this mail.
                nLetterMenu.Show();
            }
        }

        /// <summary>
        /// Remove the specified mail from the mail manager.
        /// </summary>
        /// <param name="mailId">The ID of the mail.</param>
        /// <param name="arrivalDay">The arrival day of the mail.</param>
        private void RemoveMail(string mailId, int arrivalDay)
        {
            registeredMailsMetaData.Remove(mailId);

            registeredMailsForDay[arrivalDay].Remove(mailId);
            if (registeredMailsForDay[arrivalDay].Count == 0)
            {
                registeredMailsForDay.Remove(arrivalDay);
            }

            // When testing in Multiplayer, it was noticed that apparently for non-host players,
            // already seen mails won't be removed from their [mailForTomorrow] list. This resulted
            // in adding already seen mails to the mailbox again and again. This caused "zombie" mails 
            // where the players's mailbox would indicate a mail, but nothing was shown for that mail (because its 
            // ID was no longer in the system).
            // Clearing the [mailForTomorrow] list manually will prevent the above described "zombie" mails.
            if (!Context.IsMainPlayer && Game1.player.mailForTomorrow.Contains(mailId))
            {
                Game1.player.mailForTomorrow.Remove(mailId);
            }
        }

        /// <summary>
        /// Inject the registered mails into the game's cached mail asset list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMailDataLoading(object sender, MailDataLoadingEventArgs e)
        {
            var currentDay = SDate.Now().DaysSinceStart;
            List<MailAssetData> customMailData = new List<MailAssetData>();

            foreach (var day in registeredMailsForDay.Keys)
            {
                if (day > currentDay)
                {
                    // the list of keys is not guaranteed to be sorted from [earlier] to [later], 
                    // so we have to iterate through all entries. 
                    continue;
                }

                customMailData.AddRange(registeredMailsForDay[day].Select(mailId => new MailAssetData(mailId, "PlaceholderContent")));
            }

            mailInjector.AddMailAssetData(customMailData);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Add all registered mail IDS to the mail injector
            mailInjector.RequestMailCacheRefresh();
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            var nextDay = SDate.Now().AddDays(1).DaysSinceStart;

            if (!registeredMailsForDay.TryGetValue(nextDay, out IList<string> mailIdsForDay))
            {
                return;
            }

            foreach (var mailId in mailIdsForDay)
            {
                Game1.addMailForTomorrow(mailId);

                // TODO: comment why we need no checks here (or put differently, why we will let exceptions surface)
                var userId = registeredMailsMetaData[mailId].UserId;
                monitor.Log($"Added the mail with ID \"{userId}\" to tomorrow's inbox.");
            }
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            saveDataHelper.WriteData2(SAVE_DATA_KEY, new SaveData(registeredMailsForDay, registeredMailsMetaData));
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            var saveData = saveDataHelper.ReadData2<SaveData>(SAVE_DATA_KEY);
            if (saveData != null)
            {
                this.registeredMailsForDay = saveData.MailPerDay;
                this.registeredMailsMetaData = saveData.MailMetaData;
            }
            else
            {
                this.registeredMailsForDay = new Dictionary<int, IList<string>>();
                this.registeredMailsMetaData = new Dictionary<string, MailMetaData>();
            }
        }

        private class MailMetaData
        {
            public MailMetaData(string modId, string userId, int arrivalDay)
            {
                ModId = modId;
                UserId = userId;
                ArrivalDay = arrivalDay;
            }

            public string ModId { get; }

            public string UserId { get; }

            public int ArrivalDay { get; }
        }

        private class SaveData
        {
            public SaveData() { }

            public SaveData(IDictionary<int, IList<string>> mailPerDay, IDictionary<string, MailMetaData> mailMetaData)
            {
                MailPerDay = mailPerDay;
                MailMetaData = mailMetaData;
            }

            public IDictionary<int, IList<string>> MailPerDay { get; set; }

            public IDictionary<string, MailMetaData> MailMetaData { get; set; }
        }
    }
}
