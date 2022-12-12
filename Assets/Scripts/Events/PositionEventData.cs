using Jemkont.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Events
{
    public class PositionEventData : EventData<PositionEventData>
    {
        public GridPosition GridPosition;

        public PositionEventData(GridPosition GridPosition)
        {
            this.GridPosition = GridPosition;
        }
    }
}
