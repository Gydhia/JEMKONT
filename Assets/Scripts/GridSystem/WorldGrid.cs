using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using DownBelow.Entity;

namespace DownBelow.GridSystem
{
    public class WorldGrid : SerializedMonoBehaviour
    {
        public string UName;

        public WorldLevel Level;

        private float widthOffset => SettingsManager.Instance.GridsPreset.CellsSize / 2f;
        private float cellsWidth => SettingsManager.Instance.GridsPreset.CellsSize;

        public bool IsCombatGrid;
        public int GridHeight;
        public int GridWidth;

        public Vector3 TopLeftOffset;

        [HideInInspector]
        public Cell[,] Cells;
        public GridData SelfData;

        public List<CharacterEntity> GridEntities;

        public Dictionary<string, CombatGrid> InnerCombatGrids = new Dictionary<string, CombatGrid>();

        public virtual void Init(GridData data)
        {
            this.GridHeight = data.GridHeight;
            this.GridWidth = data.GridWidth;
            this.IsCombatGrid = data.IsCombatGrid;

            if (!data.IsCombatGrid)
            {
                var prefabToLoad = AssetDatabase.LoadAssetAtPath<GameObject>(data.GridLevelPath);
                var loaded = Instantiate(prefabToLoad, this.transform);

                if (!loaded.TryGetComponent(out Level))
                {
                    Debug.LogError("LEVEL ERROR : YOU FORGOT TO PUT THE SCRIPT 'World Level' AT THE ROOT OF YOUR LEVEL'S PREFAB");
                }
                else
                {
                    this.Level.transform.position = -data.TopLeftOffset;
                }

                this.name = "WG_" + loaded.name;
            }

            this.SelfData = data;

            this.GenerateGrid(data);
            if(data.InnerGrids != null)
                this.GenerateInnerGrids(data.InnerGrids, this.TopLeftOffset);
            //this.RedrawGrid();

            GameManager.Instance.OnEnteredGrid += _entityEnteredGrid;
            GameManager.Instance.OnExitingGrid += _entityExitingGrid;
        }

        /// <summary>
        /// To notify if an entity entered a grid. /!\ Used only for players right now
        /// </summary>
        /// <param name="Data"></param>
        protected void _entityEnteredGrid(Events.EntityEventData Data)
        {
            if (Data.Entity.CurrentGrid == this && !this.GridEntities.Contains(Data.Entity))
                this.GridEntities.Add(Data.Entity);
        }

        /// <summary>
        /// To notify if an entity exited a grid. /!\ Used only for players right now
        /// </summary>
        /// <param name="Data"></param>
        protected void _entityExitingGrid(Events.EntityEventData Data)
        {
            this.GridEntities.Remove(Data.Entity);
        }

        public void DestroyChildren()
        {
            foreach (Transform child in this.transform)
                GameObject.Destroy(child.gameObject);
            
            this.Cells = null;
        }

        public void GenerateGrid(GridData gridData)
        {
            this.GenerateGrid(gridData.GridHeight, gridData.GridWidth, gridData.Longitude, gridData.Latitude);

            foreach(CellData data in gridData.CellDatas)
                this.Cells[data.heightPos, data.widthPos].Datas.state = data.state;

            // Check in the inner grids, mark the shared cells as so and disable them.
            if(gridData.InnerGrids != null)
            {
                foreach (GridData innerGrid in gridData.InnerGrids)
                    for (int i = innerGrid.Longitude; i < innerGrid.Longitude + innerGrid.GridWidth; i++)
                        for (int j = innerGrid.Latitude; j < innerGrid.Latitude + innerGrid.GridHeight; j++)
                        {
                            // We'll never be in combat for now, so just destroy these cells
                            Destroy(this.Cells[j, i].gameObject);
                            this.Cells[j, i] = null;
                        }
            }

            this.GridEntities = new List<CharacterEntity>();
            if(gridData.SpawnablePresets != null)
            {
                // Used to generate UID
                foreach (var spawnable in gridData.SpawnablePresets)
                    if (spawnable.Value != null)
                        if (SettingsManager.Instance.SpawnablesPresets.TryGetValue(spawnable.Value, out BaseSpawnablePreset spawnSO))
                            // TODO: Differentiate enemies and NPC. For now they'll be enemies
                            spawnSO.Init(this.Cells[spawnable.Key.latitude, spawnable.Key.longitude]);
            }
        }

