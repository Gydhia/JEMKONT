using Jemkont.Entity;
using Jemkont.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageStrengthSpellAction : SpellAction {
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        base.Execute(targets,spellRef);
        targets[0].ApplyStat(EntityStatistics.Health,spellRef.Caster.Critical ? -spellRef.Caster.Strength * 2 : -spellRef.Caster.Strength,false);
    }
}
