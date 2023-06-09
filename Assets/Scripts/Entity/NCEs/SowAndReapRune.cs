using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SowAndReapRune : TempObject
{
    [Tooltip("Customize this in unity! (0 => only on the rune)")]
    public int range = 3;
    public BuffAlteration Buff;
    private List<PlayerBehavior> playersInRange = new List<PlayerBehavior>();

    public override void Init(Cell attachedCell, int TurnsLeft, CharacterEntity RefEntity,NCEPreset prefab)
    {
        base.Init(attachedCell, TurnsLeft, RefEntity,prefab);
        foreach (var item in GameManager.Instance.Players)
        {
            item.Value.OnEnteredCell += CheckIfInRange;
        }
    }
    void CheckIfInRange(CellEventData data)
    {
        var player = (PlayerBehavior)data.Cell.EntityIn;
        //Calculate the path between the cells, path.Count-1 <= range applybuff.
        //TODO: ApplyBuff!
        if (GridManager.Instance.FindPath(data.Cell.EntityIn, AttachedCell.PositionInGrid, true).Count - 1 <= range)
        {
            //We are in range yippeeee
            playersInRange.Add(player);
        } else if (playersInRange.Contains(player))
        {
            //NotInRangeAnymore.
            playersInRange.Remove(player);
        }
    }

    public override void Decrement(GameEventData data)
    {
        TurnsLeft--;
        if (TurnsLeft <= -1)
        {
            DestroyWithBoost();
        }
    }

    public void DestroyWithBoost()
    {
        //TODO: Add buff?
        foreach(var player in playersInRange)
        {
            player.AddAlteration(Buff);
        }
        DestroyEntity();
    }

    public override void DestroyEntity(float timer = 0f)
    {
        foreach (var item in GameManager.Instance.Players)
        {
            item.Value.OnEnteredCell -= CheckIfInRange;
        }
        base.DestroyEntity();
    }
}