        public void GenerateInnerGrids(List<GridData> innerGrids, Vector3 parentTLOffset)
        {
            int count = 0;
            this.InnerCombatGrids = new Dictionary<string, CombatGrid>();
            foreach (GridData innerGrid in innerGrids)
            {
                CombatGrid newInnerGrid = Instantiate(GridManager.Instance.CombatGridPrefab, Vector3.zero, Quaternion.identity, this.transform) as CombatGrid;

                newInnerGrid.Init(innerGrid);
                newInnerGrid.ParentGrid = this;
                newInnerGrid.Longitude = innerGrid.Longitude;
                newInnerGrid.Latitude = innerGrid.Latitude;
                newInnerGrid.UName = this.UName + count; 

                this.InnerCombatGrids.Add(newInnerGrid.UName, newInnerGrid);
                count++;
            }
        }

        /// <summary>
        /// The [0, 0] value of an array is at the top left corner. We'll follow these rules while instantiating cells
        /// </summary>
        /// <param name="height">The height of the array ([height, x])</param>
        /// <param name="width">The width of the array ([x, width])</param>
        public void GenerateGrid(int height, int width, int longitude, int latitude)
        {
            this.GridHeight = height;
            this.GridWidth = width;

            this.Cells = new Cell[height, width];
            this.TopLeftOffset = this.IsCombatGrid ? this.transform.parent.transform.position : this.transform.position;

            // Generate the grid with new cells
            for (int i = 0; i < this.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < this.Cells.GetLength(1); j++)
                {
                    this.CreateAddCell(i, j, new Vector3((j + longitude) * cellsWidth + widthOffset, 0f, -(i + latitude) * cellsWidth - widthOffset));
                }
            }
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
                    if(this.Cells[i, j] != null)
                        this.Cells[i, j].RefreshCell();
        }

        #region Utility_methods
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
                            this.CreateAddCell(i, j, new Vector3(j * cellsWidth + widthOffset, 0f, -i * cellsWidth));
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
                            this.CreateAddCell(i, j, new Vector3(j * cellsWidth + widthOffset, 0f, -i * cellsWidth));
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
        #endregion
    }

    [System.Serializable, DataContract]
    public class GridData
    {
        public GridData() { }
        public GridData(string GridName, bool IsCombatGrid, int GridHeight, int GridWidth)
        {
            this.GridName = GridName;
            this.IsCombatGrid = IsCombatGrid;
            this.GridHeight = GridHeight;
            this.GridWidth = GridWidth;
            this.CellDatas = new List<CellData>();
        }
        public GridData(string GridName, bool IsCombatGrid, int GridHeight, int GridWidth, List<CellData> CellDatas)
        {
            this.GridName = GridName;
            this.IsCombatGrid = IsCombatGrid;
            this.GridHeight = GridHeight;
            this.GridWidth = GridWidth;
            this.CellDatas = CellDatas;
        }

        /// <summary>
        /// /!\ Constructor made for the InnerGrids (aka CombatGrids), don't use it for WorldGrids
        /// </summary>
        public GridData(string GridName, bool IsCombatGrid, int GridHeight, int GridWidth, int Longitude, int Latitude, List<CellData> CellDatas, Dictionary<GridPosition, Guid> EntitiesSpawns)
        {
            this.GridName = GridName;
            this.IsCombatGrid = IsCombatGrid;
            this.GridHeight = GridHeight;
            this.GridWidth = GridWidth;
            this.Longitude = Longitude;
            this.Latitude = Latitude;
            this.CellDatas = CellDatas;
            this.SpawnablePresets = EntitiesSpawns;
        }

        /// <summary>
        /// /!\ Constructor made for the WorldGrids
        /// </summary>
        public GridData(string GridName, string GridLevelPath, bool IsCombatGrid, int GridHeight, int GridWidth, Vector3 TopLeftOffset, List<CellData> CellDatas, List<GridData> InnerGridsData, Dictionary<GridPosition, Guid> Spawnables)
        {
            this.GridName = GridName;
            this.GridLevelPath = GridLevelPath;
            this.IsCombatGrid = IsCombatGrid;
            this.GridHeight = GridHeight;
            this.GridWidth = GridWidth;
            this.TopLeftOffset = TopLeftOffset;
            this.CellDatas = CellDatas;
            this.InnerGrids = InnerGridsData;
            this.SpawnablePresets = Spawnables;
        }
        [DataMember]
        public string GridName { get; set; }
        [DataMember]
        public string GridLevelPath { get; set; }

        [DataMember]
        public bool IsCombatGrid { get; set; }
        [DataMember]
        public int Longitude { get; set; }
        [DataMember]
        public int Latitude { get; set; }
        [DataMember]
        public int GridHeight { get; set; }
        [DataMember]
        public int GridWidth { get; set; }
        [DataMember]
        public Vector3 TopLeftOffset { get; set; }

        [DataMember]
        public List<GridData> InnerGrids;
        [DataMember]
        public List<CellData> CellDatas { get; set; }

        [DataMember]
        [Newtonsoft.Json.JsonConverter(typeof(JSONGridConverter))]
        public Dictionary<GridPosition, Guid> SpawnablePresets { get; set; }
    }
}
