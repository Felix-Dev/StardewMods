using StardewModdingAPI;
using StardewMods.ArchaeologyHouseContentManagementHelper.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewMods.Common
{
    public class CommonServices
    {
        public CommonServices(IMonitor monitor, ITranslationHelper translationHelper, IReflectionHelper reflectionHelper)
        {
            Monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            TranslationHelper = translationHelper ?? throw new ArgumentNullException(nameof(translationHelper));
            ReflectionHelper = reflectionHelper ?? throw new ArgumentNullException(nameof(reflectionHelper));
        }

        public IMonitor Monitor { get; }

        public ITranslationHelper TranslationHelper { get; }

        public IReflectionHelper ReflectionHelper { get; }
    }
}
