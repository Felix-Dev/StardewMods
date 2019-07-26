using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// Provides an API to interact with the content of an <see cref="MoneyMail"/> instance.
    /// </summary>
    public class MoneyMailContent : MailContent, IMoneyMailContent
    {
        /// <summary>The money attached to the mail.</summary>
        private int attachedMoney;

        /// <summary>
        /// Create a new instance of the <see cref="MoneyMailContent"/> class.
        /// </summary>
        /// <param name="text">The text content of the mail.</param>
        /// <param name="attachedMoney">The money attached to the mail.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="text"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The specified <paramref name="attachedMoney"/> is less than zero.</exception>
        public MoneyMailContent(string text, int attachedMoney) 
            : base(text)
        {
            if (attachedMoney < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attachedMoney), "The attached money cannot be less than zero!");
            }

            AttachedMoney = attachedMoney;
        }

        /// <summary>
        /// The money attached to the mail.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The mail's attached money cannot be less than zero.</exception>
        public int AttachedMoney
        {
            get => attachedMoney;
            set => attachedMoney = value < 0
                ? throw new ArgumentOutOfRangeException("The attached money cannot be less than zero!")
                : value;
        }
    }
}
