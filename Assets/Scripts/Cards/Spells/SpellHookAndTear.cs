using Jemkont.Entity;
using Jemkont.Managers;
using Jemkont.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellHookAndTear : SpellAction {
    [Min(1)] public int damageMultiplier = 3;
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        base.Execute(targets,spellRef);
        var damage = CombatManager.Instance.HandPile.Count * damageMultiplier;
        foreach (var item in targets) {
            item.ApplyHealth(-damage,false);
        }
    }
}
