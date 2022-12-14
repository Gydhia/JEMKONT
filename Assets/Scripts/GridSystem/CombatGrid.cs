using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class CombatGrid : WorldGrid
    {
        public int Longitude;
        public int Latitude;

        public bool HasStarted = false;
        public WorldGrid ParentGrid = null;
        public List<Cell> StraightPath() {
            return new List<Cell>();
        }
    }
}
