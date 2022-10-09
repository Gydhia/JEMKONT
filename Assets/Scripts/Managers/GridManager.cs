using Jemkont.GridSystem;
using Jemkont.Player;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace Jemkont.Managers
{
    public class GridManager : _baseManager<GridManager>
    {
        public bool InCombat = false;

        public Dictionary<string, GridData> SavedGrids;

        public List<Cell> Path;
        private List<Cell> _possiblePath = new List<Cell>();

        #region Assets_reference
        public Cell CellPrefab;
        public CombatGrid GridPrefab;
        public GameObject ObjectsHandler;
        public GameObject Plane;
        public PlayerBehavior PlayerPrefab;
        #endregion

        public Dictionary<string, CombatGrid> GameGrids;
        private CombatGrid _currentGrid;
        public GameObject TestPlane;

        public CharacterEntity CurrentPlayingEntity;
        public PlayerBehavior Player;
        public Cell LastHoveredCell;

        private void Awake()
        {
            base.Awake();
            this.LoadGridsFromJSON();
        }

        public void GenerateGrid(Vector3 offset, GridData gridData)
        {
            CombatGrid newGrid = Instantiate<CombatGrid>(this.GridPrefab, offset, Quaternion.identity, this.ObjectsHandler.transform);

            newGrid.GenerateGrid(gridData);

            this._currentGrid = newGrid;
            this.SetupPlayer(this._currentGrid);
        }

        public void SetupPlayer(CombatGrid grid)
        {
            this.Player = Instantiate(this.PlayerPrefab, Vector3.zero, Quaternion.identity, this.transform);
            this.Player.Init(grid.Cells[0, 0].PositionInGrid, grid.Cells[0, 0].WorldPosition);

            this.CurrentPlayingEntity = this.Player;
            this.Player.Init(SettingsManager.Instance.PlayerStats);
            this.ShowPossibleMovements();
        }

        private void Update()
        {
            if (this.CurrentPlayingEntity == null)
                return;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // layer 7 = Cell
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 7))
            {
                if (hit.collider != null && hit.collider.TryGetComponent(out Cell cell))
                {
                    // Avoid executing this code when it has already been done
                    if (cell != this.LastHoveredCell)
                    {
                        if(this.LastHoveredCell != null && this.LastHoveredCell.Datas.state == CellState.Walkable)
                            this.LastHoveredCell.ChangeStateColor(Color.grey);
                        this.ShowPossibleMovements();

                        this.LastHoveredCell = cell;
                        if (CardDraggingSystem.instance.DraggedCard != null && this.LastHoveredCell.Datas.state == CellState.Walkable)
                            this.LastHoveredCell.ChangeStateColor(Color.cyan);

                        // Make sure that we're not using a card so we don't show the player's path
                        if (CardDraggingSystem.instance.DraggedCard == null)
                        {
                            this.FindPath(cell.PositionInGrid, cell.RefGrid);

                            if (this.Path.Count <= this.CurrentPlayingEntity.Movement)
                            {
                                for (int i = 0; i < this.Path.Count; i++)
                                {
                                    if (this.Path[i] != null)
                                        this.Path[i].ChangeStateColor(Color.green);
                                }
                            }
                        }
                    }

                    
                }
            }

            // Teleport player to location
            if (Input.GetMouseButtonUp(0))
            {
                if (CardDraggingSystem.instance.DraggedCard == null && this._possiblePath.Contains(this.LastHoveredCell))
                {
                    if (this.LastHoveredCell != null && this.LastHoveredCell.Datas.state == CellState.Walkable)
                    {
                        this.CurrentPlayingEntity.GoTo(this.LastHoveredCell);
                        this.ShowPossibleMovements();
                    }
                }
            }

            // To mark a cell as non-walkable
            if (Input.GetMouseButtonUp(1))
            {
                // layer 7 = Cell
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 7))
                {
                    if(hit.collider.TryGetComponent(out Cell cell))
                    {
                        cell.ChangeCellState(cell.Datas.state == CellState.Blocked ? CellState.Walkable : CellState.Blocked);
                    }
                }
            }
        }

        public void ShowPossibleMovements()
        {
            int movePoints = this.CurrentPlayingEntity.Movement;
            Cell entityCell = this._currentGrid.Cells[this.CurrentPlayingEntity.PlayerPosition.longitude, this.CurrentPlayingEntity.PlayerPosition.latitude];

            // Clear the highlighted cells
            foreach (Cell cell in this._possiblePath)
                cell.ChangeStateColor(Color.grey);
            this._possiblePath.Clear();

            for (int x = -movePoints; x <= movePoints; x++)
            {
                for (int y = -movePoints; y <= movePoints; y++)
                {
                    if (x == 0 && y == 0 || (Mathf.Abs(x) + Mathf.Abs(y) > movePoints))
                        continue;

                    int checkX = entityCell.Datas.widthPos + x;
                    int checkY = entityCell.Datas.heightPos + y;

                    if (checkX >= 0 && checkX < this._currentGrid.GridWidth && checkY >= 0 && checkY < _currentGrid.GridHeight)
                    {
                        this.FindPath(this._currentGrid.Cells[checkY, checkX].PositionInGrid, this._currentGrid);

                        if(this.Path.Contains(this._currentGrid.Cells[checkY, checkX]) && this.Path.Count <= movePoints && (this._currentGrid.Cells[checkY, checkX].Datas.state == CellState.Walkable))
                        {
                            this._possiblePath.Add(this._currentGrid.Cells[checkY, checkX]);
                            this._currentGrid.Cells[checkY, checkX].ChangeStateColor(new Color(0.82f, 0.796f, 0.5f, 0.8f));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// While calculate the closest path to a target, storing it in the Path var of the GridManager
        /// </summary>
        /// <param name="target"></param>
        public void FindPath(GridPosition target, CombatGrid grid)
        {
            if (Player == null)
                return;

            Cell startCell = grid.Cells[Player.PlayerPosition.longitude, Player.PlayerPosition.latitude];
            Cell targetCell = grid.Cells[target.longitude, target.latitude];

            List<Cell> openSet = new List<Cell>();
            HashSet<Cell> closedSet = new HashSet<Cell>();

            openSet.Add(startCell);

            while (openSet.Count > 0)
            {
                Cell currentCell = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentCell.fCost || openSet[i].fCost == currentCell.fCost && openSet[i].hCost < currentCell.hCost)
                    {
                        currentCell = openSet[i];
                    }
                }

                openSet.Remove(currentCell);
                closedSet.Add(currentCell);

                if (currentCell == targetCell)
                {
                    this.RetracePath(startCell, targetCell);
                    return;
                }

                List<Cell> actNeighbours = grid.IsCombatGrid ? GetCombatNeighbours(currentCell, grid) : GetNormalNeighbours(currentCell, grid);
                foreach (Cell neighbour in actNeighbours)
                {
                    if (neighbour.Datas.state != CellState.Walkable || closedSet.Contains(neighbour))
                        continue;

                    int newMovementCostToNeightbour = currentCell.gCost + GetDistance(currentCell, neighbour);
                    if (newMovementCostToNeightbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeightbour;
                        neighbour.hCost = GetDistance(neighbour, targetCell);
                        neighbour.parent = currentCell;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }
        }

        public void RetracePath(Cell startCell, Cell endCell)
        {
            List<Cell> path = new List<Cell>();
            Cell currentCell = endCell;

            while (currentCell != startCell)
            {
                path.Add(currentCell);
                currentCell = currentCell.parent;
            }
            path.Reverse();

            this.Path = path;
        }
        /// <summary>
        /// To get the 8 neighbours around a cell. Used for out of combat walk.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public List<Cell> GetNormalNeighbours(Cell cell, CombatGrid grid)
        {
            List<Cell> neighbours = new List<Cell>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = cell.Datas.widthPos + x;
                    int checkY = cell.Datas.heightPos + y;

                    if (checkX >= 0 && checkX < grid.GridWidth && checkY >= 0 && checkY < grid.GridHeight)
                    {
                        neighbours.Add(grid.Cells[checkY, checkX]);
                    }
                }
            }

            return neighbours;
        }

        /// <summary>
        /// To get the 4 lateral neighbours of a cell. Used for in combat walk
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public List<Cell> GetCombatNeighbours(Cell cell, CombatGrid grid)
        {
            List<Cell> neighbours = new List<Cell>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if ((x == 0 && y == 0) || (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1))
                        continue;

                    int checkX = cell.Datas.widthPos + x;
                    int checkY = cell.Datas.heightPos + y;

                    if (checkX >= 0 && checkX < grid.GridWidth && checkY >= 0 && checkY < grid.GridHeight)
                    {
                        neighbours.Add(grid.Cells[checkY, checkX]);
                    }
                }
            }

            return neighbours;
        }

        /// <summary>
        /// Return the weighted distance from 2 cells. 
        /// </summary>
        /// <param name="cellA"></param>
        /// <param name="cellB"></param>
        /// <returns></returns>
        public int GetDistance(Cell cellA, Cell cellB)
        {
            int dstX = Mathf.Abs(cellA.Datas.widthPos - cellB.Datas.widthPos);
            int dstY = Mathf.Abs(cellA.Datas.heightPos - cellB.Datas.heightPos);

            // 14 is the diagonal weight, used in out of combat walk.
            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }

        #region JSON_SAVES
        public void LoadGridsFromJSON()
        {
            this.SavedGrids = new Dictionary<string, GridData>();

            TextAsset[] jsons = Resources.LoadAll<TextAsset>("Saves/Grids");
            foreach (TextAsset json in jsons)
            {
                GridData loadedData = JsonConvert.DeserializeObject<GridData>(json.text);

                this.SavedGrids.Add(json.name, loadedData);
            }
        }

        public void SaveGridAsJSON(CombatGrid grid)
        {
            if (grid.UName == "" && grid.UName == string.Empty)
                return;

            List<CellData> savedCells = new List<CellData>();

            // Get the non walkable cells only
            for (int i = 0; i < grid.Cells.GetLength(0); i++)
                for (int j = 0; j < grid.Cells.GetLength(1); j++)
                    if (grid.Cells[i, j].Datas.state != CellState.Walkable)
                        savedCells.Add(grid.Cells[i, j].Datas);

            GridData gridData = new GridData();
            gridData.GridHeight = grid.GridHeight;
            gridData.GridWidth = grid.GridWidth;
            gridData.CellDatas = savedCells;

            string gridJson = JsonConvert.SerializeObject(gridData);
            this._saveJSONFile(gridJson, grid.UName);
        }
        public void SaveGridAsJSON(CellData[,] cellDatas, string name)
        {
            if (name == "" && name == string.Empty)
                return;

            List<CellData> savedCells = new List<CellData>();
            for (int i = 0; i < cellDatas.GetLength(0); i++)
                for (int j = 0; j < cellDatas.GetLength(1); j++)
                    if (cellDatas[i, j].state != CellState.Walkable)
                        savedCells.Add(cellDatas[i, j]);

            GridData gridData = new GridData();
            gridData.GridHeight = cellDatas.GetLength(0);
            gridData.GridWidth = cellDatas.GetLength(1);
            gridData.CellDatas = savedCells;

            string gridJson = JsonConvert.SerializeObject(gridData);
            this._saveJSONFile(gridJson, name);
        }
        public void SaveGridAsJSON(GridData grid, string uName)
        {
            if (uName == "" && uName == string.Empty)
                return;

            string gridJson = JsonConvert.SerializeObject(grid);
            this._saveJSONFile(gridJson, uName);
        }

        private void _saveJSONFile(string json, string pathName)
        {
            string path = Application.dataPath + "/Resources/Saves/Grids/" + pathName + ".json";
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllText(path, json);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif

            this.LoadGridsFromJSON();
        }
        #endregion
    }
    public struct GridPosition
    {
        public GridPosition(int longitude, int latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }

        public static readonly GridPosition zero = new GridPosition(0, 0);

        public int longitude { get; private set; }
        public int latitude { get; private set; }
    }
}
