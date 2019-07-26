using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Utilities;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// Represents a Stardew Valley game letter optionally attached with money the mail recipient will receive.
    /// </summary>
    public class MoneyMail : Mail, IMoneyMailContent
    {
        /// <summary>The money attached to the mail.</summary>
        private int attachedMoney;

        /// <summary>
        /// Create a new instance of the <see cref="MoneyMail"/> class.
        /// </summary>
        /// <param name="id">The ID of the mail.</param>
        /// <param name="text">The text content of the mail.</param>
        /// <param name="attachedMoney">The money attached to the mail.</param>
        /// <exception cref="ArgumentException">The speicified <paramref name="id"/> is <c>null</c>, empty or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="text"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The specified <paramref name="attachedMoney"/>is less than zero.</exception>
        public MoneyMail(string id, string text, int attachedMoney)
            : base(id, text)
        {
            if (attachedMoney < 0)
            {
                throw new ArgumentOutOfRangeException("The specified mail money has to greater than or equal to zero!", nameof(attachedMoney));
            }

            AttachedMoney = attachedMoney;
        }

        /// <summary>
        /// The money attached to the mail.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The attached money cannot be less than zero.</exception>
        public int AttachedMoney
        {
            get => attachedMoney;
            set => attachedMoney = (value >= 0)
                ? value
                : throw new ArgumentOutOfRangeException("The specified mail money has to greater than or equal to zero!");
        }
    }
}
