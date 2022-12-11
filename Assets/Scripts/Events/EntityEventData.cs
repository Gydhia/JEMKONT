using Jemkont.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Events
{
    public class EntityEventData : EventData<EntityEventData>
    {
        public CharacterEntity Entity;

        public EntityEventData(CharacterEntity Entity)
        {
            this.Entity = Entity;
        }
    }
}