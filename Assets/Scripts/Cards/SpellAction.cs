using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Jemkont.Spells
{
    public abstract class SpellAction : MonoBehaviour
    {
        public delegate UnityEvent SpellEvent();
     
        public SpellEvent OnDamageDealt;
        public SpellEvent OnHealReceived;
        public SpellEvent OnShieldRemoved;
        public SpellEvent OnShieldAdded;


        public bool ConditionsValidated(SpellResult result, SpellCondition condition)
        {
            if (condition == null)
                return true;

            return condition.Check(result);
        }

    }
}
