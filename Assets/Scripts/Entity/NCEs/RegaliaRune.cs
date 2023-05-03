using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegaliaRune : TempObject
{
    [Tooltip("Customize this in unity!")]
    public int range = 3;

    public override void Init(Cell attachedCell, int TurnsLeft, CharacterEntity RefEntity)
    {
        base.Init(attachedCell, TurnsLeft, RefEntity);
        foreach (var item in GameManager.Instance.Players)
        {
            item.Value.OnEnteredCell += CheckIfInRange;
        }
    }
    void CheckIfInRange(CellEventData data)
    {
        //Calculate the path between the cells, path.Count <= range applybuff.
    }
}
