using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Serialization
{
    /// <summary>
    /// This class provides an API to help serialize/deserialize instances of the <see cref="Item"/> class.
    /// 
    /// Without additional logic, an instance of the <see cref="Item"/> class cannot easily be serialized and it also cannot be
    /// deserialized. The <see cref="Item"/> class is an <c>abstract</c> class so when we deserialize, we need to create 
    /// a <c>concrete</c> <see cref="Item"/> implementation. Thus we also need to pass information about the concrete 
    /// <see cref="Item"/> type during serialization.
    /// </summary>
    public class ItemSerializeHelper : IItemSerializeHelper<Item>
    {
        private readonly IItemSerializeHelper<Tool> toolSerializeHelper;

        /// <summary>
        /// Create an instance of the <see cref="ItemSerializeHelper"/> class.
        /// </summary>
        public ItemSerializeHelper()
        {
            toolSerializeHelper = new ToolSerializeHelper();
        }

        /// <summary>
        /// Deconstruct a <see cref="Item"/> instance into a format which can be serialized.
        /// </summary>
        /// <param name="item">The <see cref="Item"/> instance to deconstruct.</param>
        /// <returns>A serializable representation of the <see cref="Item"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="item"/> is <c>null</c>.</exception>
        /// <exception cref="NotImplementedException">The given <paramref name="data"/> does not represent a supported <see cref="Item"/> instance.</exception>
        public Dictionary<string, string> Deconstruct(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            switch (item)
            {
                case Tool tool:
                    var data = toolSerializeHelper.Deconstruct(tool);

                    data.Add("ItemType", "Tool");
                    return data;
                default:
                    throw new NotImplementedException($"Unsupported item {item.Name}");
            }
        }

        /// <summary>
        /// Construct a matching <see cref="Item"/> instance from the provided data.
        /// </summary>
        /// <param name="data">The data to reconstruct into a <see cref="Item"/> instance.</param>
        /// <returns>A <see cref="Tool"/> instance matching the data specified in <paramref name="data"/>.</returns>
        /// <exception cref="ArgumentNullException">The given <paramref name="data"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The given <paramref name="data"/> does not contain the necessary data to create a <see cref="Tool"/> instance.</exception>
        /// <exception cref="NotImplementedException">The given <paramref name="data"/> does not represent a supported <see cref="Item"/> instance</exception>
        public Item Construct(Dictionary<string, string> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!data.ContainsKey("ItemType"))
            {
                throw new ArgumentException(nameof(data));
            }

            switch (data["ItemType"])
            {
                case "Tool":
                    return toolSerializeHelper.Construct(data);
                default:
                    throw new NotImplementedException($"Unsupported item type {data["ItemType"]}");
            }
        }
    }
}
