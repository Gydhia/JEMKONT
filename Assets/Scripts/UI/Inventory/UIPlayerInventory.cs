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
        public UIInventoryTool ClassItem;
        public UIInventoryItem SelectedItem;

        public UIStorage PlayerStorage;
        public UIStorage PlayerSpecialStorage;
        public PlayerBehavior Holder;

        private InteractableStorage _nearestInteractable;
        public Button ToNearestStorageBtn;
        
        
        [SerializeField] private GameObject r_Clickinput;


        private void Awake()
        {
            GameManager.Instance.OnGameStarted += _initInventory;

            GameManager.Instance.OnEnteredGrid += _toggleInventoryUI;
            GameManager.Instance.OnExitingGrid += _toggleInventoryUI;
        }

        private void _toggleInventoryUI(EntityEventData Data) 
        {
            if (Data.Entity != GameManager.RealSelfPlayer)
                return;

            this.gameObject.SetActive(!Data.Entity.CurrentGrid.IsCombatGrid);
        }

        private void _initInventory(GameEventData Data)
        {
            this.Holder = GameManager.SelfPlayer;

            this.PlayerStorage.SetStorageAndShow(Holder.PlayerInventory);
            this.PlayerSpecialStorage.SetStorageAndShow(Holder.PlayerSpecialSlots);

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
                if(item.ItemPreset != null)
                {
                    int remainings = this._nearestInteractable.Storage.TryAddItem(item.ItemPreset, item.Quantity);
                    this.Holder.PlayerInventory.RemoveItem(item.ItemPreset, item.Quantity - remainings);
                }
            }            
        }
        
        public void SetInputUI(bool showInput)
        {
            r_Clickinput.SetActive(showInput);
        }
    }
}
