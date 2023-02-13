using DownBelow.Spells.Alterations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace DownBelow.Managers {
    [ShowOdinSerializedPropertiesInInspector]
    public class SFXManager : _baseManager<SFXManager> {
        public Dictionary<EAlterationType, GameObject> AlterationSFX = new Dictionary<EAlterationType, GameObject>();

        public void RefreshAlterationSFX(Entity.CharacterEntity entity) {
            foreach (Alteration alt in entity.Alterations) {
                AlterationSFXToEntity(alt, entity);
            }
        }

        void AlterationSFXToEntity(Alteration alt, Entity.CharacterEntity ent) {
            if (alt.InstanciatedFXAnimator != null) {
                alt.InstanciatedFXAnimator.SetTrigger("Reminder");
            } else {
                alt.InstanciatedFXAnimator = Instantiate(AlterationSFX[alt.ToEnum()], ent.transform).GetComponent<Animator>();
            }
        }
    }

}
