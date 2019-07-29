using StardewMods.Common.StardewValley;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Serialization
{
    /// <summary>
    /// Provides an API to help serialize/deserialize instances of the <see cref="Tool"/> class.
    /// 
    /// See <seealso cref="ItemSerializeHelper"/> for more information why we have to use a serialization
    /// helper here.
    /// </summary>
    internal class ToolSerializeHelper : IItemSerializeHelper<Tool>
    {
        private const string TOOL_NAME_AXE = "Axe";
        private const string TOOL_NAME_PICKAXE = "Pickaxe";
        private const string TOOL_NAME_HOE = "Hoe";
        private const string TOOL_NAME_WATERING_CAN = "Watering Can";

        /// <summary>
        /// Construct a matching <see cref="Tool"/> instance from the provided data.
        /// </summary>
        /// <param name="data">The data to reconstruct into a <see cref="Tool"/> instance.</param>
        /// <returns>A <see cref="Tool"/> instance matching the data specified in <paramref name="data"/>.</returns>
        /// <exception cref="ArgumentException">The given <paramref name="data"/> does not contain the necessary data to create a <see cref="Tool"/> instance.</exception>
        /// <exception cref="NotImplementedException">The given <paramref name="data"/> does not represent a supported <see cref="Tool"/> instance.</exception>
        public Tool Construct(IDictionary<string, string> data)
        {
            if (data == null || !data.ContainsKey("ToolType"))
            {
                throw new ArgumentException(nameof(data), "Cannot construct a tool from the given data!");
            }

            switch (data["ToolType"])
            {
                case "Axe":
                    return data.TryGetValue("UpgradeLevel", out string level) && int.TryParse(level, out int upgradeLevel)
                        ? new Axe() { UpgradeLevel = upgradeLevel }
                        : new Axe();
                case "Lantern":
                    return new Lantern();
                case "MagnifyingGlass":
                    return new MagnifyingGlass();
                case "MeleeWeapon":
                    if (data.TryGetValue("TileIndex", out string sTileIndex) && int.TryParse(sTileIndex, out int tileIndex))
                    {
                        return data.TryGetValue("BaseName", out string baseName) && baseName.Equals(string.Empty)
                            && data.TryGetValue("Type", out string sType) && int.TryParse(sType, out int type)
                            ? new MeleeWeapon(tileIndex, type)
                            : new MeleeWeapon(tileIndex);
                    }

                    return new MeleeWeapon();
                case "MilkPail":
                    return new MilkPail();
                case "Pan":
                    return new Pan();
                case "Pickaxe":
                    return data.TryGetValue("UpgradeLevel", out level) && int.TryParse(level, out upgradeLevel)
                        ? new Pickaxe() { UpgradeLevel = upgradeLevel }
                        : new Pickaxe();
                case "Hoe":
                    return data.TryGetValue("UpgradeLevel", out level) && int.TryParse(level, out upgradeLevel)
                        ? new Hoe() { UpgradeLevel = upgradeLevel }
                        : new Hoe();
                case "WateringCan":
                    return data.TryGetValue("UpgradeLevel", out level) && int.TryParse(level, out upgradeLevel)
                        && data.TryGetValue("WaterLeft", out string water) && int.TryParse(water, out int waterLeft)
                        ? new WateringCan() { UpgradeLevel = upgradeLevel, WaterLeft = waterLeft }
                        : new WateringCan();
                default:
                    throw new NotImplementedException($"Unsupported tool {data["ToolType"]}!");
            }
        }

        /// <summary>
        /// Deconstruct a <see cref="Tool"/> instance into a format which can be serialized.
        /// </summary>
        /// <param name="item">The <see cref="Tool"/> instance to deconstruct.</param>
        /// <returns>A serializable representation of the <see cref="Tool"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="item"/> is <c>null</c>.</exception>
        public IDictionary<string, string> Deconstruct(Tool item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var data = new SerializableDictionary<string, string>();
            switch (item)
            {
                case Axe _:
                    data["ToolType"] = "Axe";
                    data["UpgradeLevel"] = item.UpgradeLevel.ToString();
                    break;
                case FishingRod _:
                    // TODO: handle attached items
                    data["ToolType"] = "FishingRod";
                    data["UpgradeLevel"] = item.UpgradeLevel.ToString();
                    break;
                case Hoe _:
                    data["ToolType"] = "Hoe";
                    data["UpgradeLevel"] = item.UpgradeLevel.ToString();
                    break;
                case Lantern _:
                    data["ToolType"] = "Lantern";
                    break;
                case MagnifyingGlass _:
                    data["ToolType"] = "MagnifyingGlass";
                    data["TileIndex"] = item.CurrentParentTileIndex.ToString();
                    break;
                case MeleeWeapon meleeWeapon:
                    data["ToolType"] = "MeleeWeapon";
                    data["TileIndex"] = item.CurrentParentTileIndex.ToString();
                    data["BaseName"] = item.BaseName;
                    data["Type"] = meleeWeapon.type.Value.ToString();
                    break;
                case MilkPail _:
                    data["ToolType"] = "MilkPail";
                    break;
                case Pan _:
                    data["ToolType"] = "Pan";
                    break;
                case Pickaxe _:
                    data["ToolType"] = "Pickaxe";
                    data["UpgradeLevel"] = item.UpgradeLevel.ToString();
                    break;
                case Raft _:
                    data["ToolType"] = "Raft";
                    break;
                case Shears _:
                    data["ToolType"] = "Shears";
                    break;
                case Slingshot _:
                    // TODO: handle attached items
                    data["ToolType"] = "Slingshot";
                    break;
                case Sword _:
                    data["ToolType"] = "Sword";
                    data["Name"] = item.BaseName;
                    data["TileIndex"] = item.CurrentParentTileIndex.ToString();
                    break;
                case Wand _:
                    data["ToolType"] = "Wand";
                    break;
                case WateringCan can:
                    data["ToolType"] = "WateringCan";
                    data["UpgradeLevel"] = item.UpgradeLevel.ToString();
                    data["WaterLeft"] = can.WaterLeft.ToString();
                    break;
            }

            return data;
        }
    }
}
