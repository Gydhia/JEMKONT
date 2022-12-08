using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.GridSystem
{
    public class CombatGrid : WorldGrid
    {
        public int Longitude;
        public int Latitude;

        public bool HasStarted = false;
        public WorldGrid ParentGrid = null;
    }
}
