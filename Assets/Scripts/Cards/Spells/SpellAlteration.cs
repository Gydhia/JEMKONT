using DownBelow.Entity;
using DownBelow.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EAlterationType {
    Stun,
    Snare,
    NoCounter,
    Critical,
    Camouflage,
    Provoke
}
public enum EDurationReferential {
    Caster,
    Targets
}
public class SpellAlteration : SpellAction {
    public EAlterationType AlterationType;
    [Tooltip("(in turns)"), Range(1,15)] public int Duration = 1;
    [Tooltip("Referential of turns: turns of whom counts towards the duration.")] public EDurationReferential DurationReferential;
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        base.Execute(targets,spellRef);
        Debug.LogError("SPELL ERROR: NOT CODED YET: NEED TO BE ABLE TO ALTER ENTITIES");
        // TODO : being able to alter entities:
        // Stun: no casts, no moves: you automatically skip ur turn.
        // Snare: speed is set and kept to 0.
        //No Counter: disable counter attacks!
        //Critical: next attack deals double damage!
        //Camouflage: not targetable until they do an action
        //TODO : Some alteration could have infinite duration but wears off out a specific condition? Ex: critical once u attack, camouflage when u do someth
    }
}
