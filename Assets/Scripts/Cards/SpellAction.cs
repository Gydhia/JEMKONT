using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Mechanics;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using EODE.Wonderland;

namespace DownBelow.Spells {
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
        public ScriptableSFX SFXData;

        public virtual void Execute(List<CharacterEntity> targets, Spell spellRef)
        {
            this._spellRef = spellRef;
            this._targets = targets;
            this.HasEnded = false;
            this.Result = new SpellResult();
            this.Result.Init(this._targets, this._spellRef.Caster, this);

            spellRef.Caster.SubToSpell(this);
        }
        async Task ProjectileGoTo(Transform ProjTransform, Transform Target, float TravelTime) {
            ProjTransform.transform.DOLookAt(Target.position, 0f);
            ProjTransform.transform.DOMoveX(Target.position.x, TravelTime);
            ProjTransform.transform.DOMoveZ(Target.position.z, TravelTime);
            //TODO : MoveY To have an arching projectile? Also, easings? tweaking? Polishing?
            await new WaitForSeconds(TravelTime);
            ProjTransform.GetComponent<Rigidbody>().isKinematic = false;
        }
        public virtual async Task DoSFX(CharacterEntity caster, Cell target, Spell spell) {
            GameObject proj = null;
            Animator anim = null;
            float TravelTime = SFXData.TravelTime;
            float landTime;
            if (anim != null) {
                landTime = (float)anim.GetStateDuration("Landed");
            } else {
                landTime = (float)0f;
            }
            
            if (SFXData != null) switch (SFXData.TravelType) {
                    case ESFXTravelType.ProjectileToEnemy:
                        proj = Instantiate(SFXData.Prefab, caster.transform.position, Quaternion.identity);
                        anim = proj.GetComponent<Animator>();
                        //entry state is "MidAir" for now.
                        await ProjectileGoTo(proj.transform, target.transform, TravelTime);
                        //Landed!
                        if (anim != null) anim.SetTrigger("Landed");
                        Destroy(proj, landTime);
                        break;
                    case ESFXTravelType.Instantaneous:
                        //prout
                        Destroy(Instantiate(SFXData.Prefab, spell.ApplyToSelf?caster.transform.position:target.transform.position, Quaternion.identity), 6f);
                        //TODO: quaternion.LookRotation to target?
                        break;
                    case ESFXTravelType.ProjectileFromEnemy:
                        proj = Instantiate(SFXData.Prefab, target.transform.position, Quaternion.identity);
                        anim = proj.GetComponent<Animator>();
                        //entry state is "MidAir" for now.
                        await ProjectileGoTo(proj.transform, caster.transform, TravelTime);
                        //Landed!
                        if (anim != null) anim.SetTrigger("Landed");
                        Destroy(proj, landTime);
                        break;
                    case ESFXTravelType.ProjectileBackAndForth:
                        proj = Instantiate(SFXData.Prefab, caster.transform.position, Quaternion.identity);
                        anim = proj.GetComponent<Animator>();
                        //entry state is "MidAir" for now, going for target.
                        await ProjectileGoTo(proj.transform, target.transform, TravelTime);
                        //Landed for the first time! Going back.
                        if (anim != null) anim.SetTrigger("Landed");
                        await new WaitForSeconds(landTime);
                        if (anim != null) anim.SetTrigger("TookOff");
                        //state is "SecondMidAir" now, going for caster.
                        await ProjectileGoTo(proj.transform, caster.transform, TravelTime);
                        //Landed again!
                        if (anim != null) anim.SetTrigger("Landed");
                        Destroy(proj, landTime);
                        break;
                }
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
                    if (data.Value >= 0) {
                        OnDamageDealt?.Invoke(data);
                    } else {
                        OnHealGiven?.Invoke(data);
                    }
                    break;
                case EntityStatistics.Shield:
                    if (data.Value >= 0) {
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
