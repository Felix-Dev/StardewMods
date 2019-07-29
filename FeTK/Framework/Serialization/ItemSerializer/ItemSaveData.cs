using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Serialization
{
    internal class ItemSaveData
    {
        public ItemSaveData(string itemType, object itemData)
        {
            ItemType = itemType ?? throw new ArgumentNullException(nameof(itemType));
            ItemData = itemData ?? throw new ArgumentNullException(nameof(itemData));
        }

        public string ItemType { get; }

        public object ItemData { get; }
    }
}
