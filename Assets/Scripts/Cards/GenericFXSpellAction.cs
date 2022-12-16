using DownBelow.Entity;
using DownBelow.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class GenericFXSpellAction : SpellAction {
    public GameObject FX;
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        Debug.Log(targets.Count);
        for (int i = 0;i < targets.Count;i++) {
            Instantiate(FX,targets[i].transform.position,Quaternion.identity);
            Debug.Log("FX!");
        }
        base.Execute(targets,spellRef);
    }
}
