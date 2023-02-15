using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SpellStrangeMushroom : SpellDealer {
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        base.Execute(targets,spellRef);
        foreach (CharacterEntity target in targets) {
            SwapHealthPower(target);
        }
    }
    private void SwapHealthPower(CharacterEntity target) {
        var health = target.Health;
        var strength = target.Strength;
        var AmountOfHealthToApply = strength - health;
        var AmountOfStrengthToApply = -AmountOfHealthToApply;
        if (strength > target.MaxHealth) target.MaxHealth = strength;
        target.ApplyStat(EntityStatistics.Health,AmountOfHealthToApply,false);
        target.ApplyStat(EntityStatistics.Strength,AmountOfStrengthToApply);
        CombatManager.Instance.DrawCard();
    }
}
