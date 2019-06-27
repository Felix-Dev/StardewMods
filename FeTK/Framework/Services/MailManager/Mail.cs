using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Services
{
    public class Mail
    {
        private string content;

        public Mail(string id, string content)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Content = content ?? throw new ArgumentNullException(nameof(content));
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
        /// The items, if any, attached to the mail.
        /// </summary>
        public List<Item> AttachedItems { get; set; }
    }
}
