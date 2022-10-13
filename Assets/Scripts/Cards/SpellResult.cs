using Jemkont.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells
{
    public class SpellResult
    {
        public Dictionary<CharacterEntity, int> DamagesDealt;
        public Dictionary<CharacterEntity, int> DamagesOverShield;
        public int SelfDamagesReceived;

        public Dictionary<CharacterEntity, int> HealingDone;
        public int SelfHealingReceived;

        public Dictionary<CharacterEntity, BuffType> BuffGiven;
        public BuffType SelfBuffReceived;
    }
}
