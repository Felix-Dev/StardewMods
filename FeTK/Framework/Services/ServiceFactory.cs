using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Services
{
    /// <summary>
    /// Provides (simplified) access to different services a consuming mod can use. 
    /// 
    /// This class uses the Singleton pattern: Each consuming mod can have exactly one
    /// service factory instance.
    /// </summary>
    public class ServiceFactory
    {
        private static readonly IModHelper ToolkitModHelper = ToolkitMod.ModHelper;

        /// <summary>Contains the requested service factories. Maps a mod (via the mod ID) to its service factory instance. </summary>
        private static readonly Dictionary<string, ServiceFactory> ServiceFactories = new Dictionary<string, ServiceFactory>();

        /// <summary>The unique ID of the mod for which this service factory was created.</summary>
        private readonly string modId;

        /// <summary>The IModHelper instance of the mod which requested this service factory.</summary>
        private readonly IModHelper modHelper;

        /// <summary>
        /// Contains the created Mail Manager instance for a service factory. Each factory has at most one instance.
        /// </summary>
        private MailManager mailManager;

        /// <summary>
        /// Get a service factory for a mod.
        /// </summary>
        /// <param name="modId">The unique ID of the relevant mod.</param>
        /// <param name="modHelper">The IModHelper instance of the mod.</param>
        /// <returns>A service factory instance for the specified mod.</returns>
        /// <exception cref="ArgumentException">The <paramref name="modId"/> is not a valid mod ID (the ID has to contain at least one number/letter character).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="modHelper"/> cannot be <c>null</c>.</exception>
        public static ServiceFactory GetFactory(string modId, IModHelper modHelper)
        {
            if (string.IsNullOrWhiteSpace(modId))
            {
                throw new ArgumentException(nameof(modId));
            }

            if (modHelper == null)
            {
                throw new ArgumentNullException(nameof(modHelper));
            }

            if (ServiceFactories.ContainsKey(modId))
            {
                return ServiceFactories[modId];
            }

            var serviceFactory = new ServiceFactory(modId, modHelper);
            ServiceFactories.Add(modId, serviceFactory);

            return serviceFactory;
        }

        private ServiceFactory(string modId, IModHelper modHelper)
        {
            this.modId = modId;
            this.modHelper = modHelper;
        }

        /// <summary>
        /// Get a Mail Manager instance. Multiple calls return the same instance. 
        /// </summary>
        /// <returns>A newly created instance (if no instance existed yet), otherwise the already existing one.</returns>
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
