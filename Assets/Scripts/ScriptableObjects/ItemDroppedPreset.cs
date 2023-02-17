using DownBelow;
using DownBelow.GridSystem;
using DownBelow.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "DownBelow/ScriptableObject/Interactables/ItemDropped")]
public class ItemDroppedPreset : BaseSpawnablePreset
{
    public ItemPreset Item;
    public int quantity;

    public override void Init(Cell attachedCell)
    {
        InventoryItem InvInt = new InventoryItem();
        InvInt.Init(Item, 0, quantity);
        attachedCell.DropDownItem(InvInt);
    }
}
