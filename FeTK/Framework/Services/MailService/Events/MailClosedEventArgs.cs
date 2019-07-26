using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// Provides data for the <see cref="IMailService.MailClosed"/> event.
    /// </summary>
    public class MailClosedEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new instance of the <see cref="MailClosedEventArgs"/> class.
        /// </summary>
        /// <param name="mailId">The ID of the mail which was closed.</param>
        /// <param name="interactionRecord">Information about how the player interacted with the mail content.</param>
        /// <exception cref="ArgumentNullException">
        /// The given <paramref name="mailId"/> is <c>null</c> -or-
        /// the given <paramref name="interactionRecord"/> is <c>null</c>.
        /// </exception>
        public MailClosedEventArgs(string mailId, MailInteractionRecord interactionRecord)
        {
            MailId = mailId ?? throw new ArgumentNullException(nameof(mailId));
            InteractionRecord = interactionRecord ?? throw new ArgumentNullException(nameof(interactionRecord));
        }

        /// <summary>
        /// The ID of the closed mail.
        /// </summary>
        public string MailId { get; }

        /// <summary>
        /// Get information about how the player interacted with the mail.
        /// </summary>
        public MailInteractionRecord InteractionRecord { get; }
    }
}
