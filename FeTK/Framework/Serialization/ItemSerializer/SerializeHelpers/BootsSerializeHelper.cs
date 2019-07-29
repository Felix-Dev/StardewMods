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
    /// Provides an API to help serialize/deserialize instances of the <see cref="Boots"/> class.
    /// 
    /// See <seealso cref="ItemSerializeHelper"/> for more information why we have to use a serialization
    /// helper here.
    /// </summary>
    internal class BootsSerializeHelper : IItemSerializeHelper<Boots>
    {
        /// <summary>
        /// Construct a matching <see cref="Boots"/> instance from the provided data.
        /// </summary>
        /// <param name="data">The data to reconstruct into a <see cref="Boots"/> instance.</param>
        /// <returns>A <see cref="Tool"/> instance matching the data specified in <paramref name="data"/>.</returns>
        /// <exception cref="ArgumentException">The given <paramref name="data"/> does not contain the necessary data to create a <see cref="Boots"/> instance.</exception>
        /// <exception cref="NotImplementedException">The given <paramref name="data"/> does not represent a supported <see cref="Boots"/> instance.</exception>
        public Boots Construct(IDictionary<string, string> data)
        {
            if (data == null || !data.ContainsKey("Id") || !int.TryParse(data["Id"], out int id))
            {
                throw new ArgumentException(nameof(data), "Cannot construct a <Boots> object from the given data!");
            }

            return new Boots(id);
        }

        /// <summary>
        /// Deconstruct a <see cref="Boots"/> instance into a format which can be serialized.
        /// </summary>
        /// <param name="item">The <see cref="Boots"/> instance to deconstruct.</param>
        /// <returns>A serializable representation of the <see cref="Boots"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="item"/> is <c>null</c>.</exception>
        public IDictionary<string, string> Deconstruct(Boots item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var data = new SerializableDictionary<string, string>
            {
                { "Id", item.indexInTileSheet.Value.ToString() },
            };

            return data;
        }
    }
}
