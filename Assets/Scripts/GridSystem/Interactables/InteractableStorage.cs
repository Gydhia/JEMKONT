using DownBelow.Managers;
using DownBelow.UI.Inventory;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class InteractableStorage : Interactable
    {
        public BaseStorage Storage;

        public override void Init(InteractablePreset InteractableRef, Cell RefCell)
        {
            base.Init(InteractableRef, RefCell);

            if(this.InteractablePreset is StoragePreset sPreset)
            {
                this.Storage = new BaseStorage();
                this.Storage.Init(sPreset, RefCell);
            }   
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