using StardewValley;
using System.Collections.Generic;

namespace FelixDev.StardewMods.FeTK.Serialization
{
    /// <summary>
    /// Provides an API to help serialize/deserialize instances of the <see cref="Item"/> class.
    /// </summary>
    public interface IItemSerializeHelper<T> where T : Item
    {
        /// <summary>
        /// Deconstruct a <see cref="Item"/> instance into a format which can be serialized.
        /// </summary>
        /// <param name="item">The <see cref="Item"/> instance to deconstruct.</param>
        /// <returns>A serializable representation of the <see cref="Item"/> instance.</returns>
        Dictionary<string, string> Deconstruct(T item);

        /// <summary>
        /// Construct a matching <see cref="Item"/> instance from the provided data.
        /// </summary>
        /// <param name="data">The data to reconstruct into a <see cref="Item"/> instance.</param>
        /// <returns>A <see cref="Item"/> instance matching the data specified in <paramref name="data"/>.</returns>
        T Construct(Dictionary<string, string> data);
    }
}