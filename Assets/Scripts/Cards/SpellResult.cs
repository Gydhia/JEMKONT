using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Spells.Alterations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public event SpellEventDataAlteration.Event OnAlterationGiven;
        /// <summary>
        /// Called whenever a spell modifies a stat. All info in data.
        /// </summary>
        public event SpellEventData.Event OnStatModified;

        public List<Cell> TargetedCells;
        private List<CharacterEntity> _targets;
        public Spell SpellRef;
        public CharacterEntity Caster;

        public virtual void Setup(List<CharacterEntity> targets, Spell spellRef)
        {
            Caster = spellRef.RefEntity;
            this.SpellRef = spellRef;
            this._targets = targets;
            this.Subscribe(this._targets, this.SpellRef.RefEntity);
            TargetedCells = new();

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
        public void FireOnAlterationGiven(SpellEventDataAlteration data)
        {
            OnAlterationGiven?.Invoke(data);
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
                default:
                    OnStatModified?.Invoke(data);
                    break;
            }
        }

        public Dictionary<CharacterEntity, int> DamagesDealt;
        public int SelfDamagesReceived = 0;

        public Dictionary<CharacterEntity, int> HealingDone;
        public int SelfHealingReceived = 0;

        public Dictionary<CharacterEntity, StatModification> StatModified;
        public Dictionary<EntityStatistics, int> SelfStatModified;
        public Dictionary<CharacterEntity,List<Alteration>> AlterationGiven;

        public List<CharacterEntity> TeleportedTo;

        public void Subscribe(List<CharacterEntity> targets, CharacterEntity caster)
        {
            this.DamagesDealt = new Dictionary<CharacterEntity, int>();
            this.HealingDone = new Dictionary<CharacterEntity, int>();

            this.StatModified = new Dictionary<CharacterEntity, StatModification>();
            this.SelfStatModified = new Dictionary<EntityStatistics, int>();
            this.AlterationGiven = new();

            this.TeleportedTo = new();

            foreach (CharacterEntity entity in targets)
            {
                if (entity == caster)
                    continue;

                entity.OnHealthRemoved += this._updateDamages;
                entity.OnHealthAdded += this._updateHealings;

                entity.OnStrengthAdded += this._updateStat;
                entity.OnStrengthRemoved += this._updateStat;
                entity.OnSpeedAdded += this._updateStat;
                entity.OnSpeedRemoved += this._updateStat;
                entity.OnAlterationReceived += this._updateAlterations;
                entity.OnAlterationGiven += caster.FireOnAlterationGiven;
            }

            caster.OnHealthRemoved += this._updateSelfDamages;
            caster.OnHealthAdded += this._updateSelfHealings;

            caster.OnStrengthAdded += this._updateSelfStat;
            caster.OnStrengthRemoved += this._updateSelfStat;

            caster.OnSpeedAdded += this._updateSelfStat;
        }

        /// <summary>
        /// Unsubscribe events
        /// </summary>
        public void Unsubscribe()
        {
            foreach (CharacterEntity entity in _targets)
            {
                if (entity == Caster)
                    continue;

                entity.OnHealthRemoved -= this._updateDamages;
                entity.OnHealthAdded -= this._updateHealings;

                entity.OnStrengthAdded -= this._updateStat;
                entity.OnStrengthRemoved -= this._updateStat;
                entity.OnSpeedAdded -= this._updateStat;
                entity.OnSpeedRemoved -= this._updateStat;
                entity.OnAlterationReceived -= this._updateAlterations;
                entity.OnAlterationGiven -= Caster.FireOnAlterationGiven;
            }

            Caster.OnHealthRemoved -= this._updateSelfDamages;
            Caster.OnHealthAdded -= this._updateSelfHealings;

            Caster.OnStrengthAdded -= this._updateSelfStat;
            Caster.OnStrengthRemoved -= this._updateSelfStat;

            Caster.OnSpeedAdded -= this._updateSelfStat;

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

        private void _updateHealings(SpellEventData data)
        {
            if (!this.HealingDone.ContainsKey(data.Entity))
                this.HealingDone.Add(data.Entity, 0);

            this.FireOnHealGiven(data);
            this.HealingDone[data.Entity] += data.Value;
        }

        private void _updateAlterations(SpellEventDataAlteration data)
        {
            if (!this.AlterationGiven.ContainsKey(data.Entity))
                this.AlterationGiven.Add(data.Entity, new());

            this.FireOnAlterationGiven(data);
            this.AlterationGiven[data.Entity].Add(data.Alteration);
        }

        private void _updateSelfDamages(SpellEventData data)
        {
            this.SelfDamagesReceived += data.Value;
            this.FireOnDamageDealt(data);
        }

        private void _updateSelfHealings(SpellEventData data)
        {
            this.FireOnHealReceived(data);
            this.SelfHealingReceived += data.Value;
        }
    }
}
