using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Events
{
    public class PositionEventData : EventData<PositionEventData>
    {
        public GridPosition GridPosition;
        public Cell Cell;

        public PositionEventData(GridPosition GridPosition, Cell Cell = null)
        {
            this.GridPosition = GridPosition;
            this.Cell = Cell;
        }
    }
}
