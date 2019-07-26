using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// Encapsulates player interaction data specific to a <see cref="MoneyMail"/> instance.
    /// </summary>
    public class MoneyMailInteractionRecord : MailInteractionRecord
    {
        /// <summary>
        /// Create a new instance of the <see cref="MoneyMail"/> class.
        /// </summary>
        /// <param name="moneyReceived">The money recieved by the player.</param>
        /// <exception cref="ArgumentOutOfRangeException">The specified <paramref name="moneyReceived"/> is less than zero.</exception>
        public MoneyMailInteractionRecord(int moneyReceived)
        {
            if (moneyReceived < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(moneyReceived), "The received money cannot be less than zero!");
            }

            MoneyReceived = moneyReceived;
        }

        /// <summary>
        /// The money received by the player.
        /// </summary>
        public int MoneyReceived { get; }
    }
}
