using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// Represents a Stardew Valley game letter with an optionally attached recipe.
    /// </summary>
    public class RecipeMail : Mail, IRecipeMailContent
    {
        private string recipeName;
        private RecipeType recipeType;

        /// <summary>
        /// Create a new instance of the <see cref="RecipeMail"/> class.
        /// </summary>
        /// <param name="id">The ID of the mail.</param>
        /// <param name="text">The text content of the mail.</param>
        /// <param name="recipeName">The name of the recipe attached to the mail.</param>
        /// <param name="recipeType">The type of the recipe attached to the mail.</param>
        /// <exception cref="ArgumentException">The specified <paramref name="id"/> is <c>null</c>, empty or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="text"/> is <c>null</c> -or-
        /// the specified <paramref name="recipeName"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The specified <paramref name="recipeType"/> is invalid.</exception>
        public RecipeMail(string id, string text, string recipeName, RecipeType recipeType)
            : base(id, text)
        {
            RecipeName = recipeName ?? throw new ArgumentNullException(nameof(recipeName));

            if (!Enum.IsDefined(typeof(RecipeType), recipeType))
            {
                throw new ArgumentOutOfRangeException(nameof(recipeType));
            }

            RecipeType = recipeType;
        }

        /// <summary>
        /// The name of the recipe included in the mail.
        /// </summary>
        /// <exception cref="ArgumentNullException">The specified recipe name is <c>null</c>.</exception>
        public string RecipeName
        {
            get => recipeName;
            set => recipeName = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The type of the recipe included in the mail.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The specified recipe type is invalid.</exception>
        public RecipeType RecipeType
        {
            get => recipeType;
            set
            {
                if (!Enum.IsDefined(typeof(RecipeType), value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                recipeType = value;
            }
        }
    }
}
