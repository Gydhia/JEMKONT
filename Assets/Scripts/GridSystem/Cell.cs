using Jemkont.Managers;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Jemkont.GridSystem
{
    public class Cell : MonoBehaviour
    {
        #region Appearance
        public MeshRenderer TopEdge;
        public MeshRenderer BottomEdge;
        public MeshRenderer LeftEdge;
        public MeshRenderer RightEdge;
        #endregion

        #region Datas
        public CellData Datas;

        public int gCost;
        public int hCost;

        public int fCost { get { return gCost + hCost; } }

        public Cell parent;

        public GridPosition PositionInGrid => new GridPosition(this.Datas.yPos, this.Datas.xPos);
        public Vector3 WorldPosition => this.gameObject.transform.position;
        #endregion

        public void Init(int yPos, int xPos, CellState state)
        {
            this.name = "Cell[" + yPos + ", " + xPos + "]";

            this.Datas.yPos = yPos;
            this.Datas.xPos = xPos;

            this.Datas.State = state;

            float edgesOffset = SettingsManager.Instance.GridsPreset.CellsEdgeOffset;
            float cellsWidth = SettingsManager.Instance.GridsPreset.CellsSize;

            // Scale the edges according to preset's width
            this.TopEdge.gameObject.transform.localScale = this.BottomEdge.gameObject.transform.localScale = new Vector3(cellsWidth / 2f, 0.1f, 0.1f);
            this.LeftEdge.gameObject.transform.localScale = this.RightEdge.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, cellsWidth / 2f);

            // Move the edges to the right position
            this.TopEdge.gameObject.transform.localPosition = new Vector3(0f, 0f, cellsWidth / 2f - edgesOffset);
            this.BottomEdge.gameObject.transform.localPosition = new Vector3(0f, 0f, edgesOffset - cellsWidth / 2f);
            this.LeftEdge.gameObject.transform.localPosition = new Vector3(cellsWidth / 2f - edgesOffset, 0f, 0f);
            this.RightEdge.gameObject.transform.localPosition = new Vector3(edgesOffset - cellsWidth / 2f, 0f, 0f);

            this.ChangeStateColor(Color.grey);
        }

        public void ChangeCellState(CellState newState)
        {
            if (this.Datas.State == newState)
                return;
            this.Datas.State = newState;

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
            {
                this.LeftEdge.material.color = this.BottomEdge.material.color =
                this.RightEdge.material.color = this.TopEdge.material.color = color;
            }
            else
            {
                this.LeftEdge.sharedMaterial.color = this.BottomEdge.sharedMaterial.color =
                this.RightEdge.sharedMaterial.color = this.TopEdge.sharedMaterial.color = color;
            }
        }
    }

    [System.Serializable, DataContract]
    public class CellData
    {
        public int yPos { get; set; }
        public int xPos { get; set; }

        public CellState State { get; set; }
    }

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