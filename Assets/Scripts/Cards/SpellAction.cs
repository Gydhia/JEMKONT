using DownBelow.Entity;
using DownBelow.Events;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DownBelow.Spells
{
    public abstract class SpellAction : MonoBehaviour
    {
        public SpellResult Result;
     
        public event SpellEventData.Event OnDamageDealt;
        public event SpellEventData.Event OnHealReceived;
        public event SpellEventData.Event OnBuffGiven;
        public event SpellEventData.Event OnShieldRemoved;
        public event SpellEventData.Event OnShieldAdded;

        [HideInInspector]
        public bool HasEnded;

        private List<CharacterEntity> _targets;
        private Spell _spellRef;

        public virtual void Execute(List<CharacterEntity> targets, Spell spellRef)
        {
            this._spellRef = spellRef;
            this._targets = targets;
            this.HasEnded = false;

            this.Result = new SpellResult();
            this.Result.Init(this._targets, this._spellRef.Caster);
        }
    }
}
