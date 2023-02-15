using DownBelow.Entity;
using DownBelow.Managers;
using DownBelow.Spells;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class SpellTeleportToUnit : SpellDealer {
    public override void Execute(List<CharacterEntity> targets,Spell spellRef) {
        base.Execute(targets,spellRef);
        if (targets.Count != 1) {
            Debug.LogError("SPELL ERROR: Trying to teleport but targeted two entities.");
            return;//Tf goin on, do not teleport
        }
        GridPosition entityPos = targets[0].EntityCell.PositionInGrid;
        GridPosition Teleport = ClosestWalkableCellPosition(entityPos);
        if (Teleport.longitude == -1) {
            Debug.LogWarning("Spell Warning: no available positions to teleport to.");
            return;
        }
        GameManager.Instance.SelfPlayer.EntityCell = targets[0].CurrentGrid.Cells[Teleport.latitude, Teleport.longitude];
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
            if (GridManager.Instance.CellDataAtPosition(pos).state == DownBelow.GridSystem.CellState.Walkable) return pos;
        }
        return new GridPosition(-1,-1);//Normally impossible.
    }
}
