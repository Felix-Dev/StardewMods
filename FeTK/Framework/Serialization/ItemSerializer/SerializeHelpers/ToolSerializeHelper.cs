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
    public class ToolSerializeHelper : IItemSerializeHelper<Tool>
    {
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
