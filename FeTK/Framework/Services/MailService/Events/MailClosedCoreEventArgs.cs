using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// Provides data for the <see cref="MailManager.MailClosed"/> event.
    /// </summary>
    internal class MailClosedCoreEventArgs : MailClosedEventArgs
    {
        /// <summary>
        /// Create a new instance of the <see cref="MailClosedCoreEventArgs"/> class.
        /// </summary>
        /// <param name="mailId">The ID of the mail to be closed.</param>
        /// <param name="arrivalDay">The mail's day of arrival in the receiver's mailbox.</param>
        /// <param name="selectedItems">Sets the items of the mail which were selected. Can be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="mailId"/> is <c>null</c> -or-
        /// the specified <paramref name="arrivalDay"/> is <c>null</c>. 
        /// </exception>
        public MailClosedCoreEventArgs(string mailId, SDate arrivalDay, List<Item> selectedItems) 
            : base(mailId, selectedItems)
        {
            ArrivalDay = arrivalDay ?? throw new ArgumentNullException(nameof(arrivalDay));
        }

        /// <summary>
        /// The mail's day of arrival in the receiver's mailbox.
        /// </summary>
        public SDate ArrivalDay { get; }
    }
}
