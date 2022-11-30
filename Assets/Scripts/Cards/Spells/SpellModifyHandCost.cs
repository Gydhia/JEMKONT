using Jemkont.Entity;
using Jemkont.Managers;
using Jemkont.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellModifyHandCost : SpellAction
{
     public int howMuch;
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        base.Execute(targets,spellRef);
        foreach (CardComponent v in CombatManager.Instance.HandPile) {
            v.ApplyCost(howMuch);
        }
    }
}
