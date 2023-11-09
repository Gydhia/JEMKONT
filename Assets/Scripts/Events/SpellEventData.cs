using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Spells;
using DownBelow.Spells.Alterations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Events
{
    public class SpellTargetEventData : EventData<SpellTargetEventData>
    {
        public Spell TargetSpell;
        public Cell Cell;
        public SpellTargetEventData(Spell TargetSpell, Cell Cell)
        {
            this.TargetSpell = TargetSpell;
            this.Cell = Cell;
        }
    }

    public class SpellEventData : EventData<SpellEventData>
    {
        public CharacterEntity Entity;
        public EntityStatistics Stat;
        public int Value;

        public SpellEventData(CharacterEntity Entity, int Value, EntityStatistics Stat = EntityStatistics.None)
        {
            this.Entity = Entity;
            this.Value = Value;
            this.Stat = Stat;
        }
    }
    public class SpellEventDataAlteration : EventData<SpellEventDataAlteration>
    {
        public Alteration Alteration;

        public SpellEventDataAlteration(CharacterEntity Entity,Alteration alteration) {
            this.Alteration = alteration;
        }
    }

}