using DownBelow.Entity;
using DownBelow.Managers;
using DownBelow.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellDraw : SpellAction {
    [Min(0)] public int NumberOfCardsToDraw;
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        for (int i = 0;i < NumberOfCardsToDraw;i++) {
            CombatManager.Instance.DrawCard();
        }
    }
}
