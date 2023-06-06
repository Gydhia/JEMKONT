using DownBelow.Spells.Alterations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using DownBelow.Mechanics;
using DG.Tweening;
using EODE.Wonderland;
using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Spells;
using System.Threading.Tasks;
using DownBelow.Events;

namespace DownBelow.Managers
{
    [ShowOdinSerializedPropertiesInInspector]
    public class SFXManager : _baseManager<SFXManager>
    {
        public const string AlterationReminderState = "Reminder";
        public const float AlterationSFXDuration = 0.35f;

        public ScriptableSFXAlterationList AlterationSFXList;

        public async Task RefreshAlterationSFX(Entity.CharacterEntity entity)
        {
            foreach (Alteration alt in entity.Alterations)
            {
                await AlterationSFXToEntity(alt, entity);
            }
        }

        async Task AlterationSFXToEntity(Alteration alt, Entity.CharacterEntity ent)
        {
            if (alt.InstanciatedFXAnimator != null)
            {
                alt.InstanciatedFXAnimator.SetTrigger(AlterationReminderState);
                await new WaitForSeconds(AlterationSFXDuration);
            } else
            {
                //FirstAnimation
                if (AlterationSFXList.AlterationsSFX.TryGetValue(alt.GetType(), out var prefab))
                {
                    var go = Instantiate(prefab, ent.transform);
                    alt.InstanciatedFXAnimator = go.GetComponent<Animator>();
                    await new WaitForSeconds(AlterationSFXDuration);
                }
            }
        }

        public async Task DOSFX(RuntimeSFXData SfxData)
        {
            if (SfxData == null || SfxData.Prefab == null) return;
            GameObject proj = null;
            Animator anim = null;
            float landTime;

            if (anim != null)
            {
                landTime = (float)anim.GetStateDuration("Landed");
            } else
            {
                landTime = (float)0f;
            }

            switch (SfxData.TravelType)
            {
                case ESFXTravelType.ProjectileToEnemy:
                    proj = Instantiate(SfxData.Prefab, SfxData.caster.transform.position, Quaternion.identity);
                    anim = proj.GetComponent<Animator>();
                    //entry state is "MidAir" for now.
                    SfxData.OnSFXStarted?.Invoke(new SFXEventData(SfxData));
                    await ProjectileGoTo(proj.transform, SfxData.target.transform, SfxData.TravelDuration);
                    SfxData.OnSFXEnded?.Invoke(new SFXEventData(SfxData));
                    //Landed!
                    if (anim != null) anim.SetTrigger("Landed");
                    Destroy(proj, landTime);
                    break;
                case ESFXTravelType.Instantaneous:
                    SfxData.OnSFXStarted?.Invoke(new SFXEventData(SfxData));
                    Destroy(Instantiate(SfxData.Prefab, SfxData.target.transform.position, Quaternion.identity), 6f);
                    SfxData.OnSFXEnded?.Invoke(new SFXEventData(SfxData));
                    //TODO: quaternion.LookRotation to target?
                    break;
                case ESFXTravelType.ProjectileFromEnemy:
                    proj = Instantiate(SfxData.Prefab, SfxData.target.transform.position, Quaternion.identity);
                    anim = proj.GetComponent<Animator>();
                    //entry state is "MidAir" for now.
                    SfxData.OnSFXStarted?.Invoke(new SFXEventData(SfxData));
                    await ProjectileGoTo(proj.transform, SfxData.caster.transform, SfxData.TravelDuration);
                    SfxData.OnSFXEnded?.Invoke(new SFXEventData(SfxData));
                    //Landed!
                    if (anim != null) anim.SetTrigger("Landed");
                    Destroy(proj, landTime);
                    break;
                case ESFXTravelType.ProjectileBackAndForth:
                    proj = Instantiate(SfxData.Prefab, SfxData.caster.transform.position, Quaternion.identity);
                    anim = proj.GetComponent<Animator>();
                    //entry state is "MidAir" for now, going for target.
                    SfxData.OnSFXStarted?.Invoke(new SFXEventData(SfxData));
                    await ProjectileGoTo(proj.transform, SfxData.target.transform, SfxData.TravelDuration);
                    //Landed for the first time! Going back.
                    if (anim != null) anim.SetTrigger("Landed");
                    await new WaitForSeconds(landTime);
                    if (anim != null) anim.SetTrigger("TookOff");
                    //state is "SecondMidAir" now, going for caster.
                    await ProjectileGoTo(proj.transform, SfxData.caster.transform, SfxData.TravelDuration);
                    SfxData.OnSFXEnded?.Invoke(new SFXEventData(SfxData));
                    //Landed again!
                    if (anim != null) anim.SetTrigger("Landed");
                    Destroy(proj, landTime);
                    break;
                case ESFXTravelType.InPlaceProjectile:
                    proj = Instantiate(SfxData.Prefab, SfxData.caster.transform.position, Quaternion.identity);
                    proj.transform.DOLookAt(SfxData.target.transform.position, 0f);
                    var part = proj.transform.GetChild(0).GetComponent<ParticleSystem>();
                    var vel = part.velocityOverLifetime;
                    float dist = Mathf.Abs(SfxData.caster.transform.localPosition.x - SfxData.target.transform.localPosition.x)
                        + Mathf.Abs(SfxData.caster.transform.localPosition.z - SfxData.target.transform.localPosition.z);
                    float velocity = 5f / SfxData.TravelUnit * dist;
                    vel.x = velocity;
                    part.Play();
                    await new WaitForSeconds(SfxData.TravelDuration);
                    SfxData.OnSFXEnded?.Invoke(new SFXEventData(SfxData));
                    Destroy(proj, 3);
                    break;
            }
        }

        async Task ProjectileGoTo(Transform ProjTransform, Transform Target, float TravelDuration)
        {
            ProjTransform.transform.DOLookAt(Target.position, 0f);
            ProjTransform.transform.DOMoveX(Target.position.x, TravelDuration);
            ProjTransform.transform.DOMoveZ(Target.position.z, TravelDuration);
            //TODO : MoveY To have an arching projectile? Also, easings? tweaking? Polishing?
            await new WaitForSeconds(TravelDuration);
            ProjTransform.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

}