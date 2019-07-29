using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Serialization
{
    /// <summary>
    /// Provides an API to help serialize/deserialize instances of the <see cref="Furniture"/> class.
    /// 
    /// See <seealso cref="ItemSerializeHelper"/> for more information why we have to use a serialization
    /// helper here.
    /// </summary>
    internal class FurnitureSerializeHelper : IItemSerializeHelper<Furniture>
    {
        /// <summary>
        /// Construct a matching <see cref="Furniture"/> instance from the provided data.
        /// </summary>
        /// <param name="data">The data to reconstruct into a <see cref="Furniture"/> instance.</param>
        /// <returns>A <see cref="Tool"/> instance matching the data specified in <paramref name="data"/>.</returns>
        /// <exception cref="ArgumentException">The given <paramref name="data"/> does not contain the necessary data to create a <see cref="Furniture"/> instance.</exception>
        /// <exception cref="NotImplementedException">The given <paramref name="data"/> does not represent a supported <see cref="Furniture"/> instance.</exception>
        public Furniture Construct(IDictionary<string, string> data)
        {
            if (data == null || !data.ContainsKey("FurnitureType")
                || !data.ContainsKey("Id") || !int.TryParse(data["Id"], out int id))
            {
                throw new ArgumentException(nameof(data), "Cannot construct a <Furniture> object from the given data!");
            }

            switch (data["FurnitureType"])
            {
                case "TV":
                    return new TV(id, Vector2.Zero);
                case "Furniture":
                    return new Furniture(id, Vector2.Zero);
                default:
                    throw new NotImplementedException($"Unsupported furniture \"{data["FurnitureType"]}\"!");
            }
        }

        /// <summary>
        /// Deconstruct a <see cref="Furniture"/> instance into a format which can be serialized.
        /// </summary>
        /// <param name="item">The <see cref="Furniture"/> instance to deconstruct.</param>
        /// <returns>A serializable representation of the <see cref="Furniture"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="item"/> is <c>null</c>.</exception>
        public IDictionary<string, string> Deconstruct(Furniture item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            string furnitureType = item is TV _ 
                ? "TV" 
                : "Furniture";

            var data = new SerializableDictionary<string, string>
            {
                { "FurnitureType", furnitureType },
                { "Id", item.ParentSheetIndex.ToString() },
            };

            return data;
        }
    }
}
