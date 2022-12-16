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
        public event SpellEventData.Event OnHealGiven;
        public event SpellEventData.Event OnAlterationGiven;
        public event SpellEventData.Event OnShieldRemoved;
        public event SpellEventData.Event OnShieldAdded;
        /// <summary>
        /// Called whenever a spell modifies a stat. All info in data.
        /// </summary>
        public event SpellEventData.Event OnStatModified;
        [HideInInspector]
        public bool HasEnded;

        private List<CharacterEntity> _targets;
        private Spell _spellRef;

        public GameObject FXPrefab;
        public virtual void Execute(List<CharacterEntity> targets, Spell spellRef)
        {
            this._spellRef = spellRef;
            this._targets = targets;
            this.HasEnded = false;
            this.Result = new SpellResult();
            this.Result.Init(this._targets, this._spellRef.Caster,this);
            if(FXPrefab!= null) {
                for (int i = 0;i < targets.Count;i++) {
                    Destroy(Instantiate(FXPrefab,targets[i].transform.position,Quaternion.identity),6f);
                }
            }
            spellRef.Caster.SubToSpell(this);
        }
        public void FireOnDamageDealt(SpellEventData data) {
            OnDamageDealt?.Invoke(data);
        }
        public void FireOnHealReceived(SpellEventData data) {
            OnHealReceived?.Invoke(data);
        }
        public void FireOnHealGiven(SpellEventData data) {
            OnHealGiven?.Invoke(data);
        }
        public void FireOnAlterationGiven(SpellEventData data) {
            OnAlterationGiven?.Invoke(data);
        }
        public void FireOnShieldRemoved(SpellEventData data) {
            OnShieldRemoved?.Invoke(data);
        }
        public void FireOnShieldAdded(SpellEventData data) {
            OnShieldAdded?.Invoke(data);
        }
        public void FireOnStatModified(SpellEventData data) {
            switch (data.Stat) {
                case EntityStatistics.Health:
                    if(data.Value >= 0) {
                        OnDamageDealt?.Invoke(data);
                    } else {
                        OnHealGiven?.Invoke(data);
                    }
                    break;
                case EntityStatistics.Shield:
                    if(data.Value >= 0) {
                        OnShieldAdded?.Invoke(data);
                    } else {
                        OnShieldRemoved?.Invoke(data);
                    }
                    break;
                default:
                    OnStatModified?.Invoke(data);
                    break;
            }
        }
    }
}
