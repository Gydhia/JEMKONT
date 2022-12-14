using Sirenix.OdinInspector;
using System;
using DownBelow.Entity;
using DownBelow.Spells.Alterations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Spells {
    public class SpellAlteration : SpellAction {
        public EAlterationType AlterationType;
        [ShowIf("@AlterationType != EAlterationType.Critical && AlterationType != EAlterationType.Dodge")]
        [Tooltip("(in turns)"), Range(1,15)] public int Duration = 1;
        [ShowIf("@AlterationType == EAlterationType.SpeedUpDown || AlterationType == EAlterationType.DmgUpDown || AlterationType == EAlterationType.DoT")]
        [Tooltip("The value to (de)buff with. Some alterations do not have any.")] public int value;
        public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
            base.Execute(targets,spellRef);
            foreach (CharacterEntity target in targets) {
                target.AddAlteration(AlterationType,Duration,value);
            }
            //Debug.LogError("SPELL ERROR: NOT CODED YET: NEED TO BE ABLE TO ALTER ENTITIES");
            // TODO : Some alteration could have infinite duration but wears off out a specific condition? Ex: critical once u attack, camouflage when u do someth
        }
    }
    public class AlterationData {
        //Maybe lol
    }
}
