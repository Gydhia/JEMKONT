using DownBelow;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "PlaceableInteractable.asset", menuName = "DownBelow/Interactables/PlaceableInteractable", order = 1)]
public class PlaceableInteractablePreset : PlaceableItem
{
    public InteractablePreset Interactable;

    protected override CellState AffectingState => CellState.Interactable;

    public override void PlaceObject(Cell cell)
    {
        Interactable.Init(cell);
    }

    protected override void InstanciatePrevisualization(CellEventData data, int rotation)
    {
        PrevisualizationInstance = Instantiate(Interactable.ObjectPrefab, data.Cell.WorldPosition, Quaternion.identity).gameObject;
        PrevisualizationInstance.transform.Rotate(new Vector3(0, 1, 0), rotation);
    }

}
