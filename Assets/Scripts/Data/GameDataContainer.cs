using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace DownBelow.GameData
{
    /// <summary>
    /// Save of the game that handle metadata for quick overview without loading the full save. Contains a <see cref="GameData">GameData</see>
    /// </summary>
    [Serializable]
    public class GameDataContainer
    {

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        static string slash = "\\";
#else
        static string slash = "/";
#endif

        [DataMember(Order = 1)]
        public string GameVersion;

        [DataMember(Order = 2)]
        public string SaveName;

        [DataMember(Order = 3)]
        public DateTime SaveTime;


        [JsonProperty(PropertyName = "Data")]
        [DataMember(Order = 20)]
        private GameData _data;

        [IgnoreDataMember]
        [JsonIgnore]
        public GameData Data
        {
            get
            {
                if(!IsGameDataLoaded && SavegameFile != null)
                {
                    this._load();
                }
                return this._data;
            }
            set
            {
                _data = value;
                IsGameDataLoaded = value != null;
            }
        }


        [JsonIgnore, NonSerialized, IgnoreDataMember]
        public bool IsMetaDataLoaded = false;

        [JsonIgnore, NonSerialized, IgnoreDataMember]
        public bool IsGameDataLoaded = false;

        [JsonIgnore, NonSerialized, IgnoreDataMember]
        public bool IsSharedSave = false;

        /// <summary>
        /// The path where this GameDataContainer is located
        /// </summary>
        [JsonIgnore, NonSerialized, IgnoreDataMember]
        public FileInfo SavegameFile;

        public static bool IsSaveValid(GameDataContainer savegame) => savegame != null;

        public static GameDataContainer FullLoad(FileInfo file)
        {
            GameDataContainer savegame = null;

            if (file.Name.Contains(".dbw"))
            {
                savegame = QuickLoad(file);
                savegame._load();
            }

            return savegame;
        }

        public void FullLoad()
        {
            if (!IsGameDataLoaded)
            {
                _load();
            }
        }

        public void _load()
        {
            bool loaded = true;
            try
            {
                loaded = LoadJson();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool LoadJson()
        {
            GameDataContainer savegame = null;
            bool ok = true;

            if (SavegameFile.Name.Contains(".dbw"))
            {
                using (System.IO.TextReader txt_reader = SavegameFile.OpenText())
                {
                    try
                    {
                        string text = txt_reader.ReadToEnd();

                        JsonSerializerSettings jsonSettings = DownBelowJSONHelper.CreateJSONGameDataContainerSettings();

                        savegame = JsonConvert.DeserializeObject<GameDataContainer>(text, jsonSettings);

                        if (savegame != null && savegame.Data != null)
                            Data = savegame.Data;
                        else
                            ok = false;
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError(new Exception("Error while loading JSON savegame", ex));
                        throw ex;
                    }
                }
            }
            else
            {
                ok = false;
            }

            return ok;
        }

        public static GameDataContainer LoadSharedJson(string sharedString)
        {
            JsonSerializerSettings jsonSettings = DownBelowJSONHelper.CreateJSONGameDataContainerSettings();

            GameDataContainer savegame = JsonConvert.DeserializeObject<GameDataContainer>(sharedString, jsonSettings);
            savegame.IsSharedSave = true;

            return savegame;
        }

        public static GameDataContainer QuickLoad(FileInfo file)
        {
            GameDataContainer savegame = null;

            try
            {
                savegame = _quickLoadJSON(file);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Savegame not quickloaded successfully. JSON failed\n" + ex);
                throw;
            }

            return savegame;
        }


        private static GameDataContainer _quickLoadJSON(FileInfo file)
        {
            GameDataContainer savegame = new GameDataContainer();
            savegame.SavegameFile = file;

            //file = await GameDataContainer._convertToSavegameV2(file);

            if (file.Name.EndsWith(".dbw"))
            {
                using (TextReader streamReader = file.OpenText())
                {
                    // 1024*12 = 12,288 - Security number to be sure that we load the entire header
                    // TODO: Read only until the "Data" section. But it's actually fast enough until we add a lot of thing in the header
                    char[] buffer = new char[12288];
                    streamReader.ReadBlock(buffer, 0, 12288);

                    string strSavegame = new string(buffer);
#if UNITY_EDITOR
                    UnityEngine.Debug.Log("Reading JSON file: " + file.FullName);
#endif
                    bool stillInHeader = true;

                    using (JsonTextReader reader = new JsonTextReader(new StringReader(strSavegame)))
                    {
                        while (reader.Read() && stillInHeader)
                        {
                            //we're only interested by PropertyName becaue they are part of the metadata
                            if (reader.TokenType == JsonToken.StartObject)
                                continue;

                            if (reader.TokenType == JsonToken.PropertyName)
                            {
                                //DebugController.Log("Token: " + reader.TokenType + ", Value: " + reader.Value);
                                string propertyname = reader.Value.ToString();

                                reader.Read();
                                string value = null;
                                if (reader.Value != null)
                                {
                                    value = reader.Value.ToString();
                                }

                                // We have to indicate the name of header's values
                                switch (propertyname.ToLower())
                                {
                                    case "gameversion":
                                        savegame.GameVersion = value;
                                        break;
                                    case "savename":
                                        savegame.SaveName = value;
                                        break;
                                    case "savetime":
                                        savegame.SaveTime = (DateTime)reader.Value;
                                        break;
                                    default:
                                        stillInHeader = false;
                                        break;
                                }
                            }

                            savegame.IsMetaDataLoaded = true;
                        }
                    }
                }

                //sanity check just in case
                if (!IsSaveValid(savegame))
                {
                    UnityEngine.Debug.LogError("Savegame not loaded successfully. File seems to be corrupted");
                }
            }

            return savegame;
        }

        public void Save(FileInfo file = null)
        {
            if (file != null)
                SavegameFile = file;

            if (SavegameFile == null)
                throw new Exception("The filepath is not provided");

            if (!IsGameDataLoaded)
                throw new Exception("The gamedata is empty, load it before");

            try
            {
                SaveTime = DateTime.Now;
                int lastSlashPosition = SavegameFile.FullName.LastIndexOf(slash);
                string tempPath = SavegameFile.FullName.Substring(0, lastSlashPosition + 1) + "~" + SavegameFile.FullName.Substring(lastSlashPosition + 1);
                FileInfo fileTemp = new FileInfo(tempPath);

                _saveJSON(fileTemp);

                SavegameFile.Refresh();

                // To avoid IOException cause the file was being used. Maybe find another way later cause it makes it laggy
                GC.Collect();
                GC.WaitForPendingFinalizers();

                if (SavegameFile.Exists)
                    SavegameFile.Delete();
                fileTemp.MoveTo(SavegameFile.FullName);

#if UNITY_EDITOR
                UnityEngine.Debug.Log("Game SAVED");
#endif
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Impossible to save the game to " + ((SavegameFile == null) ? "null" : SavegameFile.FullName));
                throw ex;
            }
        }


        public void DisposeAfterTest(string filePath)
        {

          

        }

        private void _saveJSON(FileInfo file)
        {
            using (StreamWriter writer = file.CreateText())
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("Writing JSON file: " + file.FullName);
#endif

                JsonSerializerSettings jsonSettings = DownBelowJSONHelper.CreateJSONGameDataContainerSettings();

                JsonSerializer jsonSerializer = JsonSerializer.Create(jsonSettings);

                jsonSerializer.Serialize(writer, this);
                writer.Close();
            }
        }

        public static void SortHeader(JObject jObj)
        {
            List<JProperty> props = jObj.Properties().ToList();
            JProperty dataProp = jObj.Property("Data");
            if (dataProp != null)
            {
                jObj.Remove("Data");
                jObj.Add(dataProp);
            }
        }

        public static JArray GetCellsCollection(JObject container)
        {
            JArray cells = null;
            if (container["Data"] != null && container["Data"].HasValues)
            {
                cells = (JArray)container["Data"]["cells"];
            }

            return cells;
        }
    }
}