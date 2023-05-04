using DownBelow.GridSystem;
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
        public DateTime save_time;

        public GameDataContainer CreateSavegame(FileInfo file)
        {
            GameDataContainer savegame = new GameDataContainer()
            {
                GameVersion = this.game_version,
                SaveName = this.save_name,
                SaveTime = this.save_time
            };

            return savegame;
        }

        public GameDataContainer Save(System.IO.FileInfo file)
        {
            GameDataContainer savegame = this.CreateSavegame(file);
            savegame.Save(file);

            return savegame;
        }
    }
}