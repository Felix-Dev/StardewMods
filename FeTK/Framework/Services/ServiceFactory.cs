using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Services
{
    public class ServiceFactory
    {
        private static readonly IModHelper ToolkitModHelper = ToolkitMod.ModHelper;
        private static readonly Dictionary<string, ServiceFactory> ServiceFactories = new Dictionary<string, ServiceFactory>();

        private readonly IModHelper modHelper;

        private readonly string modId;

        private MailManager mailManager;

        public static ServiceFactory GetFactory(string modId, IModHelper helper)
        {
            if (string.IsNullOrWhiteSpace(modId))
            {
                throw new ArgumentException(nameof(modId));
            }

            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            if (ServiceFactories.ContainsKey(modId))
            {
                return ServiceFactories[modId];
            }

            var serviceFactory = new ServiceFactory(modId, helper);

            ServiceFactories.Add(modId, serviceFactory);

            return serviceFactory;
        }

        private ServiceFactory(string modId, IModHelper modHelper)
        {
            this.modId = modId;
            this.modHelper = modHelper;
        }

        public MailManager GetMailManager()
        {
            if (mailManager == null)
            {
                mailManager = new MailManager(modId, ToolkitModHelper.Events, modHelper.Data, ToolkitModHelper.Content, ToolkitMod._Monitor, ToolkitModHelper.Reflection);
            }

            return mailManager;
        }
    }
}
