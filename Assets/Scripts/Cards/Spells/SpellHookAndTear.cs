using DownBelow.Entity;
using DownBelow.Managers;
using DownBelow.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellHookAndTear : SpellAction {
    [Min(1)] public int damageMultiplier = 3;
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        base.Execute(targets,spellRef);
        var damage = CombatManager.Instance.HandPile.Count * damageMultiplier;
        if (spellRef.Caster.Critical) damage *= 2;
        foreach (var item in targets) {
            item.ApplyStat(EntityStatistics.Health,-damage,false);
        }
    }
}
