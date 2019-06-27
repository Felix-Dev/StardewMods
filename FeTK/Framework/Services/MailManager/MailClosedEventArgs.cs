using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;

namespace FelixDev.StardewMods.FeTK.Services
{
    public class MailClosedEventArgs : EventArgs
    {
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
