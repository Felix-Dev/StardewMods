using FelixDev.StardewMods.FeTK.Framework.Services;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace FelixDev.StardewMods.FeTK
{
    /// <summary>The mod entry point.</summary>
    internal class ToolkitMod : Mod
    {
        /// <summary>Provides access to the simplified APIs for writing mods provided by SMAPI.</summary>
        public static IModHelper ModHelper { get; private set; }

        /// <summary>Provides access to the <see cref="IMonitor"/> API provided by SMAPI.</summary>
        public static IMonitor _Monitor { get; private set; }

        /// <summary>
        /// The mod entry point, called after the mod is first loaded.
        /// </summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            _Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialized at this point, so this is a good time to set up mod integrations.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ServiceFactory.Setup(new MailManager());
        }
    }
}
