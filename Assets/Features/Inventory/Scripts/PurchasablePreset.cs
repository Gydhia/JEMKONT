using DownBelow;
using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.UI.Inventory;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PurchasablePreset", menuName = "DownBelow/Interactables/Purchasable", order = 2)]
public class PurchasablePreset : InteractablePreset
{
    public bool RandomizeItem;
    public bool RandomizeCost;

    public EClass SpecificClass;

    public ParticleSystem OrbParticlePrefab;

    public Dictionary<ItemPreset, int> Costs;

    [ShowIf("@RandomizeCost"),MinMaxSlider(0, 30, true)]
    public Vector2Int CostsRange = new(2,4);

    public void ActualizeCosts()
    {
        if (!this.RandomizeCost)
            return;

        for (int i = 0;i < this.Costs.Count;i++)
        {
            string UID = DownBelow.Managers.GameManager.MasterPlayer.UID;

            int randomCost = RandomHelper.RandInt(this.CostsRange.x, this.CostsRange.y, UID);

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
