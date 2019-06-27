using StardewValley;
using System.Collections.Generic;

namespace FelixDev.StardewMods.FeTK.Serialization
{
    public interface IItemSerializeHelper<T> where T : Item
    {
        T Construct(Dictionary<string, string> data);

        Dictionary<string, string> Deconstruct(T item);
    }
}