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
        if (this.AttachedCell == target.EntityCell)
        {
            //We are on cell
            //TP
            Cell cellToTp = data.Cell.RefGrid.Cells.Random();
            while (cellToTp.Datas.state.HasFlag(CellState.NonWalkable))
            {
                cellToTp = data.Cell.RefGrid.Cells.Random();
            }
            target.Teleport(cellToTp);
        }
    }
    public override void DestroyEntity()
    {
        foreach (var item in CombatManager.Instance.PlayingEntities)
        {
            item.OnEnteredCell -= CheckIfOnCell;
        }
        base.DestroyEntity();
    }
}
