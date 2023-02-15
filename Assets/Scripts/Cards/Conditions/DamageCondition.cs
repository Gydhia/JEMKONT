using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.Spells {
    [CreateAssetMenu(menuName = "SpellConditions/Damage Condition")]
    public class DamageCondition : SpellConditionBase {
        public int DamageNecessary = 0;

        public override bool Validated(SpellResult currentResult) {
            return currentResult.DamagesDealt.Any(x => x.Value >= DamageNecessary);
        }
    }
}