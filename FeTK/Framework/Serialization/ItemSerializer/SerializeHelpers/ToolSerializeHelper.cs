using StardewMods.Common.StardewValley;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Serialization
{
    /// <summary>
    /// This class provides an API to help serialize/deserialize instances of the <see cref="Tool"/> class.
    /// 
    /// See <seealso cref="ItemSerializeHelper"/> for more information why we have to use a serialization
    /// helper here.
    /// </summary>
    public class ToolSerializeHelper : IItemSerializeHelper<Tool>
    {
        /// <summary>
        /// Deconstruct a <see cref="Tool"/> instance into a format which can be serialized.
        /// </summary>
        /// <param name="item">The <see cref="Tool"/> instance to deconstruct.</param>
        /// <returns>A serializable representation of the <see cref="Tool"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="item"/> is <c>null</c>.</exception>
        public Dictionary<string, string> Deconstruct(Tool item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var data = new SerializableDictionary<string, string>
            {
                { "ToolType", item.BaseName },
                { "UpgradeLevel", item.UpgradeLevel.ToString() }
            };

            if (item is WateringCan can)
            {
                data.Add("WaterLeft", can.WaterLeft.ToString());
            }

            return data;
        }

        /// <summary>
        /// Construct a matching <see cref="Tool"/> instance from the provided data.
        /// </summary>
        /// <param name="data">The data to reconstruct into a <see cref="Tool"/> instance.</param>
        /// <returns>A <see cref="Tool"/> instance matching the data specified in <paramref name="data"/>.</returns>
        /// <exception cref="ArgumentException">The given <paramref name="data"/> does not contain the necessary data to create a <see cref="Tool"/> instance.</exception>
        /// <exception cref="NotImplementedException">The given <paramref name="data"/> does not represent a supported <see cref="Tool"/> instance.</exception>
        public Tool Construct(Dictionary<string, string> data)
        {
            if (data == null || !data.ContainsKey("ToolType") || !data.ContainsKey("UpgradeLevel") 
                || !int.TryParse(data["UpgradeLevel"], out int upgradeLevel))
            {
                throw new ArgumentException(nameof(data), "Cannot construct a tool from the given data!");
            }
            
            switch (data["ToolType"])
            {
                case ToolConstants.TOOL_BASE_NAME_AXE:
                    return new Axe() { UpgradeLevel = upgradeLevel };
                case ToolConstants.TOOL_BASE_NAME_PICKAXE:
                    return new Pickaxe() { UpgradeLevel = upgradeLevel };
                case ToolConstants.TOOL_BASE_NAME_HOE:
                    return new Hoe() { UpgradeLevel = upgradeLevel };
                case ToolConstants.TOOL_BASE_NAME_WATERING_CAN:
                    return !data.ContainsKey("WaterLeft") || !int.TryParse(data["WaterLeft"], out int waterLeft)
                        ? new WateringCan() { UpgradeLevel = upgradeLevel }
                        : new WateringCan() { UpgradeLevel = upgradeLevel, WaterLeft = waterLeft };
                default:
                    throw new NotImplementedException($"Unsupported tool {data["ToolType"]}!");
            }
        }
    }
}
