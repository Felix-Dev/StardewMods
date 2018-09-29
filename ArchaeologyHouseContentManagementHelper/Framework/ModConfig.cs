using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewMods.ArchaeologyHouseContentManagementHelper.Framework
{
    /// <summary>The parsed mod configuration.</summary>
    internal class ModConfig
    {
        /// <summary>The display duration of the description for a museum item. In milliseconds. </summary>
        public int MuseumItemDisplayTime { get; set; } = 3000;
    }
}
