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
    /// Provides an API to help serialize/deserialize instances of the <see cref="Hat"/> class.
    /// 
    /// See <seealso cref="ItemSerializeHelper"/> for more information why we have to use a serialization
    /// helper here.
    /// </summary>
    internal class HatSerializeHelper : IItemSerializeHelper<Hat>
    {
        /// <summary>
        /// Construct a matching <see cref="Hat"/> instance from the provided data.
        /// </summary>
        /// <param name="data">The data to reconstruct into a <see cref="Hat"/> instance.</param>
        /// <returns>A <see cref="Tool"/> instance matching the data specified in <paramref name="data"/>.</returns>
        /// <exception cref="ArgumentException">The given <paramref name="data"/> does not contain the necessary data to create a <see cref="Hat"/> instance.</exception>
        /// <exception cref="NotImplementedException">The given <paramref name="data"/> does not represent a supported <see cref="Hat"/> instance.</exception>
        public Hat Construct(IDictionary<string, string> data)
        {
            if (data == null || !data.ContainsKey("Id") || !int.TryParse(data["Id"], out int id))
            {
                throw new ArgumentException(nameof(data), "Cannot construct a <Hat> object from the given data!");
            }

            return new Hat(id);
        }

        /// <summary>
        /// Deconstruct a <see cref="Hat"/> instance into a format which can be serialized.
        /// </summary>
        /// <param name="item">The <see cref="Hat"/> instance to deconstruct.</param>
        /// <returns>A serializable representation of the <see cref="Hat"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="item"/> is <c>null</c>.</exception>
        public IDictionary<string, string> Deconstruct(Hat item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var data = new SerializableDictionary<string, string>
            {
                { "Id", item.which.Value.ToString() },
            };

            return data;
        }
    }
}
