using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK
{
    internal class ToolkitMod : Mod
    {
        public static IModHelper ModHelper { get; private set; }

        public static IMonitor _Monitor { get; private set; }

        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;

            _Monitor = this.Monitor;
        }
    }
}
