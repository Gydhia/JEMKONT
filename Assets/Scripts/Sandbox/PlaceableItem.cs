using DownBelow;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PlaceableItem : ItemPreset
{
    [HideInInspector] public GameObject PrevisualizationInstance;

    protected abstract CellState AffectingState { get; }

    public void Place(CellEventData data)
    {
        if (PrevisualizationInstance != null)
        {
            data.Cell.Datas.placeableOnCell = this;

            Destroy(PrevisualizationInstance);
            PlaceObject(data);

            try
            {
                var stack = GameManager.SelfPlayer.PlayerInventory.StorageItems.ToList().First(x => x.ItemPreset == this);
                stack.RemoveQuantity();
                PrevisualizationInstance = null;
                data.Cell.Datas.state = AffectingState;
                if (stack.Quantity <= 0)
                {
                    InputManager.Instance.OnNewCellHovered -= Previsualize;
                    InputManager.Instance.OnCellRightClickDown -= Place;
                }
            } catch
            {
                InputManager.Instance.OnNewCellHovered -= Previsualize;
                InputManager.Instance.OnCellRightClickDown -= Place;
            }
        }
    }

    /// <summary>
    /// Create the previsualization object. <b>Should always contain an affactation to the PrevisualizationInstance variable.</b> 
    /// </summary>
    /// <param name="data"></param>
    protected abstract void InstanciatePrevisualization(CellEventData data);

    /// <summary>
    /// Places down the object prefab. 
    /// </summary>
    /// <param name="cell">The data from the event that called the placing down.</param>
    public abstract void PlaceObject(CellEventData data);

    public void Previsualize(CellEventData data)
    {
        try
        {
            var stack = GameManager.SelfPlayer.PlayerInventory.StorageItems.ToList().First(x => x.ItemPreset == this);
            if (stack.Quantity <= 0)
            {
                InputManager.Instance.OnNewCellHovered -= Previsualize;
                InputManager.Instance.OnCellRightClickDown -= Place;
                return;
            }
        } catch (InvalidOperationException ex)
        {
            InputManager.Instance.OnNewCellHovered -= Previsualize;
            InputManager.Instance.OnCellRightClickDown -= Place;
            return;
        }

        if (Placeable(data.Cell))
        {
            if (PrevisualizationInstance == null)
            {
                InstanciatePrevisualization(data);
                if (PrevisualizationInstance.TryGetComponent<PlacedDownItem>(out var down))
                {
                    down.Placed = false;
                } else
                {
                    PrevisualizationInstance.AddComponent<PlacedDownItem>();
                }
            }
            PrevisualizationInstance.transform.position = data.Cell.WorldPosition;
            PrevisualizationInstance.SetActive(true);
        } else
        {
            PrevisualizationInstance.SetActive(false);
        }
    }

    public void StopPrevisualize()
    {
        if(PrevisualizationInstance != null)
        {
            Destroy(PrevisualizationInstance);
        }
    }

    public static bool Placeable(Cell cell)
    {
        return cell.Datas.state == CellState.Walkable;
    }
}
