using DG.Tweening;
using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Spells;
using EODE.Wonderland;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Mechanics {
    public enum ESFXTravelType {
        Instantaneous,
        ProjectileToEnemy,
        //We can imagine others:
        ProjectileFromEnemy,
        ProjectileBackAndForth,

    }
    [CreateAssetMenu(menuName = "SpellSFX")]
    public class ScriptableSFX : ScriptableObject {
        public GameObject Prefab;
        [Tooltip("This also defines when the spells are going to be applied.")]
        public ESFXTravelType TravelType;
        //Sounds too, one day,maybe...
        [EnableIf("@TravelType != ESFXTravelType.Instantaneous")]
        public float TravelDuration = 0.35f;

        async Task ProjectileGoTo(Transform ProjTransform, Transform Target)
        {
            ProjTransform.transform.DOLookAt(Target.position, 0f);
            ProjTransform.transform.DOMoveX(Target.position.x, TravelDuration);
            ProjTransform.transform.DOMoveZ(Target.position.z, TravelDuration);
            //TODO : MoveY To have an arching projectile? Also, easings? tweaking? Polishing?
            await new WaitForSeconds(TravelDuration);
            ProjTransform.GetComponent<Rigidbody>().isKinematic = false;
        }

        public virtual async Task DoSFX(CharacterEntity caster, Cell target, Spell spell)
        {
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

            switch (TravelType)
                {
                    case ESFXTravelType.ProjectileToEnemy:
                        proj = Instantiate(Prefab, caster.transform.position, Quaternion.identity);
                        anim = proj.GetComponent<Animator>();
                        //entry state is "MidAir" for now.
                        await ProjectileGoTo(proj.transform, target.transform);
                        //Landed!
                        if (anim != null) anim.SetTrigger("Landed");
                        Destroy(proj, landTime);
                        break;
                    case ESFXTravelType.Instantaneous:
                        Destroy(Instantiate(Prefab, target.transform.position, Quaternion.identity), 6f);
                        //TODO: quaternion.LookRotation to target?
                        break;
                    case ESFXTravelType.ProjectileFromEnemy:
                        proj = Instantiate(Prefab, target.transform.position, Quaternion.identity);
                        anim = proj.GetComponent<Animator>();
                        //entry state is "MidAir" for now.
                        await ProjectileGoTo(proj.transform, caster.transform);
                        //Landed!
                        if (anim != null) anim.SetTrigger("Landed");
                        Destroy(proj, landTime);
                        break;
                    case ESFXTravelType.ProjectileBackAndForth:
                        proj = Instantiate(Prefab, caster.transform.position, Quaternion.identity);
                        anim = proj.GetComponent<Animator>();
                        //entry state is "MidAir" for now, going for target.
                        await ProjectileGoTo(proj.transform, target.transform);
                        //Landed for the first time! Going back.
                        if (anim != null) anim.SetTrigger("Landed");
                        await new WaitForSeconds(landTime);
                        if (anim != null) anim.SetTrigger("TookOff");
                        //state is "SecondMidAir" now, going for caster.
                        await ProjectileGoTo(proj.transform, caster.transform);
                        //Landed again!
                        if (anim != null) anim.SetTrigger("Landed");
                        Destroy(proj, landTime);
                        break;
                }
        }
    }
}
