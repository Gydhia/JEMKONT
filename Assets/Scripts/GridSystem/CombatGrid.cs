using Jemkont.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Jemkont.Entity;

namespace Jemkont.GridSystem
{
    public class CombatGrid : SerializedMonoBehaviour
    {
        public string UName;

        private float widthOffset => SettingsManager.Instance.GridsPreset.CellsSize / 2f;
        private float cellsWidth => SettingsManager.Instance.GridsPreset.CellsSize;

        public bool IsCombatGrid;
        public int GridHeight;
        public int GridWidth;

        public Vector3 TopLeftOffset;

        [HideInInspector]
        public Cell[,] Cells;

        public List<CharacterEntity> GridEntities;

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
                    this.Init(grid);
                }
                else
                    Debug.LogError("Could find grid : " + this.UName + " in the loaded grids");
            }
        }

        public void Init(GridData data)
        {
            this.GridHeight = data.GridHeight;
            this.GridWidth = data.GridWidth;
            this.IsCombatGrid = data.IsCombatGrid;

            this.GenerateGrid(data);

            GridManager.Instance.SetupPlayer(this);
            this.RedrawGrid();
        }

        public void DestroyChildren()
        {
            foreach (Transform child in this.transform)
                GameObject.Destroy(child.gameObject);
            
            this.Cells = null;
        }

        public void GenerateGrid(GridData gridData)
        {
            this.GenerateGrid(gridData.GridHeight, gridData.GridWidth);

            foreach(CellData data in gridData.CellDatas)
                this.Cells[data.heightPos, data.widthPos].Datas.state = data.state;

            this.GridEntities = new List<CharacterEntity>();
            if(gridData.EntitiesSpawns != null)
            {
                foreach (var entity in gridData.EntitiesSpawns)
                {
                    if (entity.Value != null)
                    {
                        if (CombatManager.Instance.EntitiesSpawnsSO.TryGetValue(entity.Value, out EntitySpawn entitySO))
                        {
                            this.Cells[entity.Key.longitude, entity.Key.latitude].Datas.state = CellState.EntityIn;
                            this.GridEntities.Add(Instantiate(entitySO.Entity, this.Cells[entity.Key.longitude, entity.Key.latitude].WorldPosition, Quaternion.identity, this.transform));
                        }
                    }
                }
            }
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

        [Button]
        private void RedrawGrid()
        {
            for (int i = 0; i < this.Cells.GetLength(0); i++)
                for (int j = 0; j < this.Cells.GetLength(1); j++)
                    this.Cells[i, j].RefreshCell();
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
                        for (int j = 0; j < oldWidth; j++)
                            Destroy(this.Cells[i, j].gameObject);
                    
                    this.Cells = newCells;
                }
                else
                {
                    this.Cells = newCells;
                    for (int i = oldHeight; i < newHeight; i++)
                        for (int j = 0; j < oldWidth; j++)
                            this.CreateAddCell(i, j, new Vector3(j * cellsWidth + widthOffset, 0.1f, -i * cellsWidth));
                }
            }
            // Resize the width
            else if(newWidth != oldWidth)
            {
                if (newWidth < oldWidth)
                {
                    for (int i = newWidth; i < oldWidth; i++)
                        for (int j = 0; j < oldHeight; j++)
                            Destroy(this.Cells[j, i].gameObject);
                        
                    this.Cells = newCells;
                }
                else
                {
                    this.Cells = newCells;
                    for (int j = oldWidth; j < newWidth; j++)
                        for (int i = 0; i < oldHeight; i++)
                            this.CreateAddCell(i, j, new Vector3(j * cellsWidth + widthOffset, 0.1f, -i * cellsWidth));
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
        public GridData(bool IsCombatGrid, int GridHeight, int GridWidth)
        {
            this.IsCombatGrid = IsCombatGrid;
            this.GridHeight = GridHeight;
            this.GridWidth = GridWidth;
            this.CellDatas = new List<CellData>();
        }
        public GridData(bool IsCombatGrid, int GridHeight, int GridWidth, List<CellData> CellDatas)
        {
            this.IsCombatGrid = IsCombatGrid;
            this.GridHeight = GridHeight;
            this.GridWidth = GridWidth;
            this.CellDatas = CellDatas;
        }

        public GridData(bool IsCombatGrid, int GridHeight, int GridWidth, List<CellData> CellDatas, Dictionary<GridPosition, Guid> EntitiesSpawns)
        {
            this.IsCombatGrid = IsCombatGrid;
            this.GridHeight = GridHeight;
            this.GridWidth = GridWidth;
            this.CellDatas = CellDatas;
            this.EntitiesSpawns = EntitiesSpawns;
        }
        public bool IsCombatGrid { get; set; }

        public int GridHeight { get; set; }
        public int GridWidth { get; set; }

        public List<CellData> CellDatas { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(JSONGridConverter))]
        public Dictionary<GridPosition, Guid> EntitiesSpawns { get; set; }
    }
}
