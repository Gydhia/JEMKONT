using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.Spells.Alterations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
{
    public class StatModification {
        public EntityStatistics stat;
        public int value;
        public StatModification(EntityStatistics stat,int value) {
            this.stat = stat;
            this.value = value;
        }
    }
    public class SpellResult
    {
        public event SpellEventData.Event OnDamageDealt;
        public event SpellEventData.Event OnHealReceived;
        public event SpellEventData.Event OnHealGiven;
        public event SpellEventData.Event OnAlterationGiven;
        public event SpellEventData.Event OnShieldRemoved;
        public event SpellEventData.Event OnShieldAdded;
        /// <summary>
        /// Called whenever a spell modifies a stat. All info in data.
        /// </summary>
        public event SpellEventData.Event OnStatModified;
        [HideInInspector]
        public bool HasEnded;

        private List<CharacterEntity> _targets;
        private Spell _spellRef;

        public virtual void Execute(List<CharacterEntity> targets, Spell spellRef)
        {
            this._spellRef = spellRef;
            this._targets = targets;
            this.HasEnded = false;
            this.Init(this._targets, this._spellRef.RefEntity);

            spellRef.RefEntity.SubToSpell(this);
        }

        public void FireOnDamageDealt(SpellEventData data)
        {
            OnDamageDealt?.Invoke(data);
        }
        public void FireOnHealReceived(SpellEventData data)
        {
            OnHealReceived?.Invoke(data);
        }
        public void FireOnHealGiven(SpellEventData data)
        {
            OnHealGiven?.Invoke(data);
        }
        public void FireOnAlterationGiven(SpellEventData data)
        {
            OnAlterationGiven?.Invoke(data);
        }
        public void FireOnShieldRemoved(SpellEventData data)
        {
            OnShieldRemoved?.Invoke(data);
        }
        public void FireOnShieldAdded(SpellEventData data)
        {
            OnShieldAdded?.Invoke(data);
        }
        public void FireOnStatModified(SpellEventData data)
        {
            switch (data.Stat)
            {
                case EntityStatistics.Health:
                    if (data.Value >= 0)
                        OnDamageDealt?.Invoke(data);
                    else
                        OnHealGiven?.Invoke(data);
                    break;
                case EntityStatistics.Shield:
                    if (data.Value >= 0)
                        OnShieldAdded?.Invoke(data);
                    else
                        OnShieldRemoved?.Invoke(data);
                    break;
                default:
                    OnStatModified?.Invoke(data);
                    break;
            }
        }

        public Dictionary<CharacterEntity, int> DamagesDealt;
        public Dictionary<CharacterEntity, int> DamagesOnShield;
        public int SelfDamagesReceived = 0;
        public int SelfDamagesOverShieldReceived = 0;

        public Dictionary<CharacterEntity, int> HealingDone;
        public int SelfHealingReceived = 0;

        public Dictionary<CharacterEntity, StatModification> StatModified;
        public Dictionary<EntityStatistics, int> SelfStatModified;
        public Dictionary<CharacterEntity,EAlterationType> AlterationGiven;
       
        public void Init(List<CharacterEntity> targets, CharacterEntity caster)
        {
            this.DamagesDealt = new Dictionary<CharacterEntity, int>();
            this.DamagesOnShield = new Dictionary<CharacterEntity, int>();
            this.HealingDone = new Dictionary<CharacterEntity, int>();

            this.StatModified = new Dictionary<CharacterEntity, StatModification>();
            this.SelfStatModified = new Dictionary<EntityStatistics, int>();
            this.AlterationGiven = new Dictionary<CharacterEntity, EAlterationType>();

            foreach (CharacterEntity entity in targets)
            {
                if (entity == caster)
                    continue;

                entity.OnHealthRemoved += this._updateDamages;
                entity.OnShieldRemoved += this._updateShieldDamages;
                entity.OnHealthAdded += this._updateHealings;

                entity.OnStrengthAdded += this._updateStat;
                entity.OnStrengthRemoved += this._updateStat;
                entity.OnSpeedAdded += this._updateStat;
                entity.OnSpeedRemoved += this._updateStat;
                entity.OnAlterationReceived += this._updateAlterations;
                entity.OnAlterationGiven += caster.FireOnAlterationGiven;
            }

            caster.OnHealthRemoved += this._updateSelfDamages;
            caster.OnShieldRemoved += this._updateSelfShieldDamages;
            caster.OnHealthAdded += this._updateSelfHealings;

            caster.OnStrengthAdded += this._updateSelfStat;
            caster.OnStrengthRemoved += this._updateSelfStat;

            caster.OnSpeedAdded += this._updateSelfStat;
        }

        private void _updateStat(SpellEventData data)
        {
            if (!this.StatModified.ContainsKey(data.Entity))
                this.StatModified.Add(data.Entity,new(data.Stat,0));

            this.FireOnStatModified(data);
            this.StatModified[data.Entity].value += data.Value;
        }
        private void _updateSelfStat(SpellEventData data) 
        {
            if (!this.SelfStatModified.ContainsKey(data.Stat))
                this.SelfStatModified.Add(data.Stat,data.Value);

            this.FireOnStatModified(data);
            this.SelfStatModified[data.Stat] += data.Value;
        }
        private void _updateDamages(SpellEventData data)
        {
            if (!this.DamagesDealt.ContainsKey(data.Entity))
                this.DamagesDealt.Add(data.Entity, 0);

            this.FireOnDamageDealt(data);
            this.DamagesDealt[data.Entity] += data.Value;
        }

        private void _updateShieldDamages(SpellEventData data)
        {
            if (!this.DamagesOnShield.ContainsKey(data.Entity))
                this.DamagesOnShield.Add(data.Entity, 0);

            this.FireOnShieldRemoved(data);
            this.DamagesOnShield[data.Entity] += data.Value;
        }

        private void _updateHealings(SpellEventData data)
        {
            if (!this.HealingDone.ContainsKey(data.Entity))
                this.HealingDone.Add(data.Entity, 0);

            this.FireOnHealGiven(data);
            this.HealingDone[data.Entity] += data.Value;
        }

        private void _updateAlterations(SpellEventData data)
        {
           
        }

        private void _updateSelfDamages(SpellEventData data)
        {
            this.SelfDamagesReceived += data.Value;
            this.FireOnDamageDealt(data);
        }
        private void _updateSelfShieldDamages(SpellEventData data)
        {
            this.SelfDamagesOverShieldReceived += data.Value;
            this.FireOnShieldRemoved(data);
        }
        
        private void _updateSelfHealings(SpellEventData data)
        {
            this.FireOnHealReceived(data);
            this.SelfHealingReceived += data.Value;
        }
    }
}
