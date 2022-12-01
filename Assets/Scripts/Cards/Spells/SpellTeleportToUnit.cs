using Jemkont.Entity;
using Jemkont.Managers;
using Jemkont.Spells;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class SpellTeleportToUnit : SpellAction {
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        base.Execute(targets,spellRef);
        if (targets.Count != 1) {
            Debug.LogError("SPELL ERROR: Trying to teleport but targeted two entities.");
            return;//Tf goin on, do not teleport
        }
        GridPosition entityPos = new GridPosition(targets[0].EntityPosition.longitude,targets[0].EntityPosition.latitude);
        GridPosition Teleport = ClosestWalkableCellPosition(entityPos);
        if (Teleport.longitude == -1) {
            Debug.LogWarning("Spell Warning: no available positions to teleport to.");
            return;
        }
        GameManager.Instance.SelfPlayer.EntityPosition = Teleport;
    }
    GridPosition ClosestWalkableCellPosition(GridPosition to) {
        GridPosition[] positionsToCheck = new GridPosition[8] {
        new GridPosition(to.longitude,to.latitude+1),
        new GridPosition(to.longitude-1,to.latitude+1),
        new GridPosition(to.longitude-1,to.latitude),
        new GridPosition(to.longitude-1,to.latitude-1),
        new GridPosition(to.longitude,to.latitude-1),
        new GridPosition(to.longitude+1,to.latitude-1),
        new GridPosition(to.longitude+1,to.latitude),
        new GridPosition(to.longitude+1,to.latitude+1)
        };
        foreach (GridPosition pos in positionsToCheck) {
            if (GridManager.Instance.CellDataAtPosition(pos).state == Jemkont.GridSystem.CellState.Walkable) return pos;
        }
        return new GridPosition(-1,-1);//Normally impossible.
    }
}
