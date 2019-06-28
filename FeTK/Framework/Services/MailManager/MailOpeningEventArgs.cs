using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;

namespace FelixDev.StardewMods.FeTK.Services
{
    /// <summary>
    /// Provides data for the [MailManager.MailOpening] event.
    /// </summary>
    public class MailOpeningEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MailOpeningEventArgs"/> class.
        /// </summary>
        /// <param name="mail">The mail which is being opened.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="mail"/> is <c>null</c>.</exception>
        public MailOpeningEventArgs(Mail mail)
        {
            Mail = mail ?? throw new ArgumentNullException(nameof(mail));
        }

        /// <summary>
        /// The instance of the mail which is being opened.
        /// </summary>
        public Mail Mail { get; }
    }
}
