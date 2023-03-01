using DownBelow.Managers;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Sirenix.OdinInspector;
using DownBelow.Entity;
using DownBelow.Inventory;
using Unity.Mathematics;
using DownBelow.UI;
using UnityEditor;

namespace DownBelow.GridSystem
{
    public class Cell : MonoBehaviour
    {
        public BoxCollider Collider;

        public WorldGrid RefGrid;

        public InventoryItem ItemContained = null;
        [SerializeField] private GameObject ItemContainedObject = null;
        #region Datas
        public CellData Datas;

        public int gCost;
        public int hCost;

        public int fCost { get { return gCost + hCost; } }

        public Cell parent;
        public CharacterEntity EntityIn;
        public Interactable AttachedInteract;

        public GridPosition PositionInGrid => new GridPosition(this.Datas.widthPos, this.Datas.heightPos);
        public Vector3 WorldPosition => this.gameObject.transform.position;
        #endregion

        public void Init(int yPos, int xPos, CellState state, WorldGrid refGrid)
        {
            this.RefGrid = refGrid;

            this.name = "Cell[" + yPos + ", " + xPos + "]";

            this.Datas.heightPos = yPos;
            this.Datas.widthPos = xPos;

            this.Datas.state = state;

            float cellsWidth = SettingsManager.Instance.GridsPreset.CellsSize;

            this.Collider.size = new Vector3(cellsWidth, cellsWidth / 6f, cellsWidth);
        }

        public void AttachInteractable(Interactable linkedObject)
        {
            this.AttachedInteract = linkedObject;
        }

        public void ChangeCellState(CellState newState, bool force = false)
        {
            if (!force && this.Datas.state == newState)
                return;
            this.Datas.state = newState;
        }

        public void RefreshCell()
        {
            this.ChangeCellState(this.Datas.state, true);
        }

        public void DropDownItem(InventoryItem item)
        {
            ItemContained = item;
            ItemContainedObject = Instantiate(ItemContained.ItemPreset.DroppedItemPrefab, this.transform.position, quaternion.identity);
            //Mettre un animator sur le prefab pour le faire tourner ou jsp
#if UNITY_EDITOR
            EditorGUIUtility.PingObject(this);
            Selection.activeObject = this;
#endif
        }

        public void TryPickUpItem(PlayerBehavior player)
        {
            int qtyRemainingInItem = ItemContained.Quantity;
            if (ItemContained.ItemPreset is ToolItem)
            {
                qtyRemainingInItem -= player.PlayerSpecialSlot.TryAddItem(ItemContained.ItemPreset, ItemContained.Quantity);
                player.ActiveTool = (ToolItem)ItemContained.ItemPreset;
            } else
            {
                qtyRemainingInItem -= player.PlayerInventory.TryAddItem(ItemContained.ItemPreset, ItemContained.Quantity);
            }
            ItemContained.RemoveQuantity(qtyRemainingInItem);
            if (ItemContained.Quantity <= 0)
            {
                Destroy(ItemContainedObject);
                ItemContained = null; ItemContainedObject = null;
            }
#if UNITY_EDITOR
           // Debug.Log($"Actual Quantity : {ItemContained.Quantity}, Quantity returned: {qtyRemainingInItem}");
            EditorGUIUtility.PingObject(this);
            Selection.activeObject = this;
#endif
        }
    }

    [System.Serializable]
    public class CellData
    {
        public CellData(int yPos, int xPos, CellState state)
        {
            this.heightPos = yPos;
            this.widthPos = xPos;
            this.state = state;
        }
        [ShowInInspectorAttribute]
        public int heightPos { get; set; }
        [ShowInInspectorAttribute]
        public int widthPos { get; set; }
        [ShowInInspectorAttribute]
        public CellState state { get; set; }
    }

    [System.Serializable]
    public enum CellState
    {
        [EnumMember(Value = "Walkable")]
        Walkable = 0,
        [EnumMember(Value = "Blocked")]
        Blocked = 1,
        [EnumMember(Value = "EntityIn")]
        EntityIn = 2,
        [EnumMember(Value = "Shared")]
        Shared = 3,
        [EnumMember(Value = "Interactable")]
        Interactable = 4
    }

}