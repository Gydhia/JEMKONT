using DownBelow.Entity;
using DownBelow.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellNurtureSoil : SpellAction {
    public int amount;
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        base.Execute(targets,spellRef);
        if (targets.Count > 1) return;
        targets[0].MaxHealth += amount;
        targets[0].ApplyStrenght(amount);
    }
}
