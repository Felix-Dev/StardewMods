using FelixDev.StardewMods.FeTK.Framework.Serialization;
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
    // TODO: Make class a public API (instead of internal)? That would require making the root folder of
    // (the first folder after AppData/Roaming/Stardew Valley in the absolute path) settable.

    /// <summary>
    /// Provides an API to read/write save data for a mod. It is based on the <see cref="IDataHelper"/>
    /// API provided by SMAPI, which can handle mod-specific save data in single-player or for the host ONLY in
    /// multiplayer.
    /// The <see cref="ModSaveDataHelper"/> class extends the <see cref="IDataHelper"/> API to handle mod-specific
    /// save data even for non-host players in multiplayer. The save data is stored in Stardew Valley's app data folder.
    /// </summary>
    internal class ModSaveDataHelper
    {
        private static readonly IDataHelper dataHelper = ToolkitMod.ModHelper.Data;

        private static IDictionary<string, ModSaveDataHelper> saveDataHelpers = new Dictionary<string, ModSaveDataHelper>();

        private static ModSaveDataHelper globalSaveDataHelper;

        /// <summary>Encapsulates SMAPI's JSON file parsing.</summary>
        private readonly JsonHelper jsonHelper;

        /// <summary>The save data buffer to read from/write to. Written to the specified save data file.</summary>
        private IDictionary<string, string> serializedSaveData;

        private readonly string basePath;
        private readonly string dataOwner;

        public static ModSaveDataHelper GetSaveDataHelper(string dataOwner = null)
        {
            if (dataOwner == null)
            {
                if (globalSaveDataHelper == null)
                {
                    globalSaveDataHelper = new ModSaveDataHelper(null);
                }

                return globalSaveDataHelper;
            }

            if (!saveDataHelpers.ContainsKey(dataOwner))
            {
                saveDataHelpers[dataOwner] = new ModSaveDataHelper(dataOwner);
            }

            return saveDataHelpers[dataOwner];
        }

        private ModSaveDataHelper(string dataOwner)
        {
            this.basePath = Path.Combine(Constants.DataPath, "FeTK", "Saves");
            this.dataOwner = dataOwner;

            this.jsonHelper = new JsonHelper();

            ToolkitMod.ModHelper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitleScreen;
            ToolkitMod.ModHelper.Events.GameLoop.Saved += OnSaved;
        }

        /// <summary>
        /// Clear the save data buffer.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        /// <remarks>
        /// When the player returns to the title screen, the player exists the current save game. We have to 
        /// clear the save data buffer so that it doesn't carry over specific data of a previous save when 
        /// a new save is loaded.
        /// </remarks>
        private void OnReturnedToTitleScreen(object sender, ReturnedToTitleEventArgs e)
        {
            this.serializedSaveData = null;
        }

        /// <summary>
        /// Write the save data buffer containing save data of a non-host multiplayer player to the save file.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        /// <remarks>
        /// Typically, SMAPI mods use the <see cref="IGameLoopEvents.Saving"/> event to write the data
        /// they want to save. Hence, we wait for that event with all its handlers to pass and write our
        /// save data buffer in the <see cref="IGameLoopEvents.Saved"/> event, containing all of the mod data
        /// requested of being saved. 
        /// </remarks>
        private void OnSaved(object sender, SavedEventArgs e)
        {
            if (!Context.IsMainPlayer && serializedSaveData != null)
            {
                jsonHelper.WriteJsonFile(GetSaveDataFilePath(), serializedSaveData);
            }
        }

        /// <summary>
        /// Get the path to the file containing the saved data.
        /// </summary>
        /// <returns>The file path on success; otherwise <c>null</c> if no save was loaded.</returns>
        private string GetSaveDataFilePath()
        {
            if (Constants.SaveFolderName == null || Constants.SaveFolderName.Equals(string.Empty))
            {
                return null;
            }

            return dataOwner == null
                ? Path.Combine(this.basePath, Constants.SaveFolderName)
                : Path.Combine(this.basePath, Constants.SaveFolderName + " - Mods", dataOwner, Constants.SaveFolderName);
        }

        /// <summary>
        /// Read arbitrary data for the current save. This is only possible if a save has been loaded.
        /// </summary>
        /// <typeparam name="TData">
        /// The data type. This should be a plain class that has public properties for the data you want. 
        /// The properties can be complex types.
        /// </typeparam>
        /// <param name="dataId">The unique ID for the data.</param>
        /// <returns>The parsed data, or <c>null</c> if the entry does not exist or is empty.</returns>
        public TData ReadData<TData>(string dataId)
            where TData : class
        {
            if (dataId == null)
            {
                throw new ArgumentNullException(nameof(dataId));
            }

            if (!Context.IsMainPlayer)
            {
                if (serializedSaveData == null)
                {
                    string saveDataPath = GetSaveDataFilePath() ?? throw new InvalidOperationException("The player hasn't loaded a save yet.");
                    bool result = jsonHelper.ReadJsonFileIfExists(saveDataPath, out serializedSaveData);
                    if (!result)
                    {
                        serializedSaveData = new Dictionary<string, string>();
                    }
                }

                return serializedSaveData.TryGetValue(dataId, out string serialisedData)
                        ? jsonHelper.Deserialise<TData>(serialisedData)
                        : null;
            }
            else
            {
                return dataHelper.ReadSaveData<TData>(dataId);
            }
        }

        /// <summary>
        /// Save arbitrary data for the current save. This is only possible if a save has been loaded 
        /// and the data will be lost if the player exits without saving.
        /// the current day.
        /// </summary>
        /// <typeparam name="TData">
        /// The data type. This should be a plain class that has public properties for the data you want. 
        /// The properties can be complex types.
        /// </typeparam>
        /// <param name="dataId">The unique ID for the data.</param>
        /// <param name="data">The arbitrary data to save.</param>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="dataId"/> is <c>null</c> -or-
        /// the specified <paramref name="data"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The player hasn't loaded a save file yet.</exception>
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
                if (serializedSaveData == null)
                {
                    string saveDataPath = GetSaveDataFilePath() ?? throw new InvalidOperationException("The player hasn't loaded a save yet.");
                    bool result = jsonHelper.ReadJsonFileIfExists(saveDataPath, out serializedSaveData);
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

    }
}
