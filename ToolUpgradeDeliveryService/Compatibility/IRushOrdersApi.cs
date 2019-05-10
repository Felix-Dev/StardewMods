using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewMods.ToolUpgradeDeliveryService.Compatibility
{
    public interface IRushOrdersApi
    {
        event EventHandler<Tool> ToolRushed;
    }
}
