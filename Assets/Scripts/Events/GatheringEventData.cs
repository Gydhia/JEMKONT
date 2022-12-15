using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Events
{
    public class GatheringEventData : EventData<GatheringEventData>
    {
        public InteractableResource ResourceRef;

        public GatheringEventData(InteractableResource ResourceRef)
        {
            this.ResourceRef = ResourceRef;
        }
    }
}