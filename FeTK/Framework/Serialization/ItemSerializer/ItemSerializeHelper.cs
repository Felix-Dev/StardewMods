using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Serialization
{
    public class ItemSerializeHelper : IItemSerializeHelper<Item>
    {
        private readonly ToolSerializeHelper toolSerializeHelper;

        public ItemSerializeHelper()
        {
            toolSerializeHelper = new ToolSerializeHelper();
        }

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

        public Item Construct(Dictionary<string, string> itemData)
        {
            if (itemData == null)
            {
                throw new ArgumentNullException(nameof(itemData));
            }

            if (!itemData.ContainsKey("ItemType"))
            {
                throw new ArgumentException(nameof(itemData));
            }

            switch (itemData["ItemType"])
            {
                case "Tool":
                    return toolSerializeHelper.Construct(itemData);
                default:
                    throw new NotImplementedException($"Unsupported item type {itemData["ItemType"]}");
            }
        }
    }
}
