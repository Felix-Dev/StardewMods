using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;

namespace FelixDev.StardewMods.FeTK.Framework.UI
{
    /// <summary>
    /// Provides data for the <see cref="LetterViewerMenuWrapper.MenuClosed"/> event.
    /// </summary>
    public class LetterViewerMenuClosedEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new instance of the <see cref="LetterViewerMenuClosedEventArgs"/> class.
        /// </summary>
        /// <param name="mailId">The ID of the mail to be closed.</param>
        /// <param name="selectedItems">Sets the items of the mail which were selected. Can be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="mailId"/> is <c>null</c>.</exception>
        public LetterViewerMenuClosedEventArgs(string mailId, List<Item> selectedItems)
        {
            MailId = mailId ?? throw new ArgumentNullException(nameof(mailId));
            SelectedItems = selectedItems ?? new List<Item>();
        }

        /// <summary>
        /// The ID of the closed mail.
        /// </summary>
        public string MailId { get; }

        /// <summary>
        /// Gets a list that contains the items that were selected.
        /// </summary>
        public List<Item> SelectedItems { get; }
    }
}
