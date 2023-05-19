using DownBelow.Entity;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DownBelow.GridSystem
{
    public abstract class InteractablePurchase<T> : Interactable
    {
        protected PurchasablePreset Preset;

        public T CurrentItemPurchase;

        public GameObject Podium;
        public GameObject Purchasable;

        public override void Init(InteractablePreset InteractableRef, Cell RefCell)
        {
            base.Init(InteractableRef, RefCell);
            this.Preset = ((PurchasablePreset)this.InteractablePreset);

            this.transform.rotation = Quaternion.LookRotation(this.transform.position - Camera.main.transform.position) * Quaternion.Euler(0, 90, 0);
        }

        public override void OnFocused()
        {
            base.OnFocused();
            // TODO show tooltip of card i.e.
        }

        public override void OnUnfocused()
        {
            base.OnUnfocused();
        }

        protected bool CanBuy()
        {
            return this.Preset.ValidateCosts(UIManager.Instance.PlayerInventory.PlayerStorage.Storage);
        }
        public override void Interact(PlayerBehavior p)
        {
            this.TryBuy(p);
        }

        protected abstract void TryBuy(PlayerBehavior player);
        protected abstract void RefreshPurchase();

        public abstract void GiveItemToPlayer(T Item);
        public abstract List<T> GetItemsPool();
    }
}