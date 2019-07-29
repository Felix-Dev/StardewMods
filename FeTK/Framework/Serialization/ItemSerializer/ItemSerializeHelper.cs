using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SObject = StardewValley.Object;

namespace FelixDev.StardewMods.FeTK.Framework.Serialization
{
    /// <summary>
    /// This class provides an API to help serialize/deserialize instances of the <see cref="Item"/> class.
    /// 
    /// Without additional logic, an instance of the <see cref="Item"/> class cannot easily be serialized and it also cannot be
    /// deserialized. The <see cref="Item"/> class is an <c>abstract</c> class so when we deserialize, we need to create 
    /// a <c>concrete</c> <see cref="Item"/> implementation. Thus we also need to pass information about the concrete 
    /// <see cref="Item"/> type during serialization.
    /// </summary>
    internal class ItemSerializeHelper : IItemSerializeHelper<Item>
    {
        private readonly IItemSerializeHelper<Tool> toolSerializeHelper;
        private readonly IItemSerializeHelper<Boots> bootsSerializeHelper;
        private readonly IItemSerializeHelper<Furniture> furnitureSerializeHelper;
        private readonly IItemSerializeHelper<Hat> hatSerializeHelper;
        private readonly IItemSerializeHelper<SObject> objectSerializeHelper;
        private readonly IItemSerializeHelper<Ring> ringSerializeHelper;
        private readonly IItemSerializeHelper<Wallpaper> wallpaperSerializeHelper;

        /// <summary>
        /// Create an instance of the <see cref="ItemSerializeHelper"/> class.
        /// </summary>
        public ItemSerializeHelper()
        {
            toolSerializeHelper = new ToolSerializeHelper();
            bootsSerializeHelper = new BootsSerializeHelper();
            furnitureSerializeHelper = new FurnitureSerializeHelper();
            hatSerializeHelper = new HatSerializeHelper();
            objectSerializeHelper = new ObjectSerializeHelper();
            ringSerializeHelper = new RingSerializeHelper();
            wallpaperSerializeHelper = new WallpaperSerializeHelper();
        }

        /// <summary>
        /// Construct a matching <see cref="Item"/> instance from the provided data.
        /// </summary>
        /// <param name="data">The data to reconstruct into a <see cref="Item"/> instance.</param>
        /// <returns>A <see cref="Tool"/> instance matching the data specified in <paramref name="data"/>.</returns>
        /// <exception cref="ArgumentNullException">The given <paramref name="data"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The given <paramref name="data"/> does not contain the necessary data to create a <see cref="Tool"/> instance.</exception>
        /// <exception cref="NotImplementedException">The given <paramref name="data"/> does not represent a supported <see cref="Item"/> instance</exception>
        public Item Construct(IDictionary<string, string> data)
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
                case "Boots":
                    return bootsSerializeHelper.Construct(data);
                case "Furniture":
                    return furnitureSerializeHelper.Construct(data);
                case "Hat":
                    return hatSerializeHelper.Construct(data);
                case "Object":
                    return objectSerializeHelper.Construct(data);
                case "Ring":
                    return ringSerializeHelper.Construct(data);
                case "Tool":
                    return toolSerializeHelper.Construct(data);
                case "Wallpaper":
                    return toolSerializeHelper.Construct(data);
                default:
                    throw new NotImplementedException($"Unsupported item type {data["ItemType"]}");
            }
        }

        /// <summary>
        /// Deconstruct a <see cref="Item"/> instance into a format which can be serialized.
        /// </summary>
        /// <param name="item">The <see cref="Item"/> instance to deconstruct.</param>
        /// <returns>A serializable representation of the <see cref="Item"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="item"/> is <c>null</c>.</exception>
        /// <exception cref="NotImplementedException">The given <paramref name="data"/> does not represent a supported <see cref="Item"/> instance.</exception>
        public IDictionary<string, string> Deconstruct(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            IDictionary<string, string> serializedItemData = new Dictionary<string, string>();
            switch (item)
            {
                case Boots boots:
                    serializedItemData.Add("ItemType", "Boots");
                    foreach (var kvp in bootsSerializeHelper.Deconstruct(boots))
                    {
                        serializedItemData[kvp.Key] = kvp.Value;
                    }
                    return serializedItemData;
                case Furniture furniture:
                    serializedItemData.Add("ItemType", "Furniture");
                    foreach (var kvp in furnitureSerializeHelper.Deconstruct(furniture))
                    {
                        serializedItemData[kvp.Key] = kvp.Value;
                    }
                    return serializedItemData;
                case Hat hat:
                    serializedItemData.Add("ItemType", "Hat");
                    foreach (var kvp in hatSerializeHelper.Deconstruct(hat))
                    {
                        serializedItemData[kvp.Key] = kvp.Value;
                    }
                    return serializedItemData;
                case Ring ring:
                    serializedItemData.Add("ItemType", "Ring");
                    foreach (var kvp in ringSerializeHelper.Deconstruct(ring))
                    {
                        serializedItemData[kvp.Key] = kvp.Value;
                    }
                    return serializedItemData;
                case Tool tool:
                    serializedItemData.Add("ItemType", "Tool");
                    foreach (var kvp in toolSerializeHelper.Deconstruct(tool))
                    {
                        serializedItemData[kvp.Key] = kvp.Value;
                    }
                    return serializedItemData;
                case Wallpaper wallpaper:
                    serializedItemData.Add("ItemType", "Wallpaper");
                    foreach (var kvp in wallpaperSerializeHelper.Deconstruct(wallpaper))
                    {
                        serializedItemData[kvp.Key] = kvp.Value;
                    }
                    return serializedItemData;
                case SObject @object:
                    serializedItemData.Add("ItemType", "Object");
                    foreach (var kvp in objectSerializeHelper.Deconstruct(@object))
                    {
                        serializedItemData[kvp.Key] = kvp.Value;
                    }
                    return serializedItemData;
                default:
                    throw new NotImplementedException($"Unsupported item {item.Name}");
            }
        }
    }
}
