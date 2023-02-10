using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.UI.Inventory;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI.Inventory
{
    public class UIPlayerInventory : MonoBehaviour
    {
        public UIInventoryItem ClassItem;
        public UIInventoryItem SelectedItem;

        public UIStorage PlayerStorage;
        public PlayerBehavior Holder;

        private InteractableStorage _nearestInteractable;
        public Button ToNearestStorageBtn;

        private void Awake()
        {
            GameManager.Instance.OnPlayersWelcomed += _initInventory;
        }

        private void _initInventory(GameEventData Data)
        {
            this.Holder = GameManager.Instance.SelfPlayer;

            this.PlayerStorage.SetStorageAndShow(Holder.PlayerInventory);

            this.Holder.OnEnteredCell += _updateChestInteract;
            this.ToNearestStorageBtn.onClick.AddListener(this.MoveToNearestStorage);
        }

        private void _updateChestInteract(CellEventData Data)
        {
            List<Cell> cells = GridUtility.GetSurroundingCells(CellState.Interactable, Data.Cell);
            foreach (Cell cell in cells)
            {
                if(cell.AttachedInteract != null && cell.AttachedInteract is InteractableStorage)
                {
                    InteractableStorage interactable = cell.AttachedInteract as InteractableStorage;
                    this._nearestInteractable = interactable;
                    this.ToNearestStorageBtn.gameObject.SetActive(true);

                    return;
                }
            }

            this._nearestInteractable = null;
            this.ToNearestStorageBtn.gameObject.SetActive(false);
        }

        public void MoveToNearestStorage()
        {
            foreach (var item in this.Holder.PlayerInventory.StorageItems)
            {
                this._nearestInteractable.Storage.AddItem(item.Key, item.Value);
            }
            for (int i = 0; i < this.Holder.PlayerInventory.StorageItems.Count; i++)
            {
                this.Holder.PlayerInventory.RemoveItem(
                    this.Holder.PlayerInventory.StorageItems.ElementAt(0).Key,
                    this.Holder.PlayerInventory.StorageItems.ElementAt(0).Value) ;
            }
            
        }
    }
}
