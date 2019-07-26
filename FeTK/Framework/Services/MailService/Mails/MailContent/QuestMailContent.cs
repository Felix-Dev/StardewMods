using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// Provides an API to interact with the content of a <see cref="QuestMail"/> instance.
    /// </summary>
    public class QuestMailContent : MailContent, IQuestMailContent
    {

        /// <summary>
        /// Create a new instance of the <see cref="QuestMailContent"/> class.
        /// </summary>
        /// <param name="text">The text content of the mail.</param>
        /// <param name="questId">The ID of the quest included in the mail.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="text"/> is <c>null</c>.</exception>
        public QuestMailContent(string text, int questId)
            : base(text)
        {
            QuestId = questId;
        }

        /// <summary>
        /// The ID of the quest included in the mail.
        /// </summary>
        /// <remarks>A quest ID less than one (1) indicates no quest.</remarks>
        public int QuestId { get; set; }
    }
}
