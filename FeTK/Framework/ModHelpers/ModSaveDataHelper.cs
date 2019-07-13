using FelixDev.StardewMods.FeTK.Framework.Serialization;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.ModHelpers
{
    // TODO: - Remove ReadData/WriteData functions
    //       - Create a GetFilePath() function

    /// <summary>
    /// This class provides an API to read/write save data for a mod. It is based on the <see cref="IDataHelper"/>
    /// API provided by SMAPI, which can handle mod-specific save data in single-player or for the host ONLY in
    /// multiplayer.
    /// The <see cref="ModSaveDataHelper"/> class extends the <see cref="IDataHelper"/> API to handle mod-specific
    /// save data even for non-host players in multiplayer. The save data is stored in Stardew Valley's app  data folder.
    /// </summary>
    internal class ModSaveDataHelper
    {
        //private readonly IDataHelper dataHelper;

        private static readonly IDataHelper dataHelper = ToolkitMod.ModHelper.Data;

        private static ModSaveDataHelper globalSaveDataHelper;

        /// <summary>Encapsulates SMAPI's JSON file parsing.</summary>
        private readonly JsonHelper jsonHelper;

        private IDictionary<string, string> serializedSaveData;

        private static IDictionary<string, ModSaveDataHelper> saveDataHelpers = new Dictionary<string, ModSaveDataHelper>();

        private readonly string basePath;
        private readonly string dataOwner;

        public static ModSaveDataHelper GetSaveDataHelper(string dataOwner = null)
        {
            if (dataOwner == null)
            {
                if (globalSaveDataHelper == null)
                {
                    globalSaveDataHelper = new ModSaveDataHelper();
                }

                return globalSaveDataHelper;
            }

            if (!saveDataHelpers.ContainsKey(dataOwner))
            {
                saveDataHelpers[dataOwner] = new ModSaveDataHelper(dataOwner);
            }

            return saveDataHelpers[dataOwner];
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="ModSaveDataHelper"/> class.
        /// </summary>
        /// <param name="dataHelper">The <see cref="IDataHelper"/> instance of the mod to save data for.</param>
        public ModSaveDataHelper(IDataHelper dataHelper)
        {
            //this.dataHelper = dataHelper ?? throw new ArgumentNullException(nameof(dataHelper));
        }

        private ModSaveDataHelper()
        {
            this.basePath = Path.Combine(Constants.DataPath, "FeTK", "Saves");

            this.jsonHelper = new JsonHelper();

            ToolkitMod.ModHelper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitleScreen;
            ToolkitMod.ModHelper.Events.GameLoop.Saved += OnSaved;
        }

        private ModSaveDataHelper(string dataOwner)
        {
            this.basePath = Path.Combine(Constants.DataPath, "FeTK", "Saves");
            this.dataOwner = dataOwner;

            this.jsonHelper = new JsonHelper();

            ToolkitMod.ModHelper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitleScreen;
            ToolkitMod.ModHelper.Events.GameLoop.Saved += OnSaved;
        }

        private void OnReturnedToTitleScreen(object sender, ReturnedToTitleEventArgs e)
        {
            this.serializedSaveData = null;
        }

        private void OnSaved(object sender, SavedEventArgs e)
        {
            if (!Context.IsMainPlayer)
            {
                string filePath = dataOwner == null
                    ? Path.Combine(this.basePath, Constants.SaveFolderName)
                    : Path.Combine(this.basePath, Constants.SaveFolderName + " - Mods", dataOwner, Constants.SaveFolderName);

                if (serializedSaveData != null)
                {
                    jsonHelper.WriteJsonFile(filePath, serializedSaveData);
                }
            }
        }

        public TData ReadData2<TData>(string dataId)
            where TData : class
        {
            if (!Context.IsMainPlayer)
            {
                string filePath = dataOwner == null
                    ? Path.Combine(this.basePath, Constants.SaveFolderName)
                    : Path.Combine(this.basePath, Constants.SaveFolderName + " - Mods", dataOwner, Constants.SaveFolderName);

                if (serializedSaveData == null)
                {
                    bool result = jsonHelper.ReadJsonFileIfExists(filePath, out serializedSaveData);
                    if (!result)
                    {
                        serializedSaveData = new Dictionary<string, string>();
                    }
                }

                return serializedSaveData.TryGetValue(dataId, out string serialisedData)
                        ? jsonHelper.Deserialise<TData>(serialisedData)
                        : default(TData);
            }
            else
            {
                return dataHelper.ReadSaveData<TData>(dataId);
            }
        }

        public void WriteData2<TData>(string dataId, TData data)
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
                string filePath = dataOwner == null
                    ? Path.Combine(this.basePath, Constants.SaveFolderName)
                    : Path.Combine(this.basePath, Constants.SaveFolderName + " - Mods", dataOwner, Constants.SaveFolderName);

                if (serializedSaveData == null)
                {
                    bool result = jsonHelper.ReadJsonFileIfExists(filePath, out serializedSaveData);
                    if (!result)
                    {
                        serializedSaveData = new Dictionary<string, string>();
                    }
                }
                serializedSaveData[dataId] = jsonHelper.Serialise(data);
            }
            else
            {
                dataHelper.WriteSaveData(dataId, data);
            }
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
                throw new ArgumentException($"The data ID ({dataId}) needs to contain at least one non-whitespace character!", nameof(dataId));
            }

            if (!Context.IsMainPlayer)
            {
                string filePath = Path.Combine(this.basePath, Constants.SaveFolderName);
                if (jsonHelper.ReadJsonFileIfExists(filePath, out Dictionary<string, string> data))
                {
                    return data.TryGetValue(dataId, out string serialisedData)
                        ? jsonHelper.Deserialise<TData>(serialisedData)
                        : default(TData);

                }

                return default(TData);
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
