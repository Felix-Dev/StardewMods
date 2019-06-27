using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;

namespace FelixDev.StardewMods.FeTK.Menus
{
    /// <summary>
    /// Provides data for the [ItemLetterMenuHelper.MenuClosed] event.
    /// </summary>
    public class LetterViewerMenuClosedEventArgs : EventArgs
    {
        public LetterViewerMenuClosedEventArgs(string mailId, List<Item> items)
        {
            MailId = mailId ?? throw new ArgumentNullException(nameof(mailId));
            SelectedItems = items ?? throw new ArgumentNullException(nameof(items));
        }

        public string MailId { get; }

        /// <summary>
        /// The selected items or an empty list.
        /// </summary>
        public List<Item> SelectedItems { get; }
    }
}
