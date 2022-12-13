using Jemkont.Entity;
using Jemkont.Spells.Alterations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Jemkont.Spells {
    public class SpellAlteration : SpellAction {
        public EAlterationType AlterationType;
        [Tooltip("(in turns)"), Range(1,15)] public int Duration = 1;
        public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
            base.Execute(targets,spellRef);
            foreach (CharacterEntity target in targets) {
                target.AddAlteration(AlterationType,Duration);
            }

            //Debug.LogError("SPELL ERROR: NOT CODED YET: NEED TO BE ABLE TO ALTER ENTITIES");
            // TODO : Some alteration could have infinite duration but wears off out a specific condition? Ex: critical once u attack, camouflage when u do someth
        }
    }
    public class AlterationData {
        //Maybe lol
    }
}
