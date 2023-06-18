using DownBelow;
using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaceableItem", menuName = "DownBelow/Inventory/PlaceableItem", order = 1)]
public class PlaceableItemPreset : PlaceableItem
{
    public GameObject ObjectToPlace;
    public CellState AffectingCellState;
    protected override CellState AffectingState => AffectingCellState;

    public override void PlaceObject(Cell cell)
    {
        PrevisualizationInstance.transform.SetParent(cell.transform);
    }

    protected override void InstanciatePrevisualization(CellEventData data)
    {
        PrevisualizationInstance = Instantiate(ObjectToPlace, data.Cell.WorldPosition, Quaternion.identity);
    }
}
