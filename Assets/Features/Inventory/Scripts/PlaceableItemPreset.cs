using DownBelow;
using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaceableItem", menuName = "DownBelow/ScriptableObject/PlaceableItem", order = 1)]
public class PlaceableItemPreset : ItemPreset, IPlaceable
{
    public GameObject ObjectToPlace;
    GameObject instance;
    public CellState AffectingState;
    public void Place(CellEventData data)
    {
        if (instance != null)
        {
            instance.transform.SetParent(data.Cell.transform);
            data.Cell.Datas.placeableOnCell = this;

            var qty = GameManager.Instance.SelfPlayer.PlayerInventory.TryAddItem(this, -1);
            instance = null;
            data.Cell.Datas.state = AffectingState;
            if (qty <= 0)
            {
                InputManager.Instance.OnNewCellHovered -= Previsualize;
                InputManager.Instance.OnCellRightClickDown -= Place;
            }
        }
    }

    public void Previsualize(CellEventData data)
    {
        if (instance == null)
        {
            instance = Instantiate(ObjectToPlace, data.Cell.WorldPosition, Quaternion.identity);
        } else
        {
            instance.transform.position = data.Cell.WorldPosition;
        }
    }
}
