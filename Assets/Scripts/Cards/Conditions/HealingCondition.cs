using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace DownBelow.Spells {

    [CreateAssetMenu(menuName = "SpellConditions/Healing Required")]
    public class HealingCondition : SpellConditionBase {
        public int HealingRequired;
        public override bool Validated(SpellResult currentResult) {
            return currentResult.HealingDone.Any(x=>x.Value >= HealingRequired);
        }
    }
}
