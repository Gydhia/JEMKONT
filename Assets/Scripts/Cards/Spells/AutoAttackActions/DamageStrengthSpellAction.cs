using DownBelow.Entity;
using DownBelow.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageStrengthSpellDealer : SpellDealer {
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        base.Execute(targets,spellRef);
        targets[0].ApplyStat(EntityStatistics.Health,spellRef.Caster.Critical ? (-spellRef.Caster.Strength + targets[0].Defense) * 2 : (-spellRef.Caster.Strength + targets[0].Defense),false);
    }
}
