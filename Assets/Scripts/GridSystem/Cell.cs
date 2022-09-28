using Jemkont.Managers;
using System.Collections;
using System.Collections.Generic;
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
        public int yPos;
        public int xPos;

        public bool Walkable = true;

        public int gCost;
        public int hCost;

        public int fCost { get { return gCost + hCost; } }

        public Cell parent;

        public GridPosition PositionInGrid => new GridPosition(this.yPos, this.xPos);
        public Vector3 WorldPosition => this.gameObject.transform.position;
        #endregion

        public void Init(int yPos, int xPos, bool walkable)
        {
            this.name = "Cell[" + yPos + ", " + xPos + "]";

            this.yPos = yPos;
            this.xPos = xPos;

            this.Walkable = walkable;

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

        public void ChangeStateColor(Color color)
        {
            this.LeftEdge.material.color = this.BottomEdge.material.color =
                this.RightEdge.material.color = this.TopEdge.material.color = color;
        }
    }
}