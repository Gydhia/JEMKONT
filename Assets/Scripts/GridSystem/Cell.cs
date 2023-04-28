using DownBelow.Managers;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Sirenix.OdinInspector;
using DownBelow.Entity;
using System;

namespace DownBelow.GridSystem
{
    public class Cell : MonoBehaviour
    {

        public BoxCollider Collider;

        public WorldGrid RefGrid;

        #region Datas
        public CellData Datas;

        public int gCost;
        public int hCost;

        public int fCost { get { return gCost + hCost; } }

        public Cell parent;
        private CharacterEntity _entityIn;
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