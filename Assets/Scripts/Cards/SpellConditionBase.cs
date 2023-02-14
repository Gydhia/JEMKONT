using DownBelow.Spells.Alterations;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells {
    public abstract class SpellConditionBase : SerializedScriptableObject {
        /// <summary>
        /// Checks if this condition is validated
        /// </summary>
        /// <param name="currentResult"> Current Spell Result.</param>
        /// <returns></returns>
        public abstract bool Validated(SpellResult currentResult);

    }
}
