using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EODE.Wonderland;

public class KoiPond : TempObject
{
    public override void Init(Cell attachedCell, int TurnsLeft, CharacterEntity RefEntity, NCEPreset prefab)
    {
        base.Init(attachedCell, TurnsLeft, RefEntity, prefab);
        foreach (var item in CombatManager.Instance.PlayingEntities)
        {
            item.OnEnteredCell += CheckIfOnCell;
        }
    }
    void CheckIfOnCell(CellEventData data)
    {
        CharacterEntity target = data.Cell.EntityIn;
        if(target == null) { return; }
        if (this.AttachedCell == target.EntityCell)
        {
            //We are on cell
            //TP
            Cell cellToTp = data.Cell.RefGrid.Cells.RandomWalkable(this.RefEntity.UID);
            while (cellToTp.Datas.state.HasFlag(CellState.NonWalkable))
            {
                cellToTp = data.Cell.RefGrid.Cells.RandomWalkable(this.RefEntity.UID);
            }

            if(GameManager.CombatActionsBuffer.Count > 0 && GameManager.CombatActionsBuffer[0].RefEntity == target && GameManager.CombatActionsBuffer[0] is MovementAction move)
            {
                move.ForceKillAction();
            }

            target.Teleport(cellToTp);
        }
    }
    public override void DestroyEntity(float timer = 0f)
    {
        foreach (var item in CombatManager.Instance.PlayingEntities)
        {
            item.OnEnteredCell -= CheckIfOnCell;
        }
        base.DestroyEntity();
    }
}
