using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// Provides (simplified) access to different services a consuming mod can use. 
    /// 
    /// This class uses the Singleton pattern: Each consuming mod can have exactly one
    /// instance of the <see cref="ServiceFactory"/> class.
    /// </summary>
    public class ServiceFactory
    {
        /// <summary>Contains the created <see cref="ServiceFactory"/> instances. Maps a mod (via the mod ID) to its service factory instance. </summary>
        private static readonly IDictionary<string, ServiceFactory> serviceFactories = new Dictionary<string, ServiceFactory>();

        private static readonly IMailManager mailManager = new MailManager();

        /// <summary>The unique ID of the mod for which this service factory was created.</summary>
        private readonly string modId;

        /// <summary>The IModHelper instance of the mod which requested this service factory.</summary>
        private readonly IModHelper modHelper;

        /// <summary>
        /// Contains the created <see cref="MailService"/> instance for a service factory. Each factory has at most one instance.
        /// </summary>
        private IMailSender mailService;

        /// <summary>
        /// Get an instance of the <see cref="ServiceFactory"/> class.
        /// </summary>
        /// <param name="modId">The unique ID of the relevant mod.</param>
        /// <param name="modHelper">The <see cref="IModHelper"/> instance of the mod.</param>
        /// <returns>A service factory for the specified mod.</returns>
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

            if (serviceFactories.ContainsKey(modId))
            {
                return serviceFactories[modId];
            }

            var serviceFactory = new ServiceFactory(modId, modHelper);
            serviceFactories[modId] = serviceFactory;

            return serviceFactory;
        }

        /// <summary>
        /// Initialize an instance of the <see cref="ServiceFactory"/> class.
        /// </summary>
        /// <param name="modId">The unique ID of the relevant mod.</param>
        /// <param name="modHelper">The <see cref="IModHelper"/> instance of the mod.</param>
        private ServiceFactory(string modId, IModHelper modHelper)
        {
            this.modId = modId;
            this.modHelper = modHelper;
        }

        /// <summary>
        /// Get a mail service for the mod to use which requested this service factory.
        /// </summary>
        /// <returns>A newly created instance for a mod (if no such instance existed yet), otherwise the already existing one.</returns>
        public IMailService GetMailService()
        {
            if (mailService == null)
            {
                mailService = new MailService(modId, mailManager);

                mailManager.RegisterMailSender(modId, mailService);
            }

            return mailService;
        }
    }
}
