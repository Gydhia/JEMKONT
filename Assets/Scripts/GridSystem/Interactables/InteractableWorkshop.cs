using DownBelow.Entity;
using DownBelow.Managers;
using DownBelow.UI.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public abstract class InteractableWorkshop : InteractableStorage
    {
        public abstract string WorkshopName();

        public ItemPreset FuelItem;
        public ItemPreset InputItem;
        public ItemPreset OutputItem;

        // We voluntary override the InteractableStorage Init and Itneract
        public override void Init(InteractablePreset InteractableRef, Cell RefCell)
        {
            base.Init(InteractableRef, RefCell);

            this.Storage = new BaseStorage();
            this.Storage.Init(3, RefCell);
        }

        public override void Interact(PlayerBehavior p)
        {
            UIManager.Instance.WorkshopSection.ShowWorkshop(this, p == GameManager.RealSelfPlayer);
        }
    }
}