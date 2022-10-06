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

        #region Assets_reference
        public Cell CellPrefab;
        public CombatGrid GridPrefab;
        public GameObject ObjectsHandler;
        public GameObject Plane;
        public PlayerBehavior PlayerPrefab;
        #endregion

        public CombatGrid MainGrid;
        public GameObject TestPlane;

        public PlayerBehavior Player;

        private void Awake()
        {
            this.LoadGridsFromJSON();
        }

        private void Start()
        {
            //this._generateGrid(10, 15);
        }

        [Button]
        private void _generateGrid(int height, int width)
        {
            if (this.MainGrid != null)
                Destroy(this.MainGrid.gameObject);
            if (this.TestPlane != null)
                Destroy(this.TestPlane);
            if (this.Player != null)
                Destroy(this.Player.gameObject);
            this.MainGrid = Instantiate<CombatGrid>(this.GridPrefab, this.ObjectsHandler.transform);
            this.MainGrid.Init(height, width);

            float cellsWidth = SettingsManager.Instance.GridsPreset.CellsSize;

            // Place the test plane
            this.TestPlane = Instantiate(
                this.Plane,
                new Vector3(
                    (width * cellsWidth) / 2,
                    0f,
                    (height * cellsWidth) / 2
                ),
                Quaternion.identity,
                this.ObjectsHandler.transform
            );
            this.TestPlane.transform.localScale = new Vector3(width * (cellsWidth / 10f), 0f, height * (cellsWidth / 10f));

            this.Player = Instantiate(this.PlayerPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity, this.transform);
            this.Player.Init(this.MainGrid.Cells[0, 0].PositionInGrid, this.MainGrid.Cells[0, 0].WorldPosition);
        }

        private void Update()
        {
            // We just raycast the cell and then ask to find a path to it from our player
            if (Input.GetMouseButtonUp(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // layer 6 = ground
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 6))
                {
                    for (int i = 0; i < this.Path.Count; i++)
                    {
                        if (this.Path[i] != null)
                            this.Path[i].ChangeStateColor(Color.grey);
                    }
                    float cellSize = SettingsManager.Instance.GridsPreset.CellsSize;
                    GridPosition clickPosition = new GridPosition(Mathf.Abs((int)((hit.point.x - this.MainGrid.TopLeftOffset.x) / cellSize)), Mathf.Abs((int)((hit.point.z - this.MainGrid.TopLeftOffset.z) / cellSize)));
                    Debug.Log("x:" + clickPosition.x + " y:" + clickPosition.y);
                    this.FindPath(clickPosition);

                    for (int i = 0; i < this.Path.Count; i++)
                    {
                        if (this.Path[i] != null)
                            this.Path[i].ChangeStateColor(Color.green);
                    }
                }
            }


            // To mark a cell as non-walkable
            if (Input.GetMouseButtonUp(1))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // layer 6 = ground
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 6))
                {
                    float cellSize = SettingsManager.Instance.GridsPreset.CellsSize;
                    GridPosition clickPosition = new GridPosition(Mathf.Abs((int)((hit.point.x - this.MainGrid.TopLeftOffset.x) / cellSize)), Mathf.Abs((int)((hit.point.z - this.MainGrid.TopLeftOffset.z) / cellSize)));
                    Debug.Log("x:" + clickPosition.x + " y:" + clickPosition.y);
                    this.MainGrid.Cells[clickPosition.y, clickPosition.x].ChangeCellState(CellState.Blocked);
                }
            }
        }

        /// <summary>
        /// While calculate the closest path to a target, storing it in the Path var of the GridManager
        /// </summary>
        /// <param name="target"></param>
        public void FindPath(GridPosition target)
        {
            Cell startCell = this.MainGrid.Cells[Player.PlayerPosition.y, Player.PlayerPosition.x];
            Cell targetCell = this.MainGrid.Cells[target.y, target.x];

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

                List<Cell> actNeighbours = this.InCombat ? GetCombatNeighbours(currentCell) : GetNormalNeighbours(currentCell);
                foreach (Cell neighbour in actNeighbours)
                {
                    if (neighbour.Datas.State != CellState.Walkable || closedSet.Contains(neighbour))
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
        public List<Cell> GetNormalNeighbours(Cell cell)
        {
            List<Cell> neighbours = new List<Cell>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = cell.Datas.xPos + x;
                    int checkY = cell.Datas.yPos + y;

                    if (checkX >= 0 && checkX < this.MainGrid.GridWidth && checkY >= 0 && checkY < this.MainGrid.GridHeight)
                    {
                        neighbours.Add(this.MainGrid.Cells[checkY, checkX]);
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
        public List<Cell> GetCombatNeighbours(Cell cell)
        {
            List<Cell> neighbours = new List<Cell>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if ((x == 0 && y == 0) || (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1))
                        continue;

                    int checkX = cell.Datas.xPos + x;
                    int checkY = cell.Datas.yPos + y;

                    if (checkX >= 0 && checkX < this.MainGrid.GridWidth && checkY >= 0 && checkY < this.MainGrid.GridHeight)
                    {
                        neighbours.Add(this.MainGrid.Cells[checkY, checkX]);
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
            int dstX = Mathf.Abs(cellA.Datas.xPos - cellB.Datas.xPos);
            int dstY = Mathf.Abs(cellA.Datas.yPos - cellB.Datas.yPos);

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
                GridData loadedData = JsonConvert.DeserializeObject<GridData>(jsons[0].text);

                this.SavedGrids.Add(json.name, loadedData);
            }
        }

        public void SaveGridAsJSON(CombatGrid grid)
        {
            List<CellData> savedCells = new List<CellData>();

            // Get the non walkable cells only
            for (int i = 0; i < grid.Cells.GetLength(0); i++)
                for (int j = 0; j < grid.Cells.GetLength(1); j++)
                    if (grid.Cells[i, j].Datas.State != CellState.Walkable)
                        savedCells.Add(grid.Cells[i, j].Datas);

            GridData gridData = new GridData();
            gridData.GridHeight = grid.GridHeight;
            gridData.GridWidth = grid.GridWidth;
            gridData.CellDatas = savedCells;

            string gridJson = JsonConvert.SerializeObject(gridData);

            File.WriteAllText(Application.dataPath + "/Resources/Saves/Grids/" + grid.UName + ".json", gridJson);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        #endregion
    }
    public struct GridPosition
    {
        public GridPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static readonly GridPosition zero = new GridPosition(0, 0);

        public int x { get; private set; }
        public int y { get; private set; }
    }
}
