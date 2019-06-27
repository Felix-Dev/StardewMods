using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.ModHelpers
{
    internal class ModSaveDataHelper
    {
        private readonly IDataHelper dataHelper;

        public ModSaveDataHelper(IDataHelper dataHelper)
        {
            this.dataHelper = dataHelper ?? throw new ArgumentNullException(nameof(dataHelper));
        }

        public TData ReadData<TData>(string dataId)
            where TData : class
        {
            if (dataId == null)
            {
                throw new ArgumentNullException(nameof(dataId));
            }

            return !Context.IsMainPlayer
                ? dataHelper.ReadJsonFile<TData>($"{Constants.SaveFolderName}/{dataId}")
                : dataHelper.ReadSaveData<TData>(dataId);
        }

        public void WriteData<TData>(string dataId, TData data)
            where TData : class
        {
            if (dataId == null)
            {
                throw new ArgumentNullException(nameof(dataId));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!Context.IsMainPlayer)
            {
                dataHelper.WriteJsonFile($"{Constants.SaveFolderName}/{dataId}", data);
            }
            else
            {
                dataHelper.WriteSaveData(dataId, data);
            }
        }

    }
}
