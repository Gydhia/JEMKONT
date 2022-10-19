using Jemkont.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells
{
    public class SpellFireball : SpellAction
    {
        public int BaseDamages = 8;
        public float TimeToReach = 1f;

        // TODO : Change this to a VFX ;)
        public GameObject FireballPrefab;

        public override void Execute(List<CharacterEntity> targets, Spell spellRef)
        {
            base.Execute(targets, spellRef);

            foreach (CharacterEntity entity in targets)
            {
                StartCoroutine(LaunchFire(entity));
            }
        }

        public IEnumerator LaunchFire(CharacterEntity target)
        {
            GameObject fireball = Instantiate(this.FireballPrefab, this.transform);

            float timer = 0f;
            Vector3 basePos = fireball.transform.position;
            basePos.y = 1.5f;

            while(timer < this.TimeToReach)
            {
                float value = timer * (1 / this.TimeToReach);
                fireball.transform.position = Vector3.Lerp(basePos, target.transform.position, value);

                yield return new WaitForSeconds(Time.deltaTime);
                timer += Time.deltaTime;
            }

            target.ApplyHealth(-this.BaseDamages, false);
            this.HasEnded = true;
        }
    }
}