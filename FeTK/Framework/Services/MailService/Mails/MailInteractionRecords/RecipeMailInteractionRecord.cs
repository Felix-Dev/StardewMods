using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// Encapsulates player interaction data specific to a <see cref="RecipeMail"/> instance.
    /// </summary>
    public class RecipeMailInteractionRecord : MailInteractionRecord
    {
        /// <summary>
        /// Create a new instance of the <see cref="RecipeMailInteractionRecord"/> class.
        /// </summary>
        /// <param name="recipeName">The name of the recipe received by the player.</param>
        public RecipeMailInteractionRecord(string recipeName)
        {
            RecipeName = recipeName;
        }

        /// <summary>
        /// The name of the recipe which was received by the player.
        /// </summary>
        public string RecipeName { get; }
    }
}
