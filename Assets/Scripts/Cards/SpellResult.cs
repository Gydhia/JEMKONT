using Jemkont.Entity;
using Jemkont.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells
{
    public class SpellResult
    {
        public Dictionary<CharacterEntity, int> DamagesDealt;
        public Dictionary<CharacterEntity, int> DamagesOnShield;
        public int SelfDamagesReceived = 0;
        public int SelfDamagesOverShieldReceived = 0;

        public Dictionary<CharacterEntity, int> HealingDone;
        public int SelfHealingReceived = 0;

        public Dictionary<CharacterEntity, List<BuffType>> BuffGiven;
        public List<BuffType> SelfBuffReceived;

        public void Init(List<CharacterEntity> targets, CharacterEntity caster)
        {
            this.DamagesDealt = new Dictionary<CharacterEntity, int>();
            this.DamagesOnShield = new Dictionary<CharacterEntity, int>();
            this.HealingDone = new Dictionary<CharacterEntity, int>();
            this.BuffGiven = new Dictionary<CharacterEntity, List<BuffType>>();

            foreach (CharacterEntity entity in targets)
            {
                if (entity == caster)
                    continue;

                entity.OnHealthRemoved += this._updateDamages;
                entity.OnShieldRemoved += this._updateShieldDamages;
                entity.OnHealthAdded += this._updateHealings;

                entity.OnDexterityAdded += this._updateBuffs;
                entity.OnStrengthAdded += this._updateBuffs;
                entity.OnDexterityRemoved += this._updateBuffs;
                entity.OnStrengthRemoved += this._updateBuffs;
            }

            caster.OnHealthRemoved += this._updateSelfDamages;
            caster.OnShieldRemoved += this._updateSelfShieldDamages;
            caster.OnHealthAdded += this._updateSelfHealings;

            caster.OnDexterityAdded += this._updateSelfBuffs;
            caster.OnStrengthAdded += this._updateSelfBuffs;
            caster.OnDexterityRemoved += this._updateSelfBuffs;
            caster.OnStrengthRemoved += this._updateSelfBuffs;
        }

        private void _updateDamages(SpellEventData data)
        {
            if (!this.DamagesDealt.ContainsKey(data.Entity))
                this.DamagesDealt.Add(data.Entity, 0);

            this.DamagesDealt[data.Entity] += data.Value;
        }

        private void _updateShieldDamages(SpellEventData data)
        {
            if (!this.DamagesOnShield.ContainsKey(data.Entity))
                this.DamagesOnShield.Add(data.Entity, 0);

            this.DamagesOnShield[data.Entity] += data.Value;
        }

        private void _updateHealings(SpellEventData data)
        {
            if (!this.HealingDone.ContainsKey(data.Entity))
                this.HealingDone.Add(data.Entity, 0);

            this.HealingDone[data.Entity] += data.Value;
        }

        private void _updateBuffs(SpellEventData data)
        {
            if (!this.BuffGiven.ContainsKey(data.Entity))
                this.BuffGiven.Add(data.Entity, new List<BuffType>());

            this.BuffGiven[data.Entity].Add(data.Buff);
        }

        private void _updateSelfDamages(SpellEventData data)
        {
            this.SelfDamagesReceived += data.Value;
        }
        private void _updateSelfShieldDamages(SpellEventData data)
        {
            this.SelfDamagesOverShieldReceived += data.Value;
        }
        
        private void _updateSelfHealings(SpellEventData data)
        {
            this.SelfHealingReceived += data.Value;
        }

        private void _updateSelfBuffs(SpellEventData data)
        {
            if(this.SelfBuffReceived == null)
                this.SelfBuffReceived = new List<BuffType>();

            this.SelfBuffReceived.Add(data.Buff);
        }
    }
}
