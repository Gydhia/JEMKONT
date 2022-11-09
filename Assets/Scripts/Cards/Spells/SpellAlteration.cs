using Jemkont.Entity;
using Jemkont.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EAlterationType {
    Stun,
    Snare,
    NoCounter
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
        // TODO : being able to alter entities:
        // Stun: no casts, no moves: you automatically skip ur turn.
        // Snare: speed is set and kept to 0.
    }
}
