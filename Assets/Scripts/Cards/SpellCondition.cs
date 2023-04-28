using DownBelow.Entity;
using DownBelow.Spells.Alterations;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.Spells
{
    [CreateAssetMenu(menuName = "Cards/Condition_SO")]
    public class SpellCondition : SerializedScriptableObject
    {
        private SpellResult _currentResult;
        [HideInInspector]
        public List<CharacterEntity> TargetedEntities = new List<CharacterEntity>();

        [BoxGroup("Targeting")]
        public TargetType TargetType;
        //[BoxGroup("Targeting")]
        //public bool OnlyAffectedTarget;

        [BoxGroup("Damages")]
        public bool Dmgs;
        [BoxGroup("Damages")]
        [EnableIf("@this.Dmgs")]
        public bool OneDamagedTarget;
        [BoxGroup("Damages")]
        [EnableIf("@this.Dmgs")]
        public int Damages;

        [BoxGroup("Healing")]
        public bool Healing;
        [BoxGroup("Healing")]
        [EnableIf("@Healing")]
        public bool OneHealedTarget;
        [BoxGroup("Healing")]
        [EnableIf("@this.Healing")]
        public int Heal;

        [BoxGroup("Alterations")]
        public bool IsAltered;
        [BoxGroup("Alterations")]
        [Tooltip("If set to None, it will not be counted.")]
        [EnableIf("@this.IsAltered")]
        public EAlterationType Buff;

        public bool Check(SpellResult result)
        {
            // Since it's scriptable object, we need to clear it if used multiple times
            this.TargetedEntities.Clear();
            this._currentResult = result;

            bool validated = false;

            if (this.Dmgs)
                validated = this.CheckDamages();

            if (this.Healing)
                validated = this.CheckHealing();

            if (this.IsAltered)
                validated = this.CheckBuffs();

            this._currentResult = null;
            return validated;
        }

        public bool CheckDamages()
        {
            // If the condition is for one target, or every damages dealt
            bool dmgValidated = false;
            int total = 0;
            foreach (var dmg in this._currentResult.DamagesDealt)
            {
                if (!this.OneDamagedTarget)
                    total += dmg.Value;
                else if (dmg.Value > this.Damages)
                    return dmgValidated = true;

                this.TargetedEntities.Add(dmg.Key);
            }

            return dmgValidated || (!this.OneDamagedTarget && total > this.Damages);
        }

        public bool CheckHealing()
        {
            bool healValidated = false;
            int total = 0;
            foreach (var heal in this._currentResult.HealingDone)
            {
                if (!this.OneHealedTarget)
                    total += heal.Value;
                else if (heal.Value > this.Heal)
                    healValidated = true;

                this.TargetedEntities.Add(heal.Key);
            }

            return healValidated || (!this.OneHealedTarget && total > this.Heal);
        }

        public bool CheckBuffs()
        {
            return false;
        }

        public List<CharacterEntity> GetValidatedTargets()
        {
            switch (this.TargetType)
            {
                case TargetType.Self:

                    return new List<CharacterEntity> { this._currentResult.SpellRef.RefEntity };

                case TargetType.Enemy:

                    return this.TargetedEntities.Where(e => !e.IsAlly).ToList();

                case TargetType.Ally:

                    return this.TargetedEntities.Where(e => e.IsAlly).ToList();

                case TargetType.Entities:
                case TargetType.All:
                    return this.TargetedEntities;
                case TargetType.Empty:
                default:
                    return null; // Maybe that this shouldn't be selectable 
            }
        }
    }
}