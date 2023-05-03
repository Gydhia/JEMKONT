using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object that is temporary in combat.
/// </summary>
public class TempObject : MonoBehaviour
{
    Cell AttachedCell;
    int TurnsLeft;
    CharacterEntity RefEntity;

    public void Init(Cell attachedCell, int TurnsLeft, CharacterEntity RefEntity)
    {
        AttachedCell = attachedCell;
        this.TurnsLeft = TurnsLeft;
        this.RefEntity = RefEntity;
        this.RefEntity.OnTurnBegun += Decrement;
    }

    public void Decrement(GameEventData data)
    {
        TurnsLeft--;
        if (TurnsLeft <= -1)
        {
            AttachedCell.ChangeCellState(CellState.Walkable);
            RefEntity.OnTurnBegun -= Decrement; 
            Destroy(this.gameObject);
            //Aniamtion one day :pray:
        }
    }
}
