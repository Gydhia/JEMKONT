using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Spells {

    [CreateAssetMenu(menuName = "SpellConditions/No Operator")]
    public class NoOperatorCondition : SpellConditionBase {
        public SpellConditionBase condition;
        public override bool Validated(SpellResult currentResult) {
            return !condition.Validated(currentResult);
        }
    }
}
