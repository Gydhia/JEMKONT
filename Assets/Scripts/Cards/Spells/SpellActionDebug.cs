using DownBelow.Entity;
using DownBelow.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellDealerDebug : SpellDealer {
    public string MessageToPrint;
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        Debug.Log(MessageToPrint);
        base.Execute(targets,spellRef);
    }
}
