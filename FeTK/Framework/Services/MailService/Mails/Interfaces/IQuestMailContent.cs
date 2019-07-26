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
    public interface IQuestMailContent : IMailContent
    {
        /// <summary>
        /// The ID of the quest included in the mail.
        /// </summary>
        /// <remarks>A quest ID less than one (1) indicates no quest.</remarks>
        int QuestId { get; set; }
    }
}
