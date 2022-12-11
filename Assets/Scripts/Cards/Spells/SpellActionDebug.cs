using Jemkont.Entity;
using Jemkont.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellActionDebug : SpellAction {
    public string MessageToPrint;
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        Debug.Log(MessageToPrint);
        base.Execute(targets,spellRef);
    }
}
