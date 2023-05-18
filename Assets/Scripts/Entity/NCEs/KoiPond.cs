using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        var player = data.Cell.EntityIn;
        if (this.AttachedCell == player.EntityCell)
        {
            //We are on cell
            //TP
            
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
