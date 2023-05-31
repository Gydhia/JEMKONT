using DownBelow;
using DownBelow.GridSystem;
using DownBelow.UI.Inventory;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PurchasablePreset", menuName = "DownBelow/ScriptableObject/Interactables/Purchasable", order = 2)]
public class PurchasablePreset : InteractablePreset
{
    public bool RandomizeItem;
    public bool RandomizeCost;

    public Dictionary<ItemPreset, int> Costs;
    [ShowIf("@RandomizeCost")]
    public List<int[]> CostsRange;

    private void OnValidate()
    {
        
    }

    public void ActualizeCosts()
    {
        if (!this.RandomizeCost)
            return;

        for (int i = 0; i < this.Costs.Count; i++)
        {
            int randomCost = UnityEngine.Random.Range(this.CostsRange[i][0], this.CostsRange[i][1]);

            var key = Costs.ElementAt(i).Key;
            this.Costs[key] = randomCost;
        }
    }

    public bool ValidateCosts(BaseStorage Storage)
    {
        foreach (var item in this.Costs)
        {
            if (Storage.StorageItems.Where(i => i.ItemPreset == item.Key).Sum(i => i.Quantity) < item.Value)
                return false;
        }

        return true;
    }
}
