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

[CreateAssetMenu(fileName = "PlaceableItem", menuName = "DownBelow/ScriptableObject/PlaceableItem", order = 1)]
public class PlaceableItemPreset : PlaceableItem
{
    public GameObject ObjectToPlace;
    public CellState AffectingCellState;
    protected override CellState AffectingState => AffectingCellState;

    public override void PlaceObject(CellEventData data)
    {
        PrevisualizationInstance.transform.SetParent(data.Cell.transform);
    }

    protected override void InstanciatePrevisualization(CellEventData data)
    {
        PrevisualizationInstance = Instantiate(ObjectToPlace, data.Cell.WorldPosition, Quaternion.identity);
    }
}
