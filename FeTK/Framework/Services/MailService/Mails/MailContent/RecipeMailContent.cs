using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// Provides an API to interact with the content of a <see cref="RecipeMail"/> instance.
    /// </summary>
    public class RecipeMailContent : MailContent, IRecipeMailContent
    {
        /// <summary>
        /// Create a new instance of the <see cref="RecipeMailContent"/> class.
        /// </summary>
        /// <param name="text">The text content of the mail.</param>
        /// <param name="recipeName">The name of the recipe attached to the mail.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="text"/> is <c>null</c>.</exception>
        public RecipeMailContent(string text, string recipeName) 
            : base(text)
        {
            RecipeName = recipeName;
        }

        /// <summary>
        /// The name of the recipe included in the mail.
        /// </summary>
        public string RecipeName { get; set; }
    }
}
