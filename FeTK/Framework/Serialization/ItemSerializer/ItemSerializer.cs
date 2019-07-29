using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace FelixDev.StardewMods.FeTK.Framework.Serialization
{
    internal class ItemSerializer
    {
        private readonly XmlSerializer itemSerializer;

        public ItemSerializer()
        {
            itemSerializer = new XmlSerializer(typeof(Item));
        }

        public Item Construct(ItemSaveData itemData)
        {
            if (itemData is null)
            {
                throw new ArgumentNullException(nameof(itemData));
            }

            switch (itemData.ItemType)
            {
                case "Vanilla":
                    StringReader strReader = new StringReader((string)itemData.ItemData);
                    using (var reader = XmlReader.Create(strReader))
                    {
                        return (Item)itemSerializer.Deserialize(reader);
                    }

                default:
                    throw new NotImplementedException($"Unsupported item type {itemData.ItemType}");
            }
        }

        public ItemSaveData Deconstruct(Item item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            XmlWriterSettings settings = new XmlWriterSettings
            {
                ConformanceLevel = ConformanceLevel.Auto,
                CloseOutput = true
            };

            StringWriter strWriter = new StringWriter();
            using (var writer = XmlWriter.Create(strWriter, settings))
            {
                itemSerializer.Serialize(writer, item);
            }

            return new ItemSaveData("Vanilla", strWriter.ToString());
        }
    }
}
