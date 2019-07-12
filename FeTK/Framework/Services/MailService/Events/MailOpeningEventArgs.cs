using System;
using System.Collections.Generic;
using System.Text;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// Provides data for the <see cref="IMailService.MailOpening"/> event.
    /// </summary>
    public class MailOpeningEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new instance of the <see cref="MailOpeningEventArgs"/> class.
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
