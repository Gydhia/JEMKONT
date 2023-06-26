using DownBelow.GridSystem;
using DownBelow.UI.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace DownBelow.GameData
{
    [Serializable, DataContract]
    public class GameData
    {
        [DataMember]
        public string game_version;

        [DataMember]
        public string save_name;

        [DataMember]
        public GridData[] grids_data;

        [DataMember]
        public StorageData[] players_inventories;

        [DataMember]
        public int last_unlocked_abyss = -1;

        [DataMember]
        public int current_ressources;

        [DataMember]
        public ToolData[] tools_data;

        [DataMember]
        public Guid[] owned_cards;

        [DataMember]
        public DateTime save_time;

        public GameDataContainer CreateContainer()
        {
            GameDataContainer savegame = new GameDataContainer()
            {
                GameVersion = this.game_version,
                SaveName = this.save_name,
                SaveTime = this.save_time,
                Data = this
            };

            return savegame;
        }

        public GameDataContainer Save(System.IO.FileInfo file)
        {
            GameDataContainer savegame = this.CreateContainer();
            savegame.Save(file);

            return savegame;
        }
    }
}