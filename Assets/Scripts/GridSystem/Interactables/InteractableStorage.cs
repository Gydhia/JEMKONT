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
        // Can player put resources in this chest ?
        public bool OnlyTake = false;

        public override void Init(InteractablePreset InteractableRef, Cell RefCell)
        {
            base.Init(InteractableRef, RefCell);

            if(this.InteractablePreset is StoragePreset sPreset)
            {
                this.Storage = new BaseStorage();
                this.Storage.Init(sPreset, RefCell, this.OnlyTake);
            }

            // TODO : ok, that sucks
            if (this.OnlyTake)
            {
                GridManager.SavePurposeStorage = Storage;
            }
        }
        
        public void LoadStorage(StorageData storageData, WorldGrid grid) 
        {
            this.Storage = new BaseStorage(storageData, grid.Cells[storageData.PositionInGrid.latitude, storageData.PositionInGrid.longitude], this.OnlyTake);    
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