using DownBelow;
using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlaceableItem : ItemPreset
{
    [HideInInspector] public GameObject PrevisualizationInstance;

    protected abstract CellState AffectingState { get; }

    public void AskToPlace(InputAction.CallbackContext ctx)
    {
        if (GridManager.Instance.LastHoveredCell == null || 
            GridManager.Instance.LastHoveredCell.Datas.state != CellState.Walkable ||
            GridManager.Instance.LastHoveredCell.RefGrid.IsCombatGrid ||
            GridManager.Instance.LastHoveredCell.IsPlacementCell)
        {
            return;
        }

        if (PrevisualizationInstance != null)
        {
            UIManager.Instance.PlayerInventory.PressEIndicator.gameObject.SetActive(false);

            Destroy(PrevisualizationInstance);

            var item = GameManager.RealSelfPlayer.PlayerInventory.StorageItems.First(i => i.ItemPreset == this);

            if (item.Quantity <= 1)
            {
                InputManager.Instance.OnNewCellHovered -= Previsualize;
                PlayerInputs.player_interact.performed -= AskToPlace;
            }

            NetworkManager.Instance.GiftOrRemovePlayerItem(GameManager.RealSelfPlayer.UID, this, -1);

            var placeAction = new PlaceItemAction(GameManager.RealSelfPlayer, GridManager.Instance.LastHoveredCell);
            placeAction.Init(this);

            NetworkManager.Instance.EntityAskToBuffAction(placeAction);
        }
    }


    /// <summary>
    /// Local action called by network to place the objet
    /// </summary>
    public void PlaceLocal(Cell cell)
    {
        PlaceObject(cell);
        PrevisualizationInstance = null;
        cell.Datas.state = AffectingState;
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
    public abstract void PlaceObject(Cell data);

    public void Previsualize(CellEventData data)
    {
        try
        {
            var stack = GameManager.SelfPlayer.PlayerInventory.StorageItems.ToList().First(x => x.ItemPreset == this);
            if (stack.Quantity <= 0)
            {
                InputManager.Instance.OnNewCellHovered -= Previsualize;
                PlayerInputs.player_interact.performed -= AskToPlace;
                return;
            }
        } catch (InvalidOperationException ex)
        {
            Debug.LogError(ex.Message);

            InputManager.Instance.OnNewCellHovered -= Previsualize;
            PlayerInputs.player_interact.performed -= AskToPlace;
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
