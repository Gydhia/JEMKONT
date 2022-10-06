using Jemkont.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

namespace Jemkont.GridSystem
{
    [System.Serializable]
    public class CombatGrid : MonoBehaviour
    {
        public string UName;

        // Editor only cause it sucks
        public bool _inspInited = false;
        public GameObject Plane;

        private float widthOffset => SettingsManager.Instance.GridsPreset.CellsSize / 2f;
        private float cellsWidth => SettingsManager.Instance.GridsPreset.CellsSize;

        public int GridHeight = 2;
        public int GridWidth = 2;

        public Vector3 TopLeftOffset;

        [SerializeField]
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
                    
                }
                else
                    Debug.LogError("Could find grid : " + this.UName + " in the loaded grids");
            }
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

        /// <summary>
        /// The [0, 0] value of an array is at the top left corner. We'll follow these rules while instantiating cells
        /// </summary>
        /// <param name="height">The height of the array ([height, x])</param>
        /// <param name="width">The width of the array ([x, width])</param>
        public void GenerateGrid(int height, int width, bool hideCells = false)
        {
            this.Cells = new Cell[height, width];

            // Generate the grid with new cells
            for (int i = 0; i < this.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < this.Cells.GetLength(1); j++)
                {
                    this.CreateAddCell(i, j, new Vector3(j * cellsWidth + widthOffset, 0.1f, -i * cellsWidth - widthOffset), hideCells);
                }
            }

            this.TopLeftOffset = this.transform.position;
        }

        public void CreateAddCell(int height, int width, Vector3 position, bool hideCell = false)
        {
            Cell newCell = Instantiate(GridManager.Instance.CellPrefab, position, Quaternion.identity, this.gameObject.transform);

            newCell.Init(height, width, CellState.Walkable);

            if (hideCell)
                newCell.gameObject.SetActive(false);

            this.Cells[height, width] = newCell;
        }

        public void ResizePlane()
        {
            if(this.Plane == null)
                this.Plane = Instantiate(
                    GridManager.Instance.Plane,
                    new Vector3((this.GridWidth * cellsWidth) / 2 ,0f,-(this.GridHeight * cellsWidth) / 2 + (cellsWidth / 2)),Quaternion.identity,this.transform
                );

            this.Plane.transform.localScale = new Vector3(this.GridWidth * (cellsWidth / 10f), 0f, this.GridHeight * (cellsWidth / 10f));
            this.Plane.transform.localPosition = new Vector3((this.GridWidth * cellsWidth) / 2, 0f, -(this.GridHeight * cellsWidth) / 2);
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
                            this.CreateAddCell(i, j, new Vector3(j * cellsWidth + widthOffset, 0.1f, -i * cellsWidth), true);
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
                            this.CreateAddCell(i, j, new Vector3(j * cellsWidth + widthOffset, 0.1f, -i * cellsWidth), true);
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

        public GridPosition GetGridIndexFromWorld(Vector3 worldPos) 
        {
            float cellSize = SettingsManager.Instance.GridsPreset.CellsSize;
            int height = (int)(Mathf.Abs(Mathf.Abs(worldPos.z) - Mathf.Abs(this.TopLeftOffset.z)) / cellSize);
            int width = (int)(Mathf.Abs(Mathf.Abs(worldPos.x) - Mathf.Abs(this.TopLeftOffset.x)) / cellSize);

            return new GridPosition(height, width);
        }

        private void OnDrawGizmos()
        {
            if (this.Cells == null)
                return;

            Color red = new Color(Color.red.r, Color.red.g, Color.red.b, 0.4f);
            Color white = new Color(Color.white.r, Color.white.g, Color.white.b, 0.4f);
            Color blue = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.4f);

            float cellsWidth = SettingsManager.Instance.GridsPreset.CellsSize;
            Vector3 cellBounds = new Vector3(cellsWidth - 1f, 2f, cellsWidth - 1f);

            float widthOffset = (cellsWidth / 2);

            for (int i = 0; i < this.GridHeight; i++)
            {
                for (int j = 0; j < this.GridWidth; j++)
                {
                    if (this.Cells[i, j] == null)
                        continue;

                    if (this.Cells[i, j].Datas.State == CellState.Walkable)
                        Gizmos.color = white;
                    else if (this.Cells[i, j].Datas.State == CellState.Blocked)
                        Gizmos.color = red;
                    else
                        Gizmos.color = blue;

                    Vector3 pos = new Vector3(j * cellsWidth + TopLeftOffset.x + (cellsWidth / 2), 0.1f, -i * cellsWidth + TopLeftOffset.z - (cellsWidth / 2));

                    Gizmos.DrawCube(pos, cellBounds);
                }
            }
        }
    }

    [System.Serializable]
    public class GridData
    {
        public int GridHeight { get; set; }
        public int GridWidth { get; set; }

        public List<CellData> CellDatas { get; set; }
    }
}
