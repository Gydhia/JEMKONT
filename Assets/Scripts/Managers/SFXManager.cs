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

namespace DownBelow.Managers {
    [ShowOdinSerializedPropertiesInInspector]
    public class SFXManager : _baseManager<SFXManager> 
        {
        public ScriptableSFXAlterationList AlterationSFXList;

        public void RefreshAlterationSFX(Entity.CharacterEntity entity) {
            foreach (Alteration alt in entity.Alterations) {
                AlterationSFXToEntity(alt, entity);
            }
        }

        void AlterationSFXToEntity(Alteration alt, Entity.CharacterEntity ent) {
            if (alt.InstanciatedFXAnimator != null) {
                alt.InstanciatedFXAnimator.SetTrigger("Reminder");
            } else {
                var go = Instantiate(AlterationSFXList.AlterationsSFX[alt.ToEnum()], ent.transform);
                alt.InstanciatedFXAnimator = go.GetComponent<Animator>();
            }
        }

        public async Task DOSFX(ScriptableSFX SfxData, CharacterEntity caster, Cell target, Spell spell)
        {
            if(SfxData == null) return;
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
                    proj = Instantiate(SfxData.Prefab, caster.transform.position, Quaternion.identity);
                    anim = proj.GetComponent<Animator>();
                    //entry state is "MidAir" for now.
                    await ProjectileGoTo(proj.transform, target.transform, SfxData.TravelDuration);
                    //Landed!
                    if (anim != null) anim.SetTrigger("Landed");
                    Destroy(proj, landTime);
                    break;
                case ESFXTravelType.Instantaneous:
                    Destroy(Instantiate(SfxData.Prefab, target.transform.position, Quaternion.identity), 6f);
                    //TODO: quaternion.LookRotation to target?
                    break;
                case ESFXTravelType.ProjectileFromEnemy:
                    proj = Instantiate(SfxData.Prefab, target.transform.position, Quaternion.identity);
                    anim = proj.GetComponent<Animator>();
                    //entry state is "MidAir" for now.
                    await ProjectileGoTo(proj.transform, caster.transform, SfxData.TravelDuration);
                    //Landed!
                    if (anim != null) anim.SetTrigger("Landed");
                    Destroy(proj, landTime);
                    break;
                case ESFXTravelType.ProjectileBackAndForth:
                    proj = Instantiate(SfxData.Prefab, caster.transform.position, Quaternion.identity);
                    anim = proj.GetComponent<Animator>();
                    //entry state is "MidAir" for now, going for target.
                    await ProjectileGoTo(proj.transform, target.transform, SfxData.TravelDuration);
                    //Landed for the first time! Going back.
                    if (anim != null) anim.SetTrigger("Landed");
                    await new WaitForSeconds(landTime);
                    if (anim != null) anim.SetTrigger("TookOff");
                    //state is "SecondMidAir" now, going for caster.
                    await ProjectileGoTo(proj.transform, caster.transform,SfxData.TravelDuration);
                    //Landed again!
                    if (anim != null) anim.SetTrigger("Landed");
                    Destroy(proj, landTime);
                    break;
            }
        }

        async Task ProjectileGoTo(Transform ProjTransform, Transform Target,float TravelDuration)
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
