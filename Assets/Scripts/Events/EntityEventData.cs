using DownBelow.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Events
{
    public class EntityEventData : EventData<EntityEventData>
    {
        public CharacterEntity Entity;
        public int OldIndex;
        public int NewIndex;

        public EntityEventData(CharacterEntity Entity, int OldIndex = -1, int NewIndex = -1)
        {
            this.OldIndex = OldIndex;
            this.NewIndex = NewIndex;
            this.Entity = Entity;
        }
    }
}