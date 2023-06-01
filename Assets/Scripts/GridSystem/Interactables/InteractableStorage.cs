using DownBelow.Managers;
using DownBelow.UI.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class InteractableStorage : Interactable
    {
        public BaseStorage Storage;

        public override void Init(InteractablePreset InteractableRef, Cell RefCell)
        {
            base.Init(InteractableRef, RefCell);

            this.Storage = new BaseStorage();
            this.Storage.Init(this.InteractablePreset as StoragePreset, RefCell);
        }
        public override void Interact(Entity.PlayerBehavior player)
        {
            // Only show the ui if we're the one who asked
            if(player == GameManager.RealSelfPlayer)
            {
                UIManager.Instance.CurrentStorage.SetStorageAndShow(this.Storage);
            }
        }
    }
}