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
        public Dictionary<CharacterEntity, int> DamagesDealt;
        public Dictionary<CharacterEntity, int> DamagesOnShield;
        public int SelfDamagesReceived = 0;
        public int SelfDamagesOverShieldReceived = 0;

        public Dictionary<CharacterEntity, int> HealingDone;
        public int SelfHealingReceived = 0;

        public Dictionary<CharacterEntity, StatModification> StatModified;
        public Dictionary<EntityStatistics, int> SelfStatModified;
        public Dictionary<CharacterEntity,EAlterationType> AlterationGiven;
        private CharacterEntity caster;
        private SpellAction _action;
        //Je le met en français psq jsuis vener mais c'est un SPELL result,
        //ça devrais pas être lié au caster/pas de diff entre le self healing et healing. Du moins, pas explicite.
        public void Init(List<CharacterEntity> targets, CharacterEntity caster, SpellAction action)
        {
            this.DamagesDealt = new Dictionary<CharacterEntity, int>();
            this.DamagesOnShield = new Dictionary<CharacterEntity, int>();
            this.HealingDone = new Dictionary<CharacterEntity, int>();

            this.StatModified = new Dictionary<CharacterEntity, StatModification>();
            this.SelfStatModified = new Dictionary<EntityStatistics, int>();
            this.AlterationGiven = new Dictionary<CharacterEntity, EAlterationType>();

            this.caster = caster;
            this._action = action;

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
                //Makes sense if u think about it (bitch)
                //entity.AUUUUUUUUUUUUUUUUUUUUGH();
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
            this._action.FireOnStatModified(data);
            this.StatModified[data.Entity].value += data.Value;
        }
        private void _updateSelfStat(SpellEventData data) 
        {
            if (!this.SelfStatModified.ContainsKey(data.Stat))
                this.SelfStatModified.Add(data.Stat,data.Value);
            this._action.FireOnStatModified(data);
            this.SelfStatModified[data.Stat] += data.Value;
        }
        private void _updateDamages(SpellEventData data)
        {
            if (!this.DamagesDealt.ContainsKey(data.Entity))
                this.DamagesDealt.Add(data.Entity, 0);
            this._action.FireOnDamageDealt(data);
            this.DamagesDealt[data.Entity] += data.Value;
        }

        private void _updateShieldDamages(SpellEventData data)
        {
            if (!this.DamagesOnShield.ContainsKey(data.Entity))
                this.DamagesOnShield.Add(data.Entity, 0);
            this._action.FireOnShieldRemoved(data);
            this.DamagesOnShield[data.Entity] += data.Value;
        }

        private void _updateHealings(SpellEventData data)
        {
            if (!this.HealingDone.ContainsKey(data.Entity))
                this.HealingDone.Add(data.Entity, 0);
            this._action.FireOnHealGiven(data);
            this.HealingDone[data.Entity] += data.Value;
        }

        private void _updateAlterations(SpellEventData data)
        {
           
        }

        private void _updateSelfDamages(SpellEventData data)
        {
            this.SelfDamagesReceived += data.Value;
            this._action.FireOnDamageDealt(data);
        }
        private void _updateSelfShieldDamages(SpellEventData data)
        {
            this.SelfDamagesOverShieldReceived += data.Value;
            this._action.FireOnShieldRemoved(data);
        }
        
        private void _updateSelfHealings(SpellEventData data)
        {
            this._action.FireOnHealReceived(data);
            this.SelfHealingReceived += data.Value;
        }

        
    }
}
