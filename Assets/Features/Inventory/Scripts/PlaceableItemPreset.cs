using DownBelow;
using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            var stack = GameManager.Instance.SelfPlayer.PlayerInventory.StorageItems.ToList().First(x=>x.ItemPreset == this);
            stack.RemoveQuantity();
            instance = null;
            data.Cell.Datas.state = AffectingState;
            if (stack.Quantity <= 0)
            {
                InputManager.Instance.OnNewCellHovered -= Previsualize;
                InputManager.Instance.OnCellRightClickDown -= Place;
            }
        }
    }

    public void Previsualize(CellEventData data)
    {
        if (IPlaceable.Placeable(data.Cell))
        {
            if (instance == null)
            {
                instance = Instantiate(ObjectToPlace, data.Cell.WorldPosition, Quaternion.identity);
                if (instance.TryGetComponent<PlacedDownItem>(out var down))
                {
                    down.Placed = false;
                } else
                {
                    instance.AddComponent<PlacedDownItem>();
                }
            }
            instance.transform.position = data.Cell.WorldPosition;
            instance.SetActive(true);
        } else
        {
            instance.SetActive(false);
        }
    }
}
