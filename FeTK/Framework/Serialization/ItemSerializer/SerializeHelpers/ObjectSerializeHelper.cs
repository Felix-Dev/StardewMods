using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace FelixDev.StardewMods.FeTK.Framework.Serialization
{
    /// <summary>
    /// Provides an API to help serialize/deserialize instances of the <see cref="Tool"/> class.
    /// 
    /// See <seealso cref="ItemSerializeHelper"/> for more information why we have to use a serialization
    /// helper here.
    /// </summary>
    internal class ObjectSerializeHelper : IItemSerializeHelper<SObject>
    {
        /// <summary>
        /// Construct a matching <see cref="SObject"/> instance from the provided data.
        /// </summary>
        /// <param name="data">The data to reconstruct into a <see cref="SObject"/> instance.</param>
        /// <returns>A <see cref="SObject"/> instance matching the data specified in <paramref name="data"/>.</returns>
        /// <exception cref="ArgumentException">The given <paramref name="data"/> does not contain the necessary data to create a <see cref="SObject"/> instance.</exception>
        /// <exception cref="NotImplementedException">The given <paramref name="data"/> does not represent a supported <see cref="SObject"/> instance.</exception>
        public SObject Construct(IDictionary<string, string> data)
        {
            if (data == null || !data.ContainsKey("Id") || !int.TryParse(data["Id"], out int id)
                || !data.ContainsKey("Stack") || !int.TryParse(data["Stack"], out int stack)
                || !data.ContainsKey("Quality") || !int.TryParse(data["Quality"], out int quality)
                || !data.ContainsKey("IsBigCraftable") || !bool.TryParse(data["IsBigCraftable"], out bool isBigCraftable))
            {
                throw new ArgumentException(nameof(data), "Cannot construct a Stardew Valley item from the given data!");
            }

            return isBigCraftable
                ? new SObject(Vector2.Zero, id, false)
                : new SObject(Vector2.Zero, id, stack) { Quality = quality};
        }

        /// <summary>
        /// Deconstruct a <see cref="SObject"/> instance into a format which can be serialized.
        /// </summary>
        /// <param name="item">The <see cref="SObject"/> instance to deconstruct.</param>
        /// <returns>A serializable representation of the <see cref="SObject"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="item"/> is <c>null</c>.</exception>
        public IDictionary<string, string> Deconstruct(SObject item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var data = new SerializableDictionary<string, string>
            {
                { "Id", item.ParentSheetIndex.ToString() },
                { "Quality", item.Quality.ToString() },
                { "Stack", item.Stack.ToString() },
                { "IsBigCraftable", item.bigCraftable.ToString() },
            };

            return data;
        }
    }
}
