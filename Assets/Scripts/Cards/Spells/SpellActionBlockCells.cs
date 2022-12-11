using Jemkont.Entity;
using Jemkont.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// WARNING this is NOT intended to work on its own. We will inherit this in spells that block cells; WITH THEIR OWN FX.
/// </summary>
public abstract class SpellActionBlockCells : SpellAction {
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        Debug.LogError("SPELL ERROR: need to be able to select multiple cells and blocking them.");
        base.Execute(targets,spellRef);
    }
}
