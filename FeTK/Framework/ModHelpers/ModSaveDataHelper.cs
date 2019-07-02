using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.ModHelpers
{
    /// <summary>
    /// This class is a helper to read/write save-file specific mod data. SMAPI already provides an implementation 
    /// of the <see cref="IDataHelper"/> interface which can handle mod specific save data. It (currently) 
    /// has a limitation though: In Stardew Valley Multiplayer, its implementation only works for the 
    /// host player.
    /// This class provides mod-specific save-file data reading/writing in Multiplayer even for non-host players by storing  
    /// the data inside the relevant mod.
    /// </summary>
    internal class ModSaveDataHelper
    {
        private readonly IDataHelper dataHelper;

        /// <summary>
        /// Initialize a new instance of the <see cref="ModSaveDataHelper"/> class.
        /// </summary>
        /// <param name="dataHelper">The <see cref="IDataHelper"/> instance of the mod to save data for.</param>
        public ModSaveDataHelper(IDataHelper dataHelper)
        {
            this.dataHelper = dataHelper ?? throw new ArgumentNullException(nameof(dataHelper));
        }

        /// <summary>
        /// Read arbitrary save-file specific data saved by a mod.
        /// </summary>
        /// <typeparam name="TData">The data model type. This should be a plain class that has public properties 
        /// for the data you want. The properties can be complex types.
        /// </typeparam>
        /// <param name="dataId">The unique key identifying the data.</param>
        /// <returns>Returns the parsed data, or <c>null</c> if the entry doesn't exist or is empty.</returns>
        /// <exception cref="ArgumentException">
        /// If the specified key <paramref name="dataId"/> is invalid. It has to be a valid file name or relative file path 
        /// and cannot contain dictionary climbing (../).
        /// </exception>
        /// <exception cref="InvalidOperationException">The player is the main player and hasn't loaded a save file yet.</exception>
        public TData ReadData<TData>(string dataId)
            where TData : class
        {
            if (string.IsNullOrWhiteSpace(dataId))
            {
                throw new ArgumentException($"The data ID ({dataId}) needs to be a valid file name!", nameof(dataId));
            }

            if (!Context.IsMainPlayer)
            {
                try
                {
                    return dataHelper.ReadJsonFile<TData>($"{Constants.SaveFolderName}/{dataId}");
                }
                catch (InvalidOperationException)
                {
                    throw new ArgumentException($"The data ID ({dataId}) needs to be a valid file name!", nameof(dataId));
                }
            }
            else
            {
                return dataHelper.ReadSaveData<TData>(dataId);
            }
        }

        /// <summary>
        /// Write arbitrary save-file specific data for a mod.
        /// </summary>
        /// <typeparam name="TData">The data model type. This should be a plain class that has public properties 
        /// for the data you want. The properties can be complex types.
        /// </typeparam>
        /// <param name="dataId">The unique key identifying the data.</param>
        /// <param name="data">The arbitrary data to save.</param>
        /// <exception cref="ArgumentException">The specified key <paramref name="dataId"/> is invalid.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="dataId"/> can not be <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The player is the main player and hasn't loaded a save file yet</exception>
        public void WriteData<TData>(string dataId, TData data)
            where TData : class
        {
            if (string.IsNullOrWhiteSpace(dataId))
            {
                throw new ArgumentException($"The specified data ID needs to be a valid filename or relative path (without dictionary climbing).", nameof(dataId));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!Context.IsMainPlayer)
            {
                try
                {
                    dataHelper.WriteJsonFile($"{Constants.SaveFolderName}/{dataId}", data);
                }
                catch (InvalidOperationException)
                {
                    throw new ArgumentException("The specified data ID needs to be a valid filename or relative path (without dictionary climbing).", nameof(dataId));
                }
            }
            else
            {
                dataHelper.WriteSaveData(dataId, data);
            }
        }

    }
}
