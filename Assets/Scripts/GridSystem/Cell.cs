using Jemkont.Managers;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Jemkont.GridSystem
{
    public class Cell : MonoBehaviour
    {
        #region Appearance
        public MeshRenderer SelfPlane;
        #endregion

        public BoxCollider Collider;

        public CombatGrid RefGrid;

        #region Datas
        public CellData Datas;

        public int gCost;
        public int hCost;

        public int fCost { get { return gCost + hCost; } }

        public Cell parent;

        public GridPosition PositionInGrid => new GridPosition(this.Datas.heightPos, this.Datas.widthPos);
        public Vector3 WorldPosition => this.gameObject.transform.position;
        #endregion

        public void Init(int yPos, int xPos, CellState state, CombatGrid refGrid)
        {
            this.RefGrid = refGrid;

            this.name = "Cell[" + yPos + ", " + xPos + "]";

            this.Datas.heightPos = yPos;
            this.Datas.widthPos = xPos;

            this.Datas.state = state;

            float edgesOffset = SettingsManager.Instance.GridsPreset.CellsEdgeOffset;
            float cellsWidth = SettingsManager.Instance.GridsPreset.CellsSize;

            // Scale the plane according to preset's width
            this.SelfPlane.gameObject.transform.localScale = new Vector3(cellsWidth / 10f - 0.1f, 0.1f, cellsWidth / 10f - 0.1f);

            this.Collider.size = new Vector3(cellsWidth - 0.01f, 1.5f, cellsWidth - 0.01f);

            this.ChangeStateColor(Color.grey);
        }

        public void ChangeCellState(CellState newState)
        {
            if (this.Datas.state == newState)
                return;
            this.Datas.state = newState;

            Color stateColor;
            switch (newState)
            {
                case CellState.Blocked: stateColor = Color.red; break;
                case CellState.EntityIn: stateColor = Color.blue; break;

                case CellState.Walkable:
                default: stateColor = Color.white; break;
            }

            this.ChangeStateColor(stateColor);
        }

        public void ChangeStateColor(Color color)
        {
            if (Application.isPlaying)
                this.SelfPlane.material.color = color;
            else
                this.SelfPlane.sharedMaterial.color = color;
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

        public int heightPos { get; set; }
        public int widthPos { get; set; }

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
        EntityIn = 2
    }

}