using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// Provides an API to interact with the content of a <see cref="MoneyMail"/> instance.
    /// </summary>
    public interface IMoneyMailContent : IMailContent
    {
        /// <summary>
        /// The money attached to the mail.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The attached money cannot be less than zero.</exception>
        int AttachedMoney { get; set; }
    }
}
