using Jemkont.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Events
{
    public class GridEventData : EventData<GridEventData>
    {
        public WorldGrid Grid;

        public GridEventData(WorldGrid Grid)
        {
            this.Grid = Grid;
        }
    }
}
