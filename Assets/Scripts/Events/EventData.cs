using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Events
{
    public abstract class EventData
    {
    }

    public abstract class EventData<EventDataType> : EventData
    {
        public delegate void Event(EventDataType Data);


        public EventData()
        {

        }

    }
}
