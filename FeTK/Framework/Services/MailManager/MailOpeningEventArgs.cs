using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;

namespace FelixDev.StardewMods.FeTK.Services
{
    public class MailOpeningEventArgs : EventArgs
    {
        public MailOpeningEventArgs(Mail mail)
        {
            Mail = mail ?? throw new ArgumentNullException(nameof(mail));
        }

        public Mail Mail { get; }
    }
}
