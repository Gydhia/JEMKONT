using DownBelow.Entity;
using DownBelow.Managers;
using DownBelow.Mechanics;
using DownBelow.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class Interactable_P_Card : InteractablePurchase<ScriptableCard>
    {
        public UIExtensibleCard UICard;

        public override List<ScriptableCard> GetItemsPool()
        {
            return SettingsManager.Instance.ScriptableCards.Values.ToList().Except(SettingsManager.Instance.OwnedCards).ToList();
        }

        public override void Init(InteractablePreset InteractableRef, Cell RefCell)
        {
            base.Init(InteractableRef, RefCell);
            this.RefreshPurchase();
        }

        public override void GiveItemToPlayer(ScriptableCard Item)
        {
            if (SettingsManager.Instance.OwnedCards.Contains(Item))
                Debug.LogError(Item.name + " IS ALREADY IN THE OWNED CARDS");

            SettingsManager.Instance.OwnedCards.Add(Item);
        }

        protected override void RefreshPurchase()
        {
            var pooledCards = this.GetItemsPool();

            if(pooledCards == null || pooledCards.Count <= 0)
            {
                this.RefCell.ChangeCellState(CellState.Walkable);
                this.gameObject.SetActive(false);
                return;
            }

            var choosenCard = pooledCards[UnityEngine.Random.Range(0, pooledCards.Count)];

            this.Preset.ActualizeCosts();
            this.CurrentItemPurchase = choosenCard;
            this.UICard.Init(this.CurrentItemPurchase);
        }

        protected override void TryBuy(PlayerBehavior player)
        {
            if (this.CanBuy())
            {
                Debug.Log("Just bought : " + this.CurrentItemPurchase);
                this.GiveItemToPlayer(this.CurrentItemPurchase);

                foreach (var item in this.Preset.Costs)
                    player.PlayerInventory.RemoveItem(item.Key, item.Value);

                this.RefreshPurchase();
            }
            else
                Debug.Log("Can't buy this item, missing resources");
        }
    }
}