using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// Provides an API to notify a <see cref="IMailObserver"/> instance of mail events.
    /// </summary>
    internal interface IMailObserver
    {
        void OnMailOpening(MailOpeningEventArgs e);

        void OnMailClosed(MailClosedCoreEventArgs e);
    }
}
