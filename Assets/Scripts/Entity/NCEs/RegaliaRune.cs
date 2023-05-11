using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegaliaRune : TempObject
{
    [Tooltip("Customize this in unity! (0 => only on the rune)")]
    public int range = 3;
    public int ArmorBoost = 2;

    private List<PlayerBehavior> playersInRange = new List<PlayerBehavior>();

    public override void Init(Cell attachedCell, int TurnsLeft, CharacterEntity RefEntity, NonCharacterEntity prefab)
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
            player.Statistics[EntityStatistics.Defense] += ArmorBoost;
            playersInRange.Add(player);
        } else if(playersInRange.Contains(player))
        {
            //NotInRangeAnymore.
            playersInRange.Remove(player);
            player.Statistics[EntityStatistics.Defense] -= ArmorBoost;
        }
    }
    public override void DestroyEntity()
    {
        foreach (var player in playersInRange)
        {
            player.Statistics[EntityStatistics.Defense] -= ArmorBoost;
        }
        foreach (var item in GameManager.Instance.Players)
        {
            item.Value.OnEnteredCell -= CheckIfInRange;
        }
        base.DestroyEntity();
    }
}
