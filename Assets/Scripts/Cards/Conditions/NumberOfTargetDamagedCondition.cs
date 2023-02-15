using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.Spells {
    [CreateAssetMenu(menuName = "SpellConditions/Number of targets damaged")]
    public class NumberOfTargetsDamagedCondition : SpellConditionBase {
        [Tooltip("Checks if we damaged enough different targets in SpellResult.")]
        public int TargetsDamaged = 1;

        public override bool Validated(SpellResult currentResult) {
            return currentResult.DamagesDealt.Count() >= TargetsDamaged;
        }
    }
}