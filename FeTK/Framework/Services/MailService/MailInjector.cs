using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    internal class MailInjector : IAssetEditor
    {
        private const string STARDEW_VALLEY_MAIL_DATA = "Data/mail";

        private readonly IContentHelper contentHelper;

        private readonly List<MailAssetData> mailAssetData = new List<MailAssetData>();

        public event EventHandler<MailDataLoadingEventArgs> MailDataLoading;

        public MailInjector(IContentHelper contentHelper)
        {
            this.contentHelper = contentHelper ?? throw new ArgumentNullException(nameof(contentHelper));

            this.contentHelper.AssetEditors.Add(this);
        }

        public void AddMailAssetData(List<MailAssetData> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            mailAssetData.AddRange(data);
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(STARDEW_VALLEY_MAIL_DATA);
        }

        public void Edit<T>(IAssetData asset)
        {
            MailDataLoading?.Invoke(this, new MailDataLoadingEventArgs());

            IDictionary<string, string> mails = asset.AsDictionary<string, string>().Data;
            foreach (var mailData in mailAssetData)
            {
                mails[mailData.ID] = mailData.Content;
            }

            mailAssetData.Clear();
        }

        /// <summary>
        /// Invalidates the game's internal Mail assets cache so it will have to be reloaded
        /// the next time the game tries to access mail assets. Custom mail data can injected
        /// during the reload process.
        /// </summary>
        public void RequestMailCacheRefresh()
        {
            contentHelper.InvalidateCache(STARDEW_VALLEY_MAIL_DATA);
        }
    }
}
