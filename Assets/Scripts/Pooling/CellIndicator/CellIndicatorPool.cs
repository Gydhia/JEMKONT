using DownBelow.Entity;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellIndicatorPool : ObjectPool<CellIndicator>
{
    public override CellIndicator GetPooled()
    {
        return base.GetPooled();
    }
    public override void ReleasePooled(CellIndicator poolObject)
    {
        base.ReleasePooled(poolObject);
    }

    public void DisplayIndicatorsFromBuffer()
    {
        //TODO : we'll see :)
        if (GameManager.NormalActionsBuffer.TryGetValue(GameManager.Instance.SelfPlayer, out List<EntityAction> SelfBuffer))
        {
            if (SelfBuffer[0] is MovementAction movement)
            {
                CellIndicator DestinationIndicator = GetPooled();
                DestinationIndicator.transform.position = movement.TargetCell.transform.position;
            }
        }
    }

    public List<CellIndicator> DisplayIndicators(List<DownBelow.GridSystem.Cell> cells, Color color)
    {
        List<CellIndicator> res = new();
        foreach (var cell in cells)
        {
            CellIndicator DestinationIndicator = GetPooled();
            DestinationIndicator.transform.position = cell.transform.position;
            DestinationIndicator.Color = color;
            res.Add(DestinationIndicator);
        }
        return res;
    }
}
