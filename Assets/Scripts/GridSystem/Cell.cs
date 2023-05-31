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
using System;
using System.ComponentModel;

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
        private CharacterEntity _entityIn;
        public NonCharacterEntity AttachedNCE;
        public bool hasNCE => AttachedNCE != null;
        public CharacterEntity EntityIn
        {
            get { return _entityIn; }
            set
            {
                this.Datas.state = value != null ? CellState.EntityIn : CellState.Walkable;
                this._entityIn = value;
            }
        }
        public Interactable AttachedInteract;
        public bool IsPlacementCell = false;

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
            GridManager.Instance.ChangeBitmapCell(this.GetGlobalPosition(), RefGrid.GridHeight,newState);
            this.Datas.state = newState;
        }

        public void RefreshCell()
        {
            this.ChangeCellState(this.Datas.state, true);
        }

        public void DropDownItem(InventoryItem item)
        {
            ItemContained = new();
            ItemContained.Init(item.ItemPreset, item.Slot, item.Quantity);
            ItemContainedObject = Instantiate(ItemContained.ItemPreset.DroppedItemPrefab, this.transform.position, quaternion.identity);
            //Mettre un animator sur le prefab pour le faire tourner ou jsp
//#if UNITY_EDITOR
//            EditorGUIUtility.PingObject(this);
//            Selection.activeObject = this;
//#endif
        }
        public bool HasItem(out InventoryItem item)
        {
            if (ItemContained != null && ItemContained.ItemPreset != null)
            {
                item = ItemContained;
                return true;

            }
            item = null;
            return false;
        }
        public void TryPickUpItem(PlayerBehavior player)
        {
            if(ItemContained != null)
            {
                int qtyRemainingInItem = ItemContained.Quantity;
                if (ItemContained.ItemPreset is ToolItem toolItem)
                {
                    qtyRemainingInItem -= player.PlayerSpecialSlots.TryAddItem(ItemContained.ItemPreset, ItemContained.Quantity);
                    player.SetActiveTool(toolItem);
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
                /*/ Debug.Log($"Actual Quantity : {ItemContained.Quantity}, Quantity returned: {qtyRemainingInItem}");
                 EditorGUIUtility.PingObject(this);
                 Selection.activeObject = this;
                 //*/
#endif
            }
        }
    }

    [Serializable, DataContract]
    public class CellData
    {
        public CellData(int yPos, int xPos, CellState state)
        {
            this.heightPos = yPos;
            this.widthPos = xPos;
            this.state = state;
        }
        [ShowInInspectorAttribute, DataMember(Name = "hp")]
        public int heightPos { get; set; }
        [ShowInInspectorAttribute, DataMember(Name = "wp")]
        public int widthPos { get; set; }
        [ShowInInspectorAttribute, DataMember(Name = "s")]
        public CellState state { get; set; }
        [ShowInInspectorAttribute, DataMember(Name = "poc"), DefaultValue(null)]
        public PlaceableItem placeableOnCell { get; set; }
    }

    [System.Serializable]
    public enum CellState
    {
        [EnumMember(Value = "Walkable")]
        Walkable = 1 << 0,
        [EnumMember(Value = "Blocked")]
        Blocked = 1 << 1,
        [EnumMember(Value = "EntityIn")]
        EntityIn = 1 << 2,
        [EnumMember(Value = "Interactable")]
        Interactable = 1 << 3,

        NonWalkable = Blocked | EntityIn | Interactable
    }

}