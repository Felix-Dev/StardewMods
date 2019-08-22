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
    public interface IRecipeMailContent : IMailContent
    {
        /// <summary>
        /// The name of the recipe included in the mail.
        /// </summary>
        /// <exception cref="ArgumentNullException">The specified recipe name is <c>null</c>.</exception>
        string RecipeName { get; set; }

        /// <summary>
        /// The type of the recipe included in the mail.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The specified recipe type is invalid.</exception>
        RecipeType RecipeType { get; set; }
    }
}
