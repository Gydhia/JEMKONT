using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Events
{
    public class GridEventData : EventData<GridEventData>
    {
        public WorldGrid Grid;
        public bool AlliesVictory;

        public GridEventData(WorldGrid Grid, bool AlliesVictory = false)
        {
            this.Grid = Grid;
            this.AlliesVictory = AlliesVictory;
        }
    }
}
