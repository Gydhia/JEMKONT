using Jemkont.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.GridSystem
{
    public class CombatGrid : MonoBehaviour
    {
        private float widthOffset => SettingsManager.Instance.GridsPreset.CellsSize / 2f;
        private float cellsWidth => SettingsManager.Instance.GridsPreset.CellsSize;

        public int GridHeight = 2;
        public int GridWidth = 2;

        public Vector3 TopLeftOffset;

        public Cell[,] Cells;

        public void Init(int height, int width)
        {
            this.GridHeight = height;
            this.GridWidth = width;

            this.GenerateGrid(height, width);
        }

        /// <summary>
        /// The [0, 0] value of an array is at the top left corner. We'll follow these rules while instantiating cells
        /// </summary>
        /// <param name="height">The height of the array ([height, x])</param>
        /// <param name="width">The width of the array ([x, width])</param>
        public void GenerateGrid(int height, int width)
        {
            this.Cells = new Cell[height, width];

            float heightOffset = this.Cells.GetLength(0) * cellsWidth - (cellsWidth / 2);
            // Generate the grid with new cells
            for (int i = 0; i < this.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < this.Cells.GetLength(1); j++)
                {
                    this.CreateAddCell(i, j, new Vector3(j * cellsWidth + widthOffset, 0.1f, -i * cellsWidth + heightOffset));
                }
            }

            this.TopLeftOffset = this.Cells[0, 0].WorldPosition;
        }

        public void CreateAddCell(int height, int width, Vector3 position)
        {
            Cell newCell = Instantiate(GridManager.Instance.CellPrefab, position, Quaternion.identity, this.gameObject.transform);

            newCell.Init(height, width, true);

            this.Cells[height, width] = newCell;
        }

        public void ResizeGrid(Cell[,] newCells)
        {
            int oldHeight = this.Cells.GetLength(0);
            int oldWidth = this.Cells.GetLength(1);

            int newHeight = newCells.GetLength(0);
            int newWidth = newCells.GetLength(1);

            // Resize height
            if(newHeight != oldHeight)
            {
                if(newHeight < oldHeight)
                {
                    for (int i = newHeight; i < oldHeight; i++)
                    {
                        for (int j = 0; j < oldWidth; j++)
                        {
#if UNITY_EDITOR
                            DestroyImmediate(this.Cells[i, j].gameObject);
#else
                            Destroy(this.Cells[i, j].gameObject);
#endif
                        }
                    }
                    this.Cells = newCells;
                }
                else
                {
                    this.Cells = newCells;
                    float heightOffset = this.Cells.GetLength(0) * cellsWidth - (cellsWidth / 2);
                    for (int i = oldHeight; i < newHeight; i++)
                    {
                        for (int j = 0; j < oldWidth; j++)
                        {
                            this.CreateAddCell(i, j, new Vector3(j * cellsWidth + widthOffset, 0.1f, -i * cellsWidth + heightOffset));
                        }
                    }
                }
            }

           // Resize the width
            else if(newWidth != oldWidth)
            {
                if (newWidth < oldWidth)
                {
                    for (int i = newWidth; i < oldWidth; i++)
                    {
                        for (int j = 0; j < oldHeight; j++)
                        {
#if UNITY_EDITOR
                            DestroyImmediate(this.Cells[i, j].gameObject);
#else
                            Destroy(this.Cells[i, j].gameObject);
#endif
                        }
                    }
                    this.Cells = newCells;
                }
                else
                {
                    float heightOffset = this.Cells.GetLength(0) * cellsWidth - (cellsWidth / 2);
                    this.Cells = newCells;
                    for (int j = oldWidth; j < newWidth; j++)
                    {
                        for (int i = 0; i < oldHeight; i++)
                        {
                            this.CreateAddCell(i, j, new Vector3(j * cellsWidth + widthOffset, 0.1f, i * cellsWidth + heightOffset));
                        }
                    }
                }
            }
        }

        public void ClearCells()
        {
            if (this.Cells != null)
            {
                for (int i = 0; i < this.Cells.GetLength(0); i++)
                    for (int j = 0; j < Cells.GetLength(1); j++)
                        Destroy(this.Cells[i, j].gameObject);

                this.Cells = null;
            }
        }

        private void OnDrawGizmos()
        {
            if (this.Cells == null || this.Cells.Length == 0)
                return;
         
            Gizmos.color = new Color(200f, 200f, 200f, 0.4f);

            float cellsWidth = SettingsManager.Instance.GridsPreset.CellsSize;
            Vector3 cellBounds = new Vector3(cellsWidth - 1f ,2f, cellsWidth - 1f);

            float heightOffset = this.Cells.GetLength(0) * cellsWidth - (cellsWidth / 2);
            float widthOffset = (cellsWidth / 2);
            
            for (int i = 0; i < this.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < this.Cells.GetLength(1); j++)
                {
                    Vector3 pos = new Vector3(j * cellsWidth + widthOffset, 0.1f, -i * cellsWidth + heightOffset);

                    Gizmos.DrawCube(pos, cellBounds);
                }
            }
        }
    }

}
