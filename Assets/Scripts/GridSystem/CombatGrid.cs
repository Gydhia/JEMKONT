using Jemkont.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Jemkont.GridSystem
{
    public class CombatGrid : SerializedMonoBehaviour
    {
        public string UName;

        // Editor only cause it sucks
        public bool _inspInited = false;

        private float widthOffset => SettingsManager.Instance.GridsPreset.CellsSize / 2f;
        private float cellsWidth => SettingsManager.Instance.GridsPreset.CellsSize;

        public int GridHeight = 2;
        public int GridWidth = 2;

        public Vector3 TopLeftOffset;

        [HideInInspector]
        public Cell[,] Cells;

        private void Start()
        {
            if(this.Cells != null)
                foreach (Cell cell in Cells)
                    cell.gameObject.SetActive(true);
            else
            {
                if (GridManager.Instance.SavedGrids.TryGetValue(this.UName, out GridData grid))
                {
                    this.DestroyChildren();
                    this.Init(grid.GridHeight, grid.GridWidth);

                    foreach(CellData cell in grid.CellDatas)
                        this.Cells[cell.heightPos, cell.widthPos].Datas = cell;
                }
                else
                    Debug.LogError("Could find grid : " + this.UName + " in the loaded grids");
            }
        }

        [Button]
        public void SpawnPlayerAtGrid()
        {
            GridManager.Instance.SetupPlayer(this);
        }

        public void Init(int height, int width)
        {
            this._inspInited = true;

            this.GridHeight = height;
            this.GridWidth = width;

            this.GenerateGrid(height, width);
        }

        public void DestroyChildren()
        {
            foreach (Transform child in this.transform)
            {
#if UNITY_EDITOR
                GameObject.DestroyImmediate(child.gameObject);
#else
                GameObject.Destroy(child.gameObject);
#endif
            }
            this.Cells = null;
        }

        public void GenerateGrid(GridData gridData)
        {
            this.GenerateGrid(gridData.GridHeight, gridData.GridWidth);

            foreach(CellData data in gridData.CellDatas)
                this.Cells[data.heightPos, data.widthPos].ChangeCellState(data.state);
        }

        /// <summary>
        /// The [0, 0] value of an array is at the top left corner. We'll follow these rules while instantiating cells
        /// </summary>
        /// <param name="height">The height of the array ([height, x])</param>
        /// <param name="width">The width of the array ([x, width])</param>
        public void GenerateGrid(int height, int width)
        {
            this.GridHeight = height;
            this.GridWidth = width;

            this.Cells = new Cell[height, width];

            // Generate the grid with new cells
            for (int i = 0; i < this.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < this.Cells.GetLength(1); j++)
                {
                    this.CreateAddCell(i, j, new Vector3(j * cellsWidth + widthOffset, 0.1f, -i * cellsWidth - widthOffset));
                }
            }

            this.TopLeftOffset = this.transform.position;
        }

        public void CreateAddCell(int height, int width, Vector3 position)
        {
            Cell newCell = Instantiate(GridManager.Instance.CellPrefab, position, Quaternion.identity, this.gameObject.transform);

            newCell.Init(height, width, CellState.Walkable, this);

            this.Cells[height, width] = newCell;
        }

        public void ResizeGrid(Cell[,] newCells, bool hideCells = false)
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
                    for (int i = oldHeight; i < newHeight; i++)
                    {
                        for (int j = 0; j < oldWidth; j++)
                        {
                            this.CreateAddCell(i, j, new Vector3(j * cellsWidth + widthOffset, 0.1f, -i * cellsWidth));
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
                            DestroyImmediate(this.Cells[j, i].gameObject);
#else
                            Destroy(this.Cells[j, i].gameObject);
#endif
                        }
                    }
                    this.Cells = newCells;
                }
                else
                {
                    this.Cells = newCells;
                    for (int j = oldWidth; j < newWidth; j++)
                    {
                        for (int i = 0; i < oldHeight; i++)
                        {
                            this.CreateAddCell(i, j, new Vector3(j * cellsWidth + widthOffset, 0.1f, -i * cellsWidth));
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
    }

    [System.Serializable]
    public class GridData
    {
        public GridData() { }
        public GridData(int GridHeight, int GridWidth)
        {
            this.GridHeight = GridHeight;
            this.GridWidth = GridWidth;
            this.CellDatas = new List<CellData>();
        }
        public GridData(int GridHeight, int GridWidth, List<CellData> CellDatas)
        {
            this.GridHeight = GridHeight;
            this.GridWidth = GridWidth;
            this.CellDatas = CellDatas;
        }

        public int GridHeight { get; set; }
        public int GridWidth { get; set; }

        public List<CellData> CellDatas { get; set; }
    }
}
