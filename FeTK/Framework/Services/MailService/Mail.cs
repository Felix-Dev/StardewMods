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
    /// Represents a Stardew Valley game letter.
    /// </summary>
    public class Mail
    {
        /// <summary>The mail content.</summary>
        private string content;

        /// <summary>
        /// Create a new instance of the <see cref="Mail"/> class.
        /// </summary>
        /// <param name="id">The ID of the mail.</param>
        /// <param name="content">The content of the mail.</param>
        /// <param name="arrivalDay">The specified day of arrival in the mailbox.</param>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="id"/> is <c>null</c> -or-
        /// the specified <paramref name="content"/> is <c>null</c> -or-
        /// the specified <paramref name="arrivalDay"/> is <c>null</c>.
        /// </exception>
        public Mail(string id, string content, SDate arrivalDay)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            ArrivalDay = arrivalDay ?? throw new ArgumentNullException(nameof(arrivalDay));
        }


        /// <summary>
        /// The ID for the mail.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The content of the mail.
        /// </summary>
        public string Content
        {
            get => content;
            set => content = value ?? throw new ArgumentNullException(nameof(content));
        }

        /// <summary>
        /// The mail's mailbox arrival day.
        /// </summary>
        public SDate ArrivalDay { get; }

        /// <summary>
        /// The items, if any, attached to the mail.
        /// </summary>
        public List<Item> AttachedItems { get; set; }
    }
}
