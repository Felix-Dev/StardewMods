using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;

namespace FelixDev.StardewMods.FeTK.Services
{
    /// <summary>
    /// Provides data for the [MailManager.MailClosed] event.
    /// </summary>
    public class MailClosedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MailClosedEventArgs"/> class.
        /// </summary>
        /// <param name="mailId">The ID of the mail to be closed.</param>
        /// <param name="selectedItems">Sets the selected items in the mail.</param>
        /// <exception cref="ArgumentNullException">If the <paramref name="mailId"/> is <c>null</c>.</exception>
        public MailClosedEventArgs(string mailId, List<Item> selectedItems)
        {
            MailId = mailId ?? throw new ArgumentNullException(nameof(mailId));
            SelectedItems = selectedItems ?? throw new ArgumentNullException(nameof(selectedItems));
        }

        /// <summary>
        /// The ID of the closed mail.
        /// </summary>
        public string MailId { get; }

        /// <summary>
        /// Attached items of the mail which were selected by the user or an empty list.
        /// </summary>
        public List<Item> SelectedItems { get; }
    }
}
