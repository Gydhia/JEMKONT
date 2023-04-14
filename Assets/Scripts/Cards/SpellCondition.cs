using DownBelow.Spells.Alterations;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
{
    [CreateAssetMenu(menuName = "Cards/Condition_SO")]
    public class SpellCondition : SerializedScriptableObject
    {
        private SpellResult _currentResult;

        [BoxGroup("Damages")]
        [EnableIf("@!this.Dmgs")]
        public bool DmgsOverShield;
        [BoxGroup("Damages")]
        [EnableIf("@!this.DmgsOverShield")]
        public bool Dmgs;
        [BoxGroup("Damages")]
        [EnableIf("@this.Dmgs || this.DmgsOverShield")]
        public bool OneDamagedTarget;
        [BoxGroup("Damages")]
        [EnableIf("@this.Dmgs || this.DmgsOverShield")]
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
            this._currentResult = result;

            bool validated = false;

            if (this.Dmgs || this.DmgsOverShield)
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
            //  For the global damages dealt.
            if (this.Dmgs)
            {
                // If the condition is for one target, or every damages dealt
                int total = 0;
                foreach (int dmg in this._currentResult.DamagesDealt.Values)
                {
                    if (!this.OneDamagedTarget)
                        total += dmg;
                    else if (dmg > this.Damages)
                        return true;
                }
                foreach (int dmg in this._currentResult.DamagesOnShield.Values)
                {
                    if (!this.OneDamagedTarget)
                        total += dmg;
                    else if (dmg > this.Damages)
                        return true;
                }

                if (!this.OneDamagedTarget && total > this.Damages)
                    return true;
            }
            else if (this.DmgsOverShield)
            {
                // If the condition is for one target, or every damages dealt
                int total = 0;
                foreach (int dmgOverShield in this._currentResult.DamagesDealt.Values)
                {
                    if (!this.OneDamagedTarget)
                        total += dmgOverShield;
                    else if (dmgOverShield > this.Damages)
                        return true;
                }

                if (!this.OneDamagedTarget && total > this.Damages)
                    return true;
            }

            return false;
        }

        public bool CheckHealing()
        {
            int total = 0;
            foreach (int heal in this._currentResult.HealingDone.Values)
            {
                if (!this.OneHealedTarget)
                    total += heal;
                else if (heal > this.Heal)
                    return true;
            }

            return (!this.OneHealedTarget && total > this.Heal);
        }

        public bool CheckBuffs()
        {
            return false;
        }
    }
}