namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    internal class MailAssetData
    {
        public MailAssetData(string id, string content)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new System.ArgumentException(nameof(id));
            }

            if (string.IsNullOrEmpty(content))
            {
                throw new System.ArgumentException(nameof(content));
            }

            ID = id;
            Content = content;
        }

        public string ID { get; }

        public string Content { get; }
    }
}