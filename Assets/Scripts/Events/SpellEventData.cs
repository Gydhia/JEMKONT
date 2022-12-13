using DownBelow.Entity;
using DownBelow.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Events
{
    public class SpellEventData : EventData<SpellEventData>
    {
        public CharacterEntity Entity;
        public BuffType Buff;
        public int Value;

        public SpellEventData(CharacterEntity Entity, int Value, BuffType Buff = BuffType.None)
        {
            this.Entity = Entity;
            this.Value = Value;
            this.Buff = Buff;
        }
    }
}