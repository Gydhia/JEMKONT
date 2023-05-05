using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GameData
{
    public static class Game 
    {
        public static GameData RefGameData { get { return _refGameDataContainer.Data; } }

        private static GameDataContainer _refGameDataContainer;
        public static GameDataContainer RefGameDataContainer
        {
            get { return _refGameDataContainer; }
            set { _refGameDataContainer = value; }
        }
    }
}

