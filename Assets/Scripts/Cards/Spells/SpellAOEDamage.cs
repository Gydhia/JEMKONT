using Jemkont.Entity;
using Jemkont.Managers;
using Jemkont.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellAOEDamage : SpellAction
{
    [Min(1)] public int Radius;
    [Min(0)] public int damage;
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        base.Execute(targets,spellRef);
        Debug.LogError("SPELL ERROR NOT CODED SEARCH TODO");
        //TODO: code AOE spells. Waiting for kiki.
    }
}
