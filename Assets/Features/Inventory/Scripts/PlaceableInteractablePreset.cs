using DownBelow;
using DownBelow.Events;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "PlaceableInteractable.asset", menuName = "DownBelow/ScriptableObject/PlaceableInteractable", order = 1)]
public class PlaceableInteractablePreset : ItemPreset, IPlaceable
{
    public InteractablePreset Interactable;
    GameObject instance;

    public void Place(CellEventData data)
    {
        if (instance != null)
        {
            data.Cell.Datas.placeableOnCell = this;

            Destroy(instance);
            Interactable.Init(data.Cell);

            try
            {
                var stack = GameManager.Instance.SelfPlayer.PlayerInventory.StorageItems.ToList().First(x => x.ItemPreset == this);
                stack.RemoveQuantity();
                instance = null;
                data.Cell.Datas.state = Interactable.AffectingState;
                if (stack.Quantity <= 0)
                {
                    InputManager.Instance.OnNewCellHovered -= Previsualize;
                    InputManager.Instance.OnCellRightClickDown -= Place;
                }
            } catch {
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
                instance = Instantiate(Interactable.ObjectPrefab, data.Cell.WorldPosition, Quaternion.identity).gameObject;
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
