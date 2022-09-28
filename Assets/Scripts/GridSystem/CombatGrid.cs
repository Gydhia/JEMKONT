using Jemkont.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.GridSystem
{
    public class CombatGrid : MonoBehaviour
    {
        public int GridHeight;
        public int GridWidth;

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

            float cellsWidth = SettingsManager.Instance.GridsPreset.CellsSize;

            float heightOffset = this.Cells.GetLength(0) * cellsWidth - (cellsWidth / 2);
            float widthOffset = (cellsWidth / 2);
            // Generate the grid with new cells
            for (int i = 0; i < this.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < this.Cells.GetLength(1); j++)
                {
                    Cell newCell = Instantiate(GridManager.Instance.CellPrefab, new Vector3(j * cellsWidth + widthOffset, 0.1f, -i * cellsWidth + heightOffset), Quaternion.identity, this.gameObject.transform);

                    newCell.Init(i, j, true);

                    this.Cells[i, j] = newCell;
                }
            }

            this.TopLeftOffset = this.Cells[0, 0].WorldPosition;
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
    }

}
