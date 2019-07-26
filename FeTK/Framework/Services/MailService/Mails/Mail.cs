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
    public class Mail : IMailContent
    {
        /// <summary>The mail text.</summary>
        private string text;

        /// <summary>
        /// Create a new instance of the <see cref="Mail"/> class.
        /// </summary>
        /// <param name="id">The ID of the mail.</param>
        /// <param name="text">The text content of the mail.</param>
        /// <exception cref="ArgumentException">The specified <paramref name="id"/> is <c>null</c>, empty or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="text"/> is <c>null</c>.</exception>
        public Mail(string id, string text)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("The mail ID needs to contain at least one non-whitespace character!", nameof(id));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            Id = id;
            Text = text;
        }


        /// <summary>
        /// The ID for the mail.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The text content of the mail.
        /// </summary>
        /// <exception cref="ArgumentNullException">The mail text cannot be <c>null</c>.</exception>
        public string Text
        {
            get => text;
            set => text = value ?? throw new ArgumentNullException(nameof(text));
        }
    }
}
