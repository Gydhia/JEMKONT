using Jemkont.GridSystem;
using Jemkont.Entity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using System;
using Jemkont.Events;

namespace Jemkont.Managers {
    public class GridManager : _baseManager<GridManager> 
    {
        #region Assets_reference

        public Vector2 FirstSpawn;
        public Vector2 SecondSpawn;
        public Vector2 ThirdSpawn;
        public Vector2 FourthSpawn;

        public Cell CellPrefab;

        [SerializeField] public WorldGrid CombatGridPrefab;
        [SerializeField] private Transform _objectsHandler;
        [SerializeField] private Transform _gridsDataHandler;

        #endregion

        public bool InCombat = false;

        #region Datas
        public Dictionary<string, GridData> SavedGrids;

        public Dictionary<Guid, EntitySpawn> EnemiesSpawnSO;
        public Dictionary<Guid, EntitySpawn> NPCsSpawnsSO;
        #endregion


        public List<Cell> Path;
        private List<Cell> _possiblePath = new List<Cell>();

        public Dictionary<string, WorldGrid> WorldGrids;

        private CombatGrid _currentCombatGrid;
        public WorldGrid MainWorldGrid;
        public GameObject TestPlane;

        public Cell LastHoveredCell;

        public override void Awake()
        {
            base.Awake();

            this.WorldGrids = new Dictionary<string, WorldGrid>();
            // Load the Grids and Entities SO
            this.LoadGridsFromJSON();
            this.LoadEveryEntities();

            Destroy(this._gridsDataHandler.gameObject);

            foreach (var savedGrid in this.SavedGrids)
            {
                // Only load the saves that are indicated so
                if (savedGrid.Value.ToLoad)
                {
                    this.GenerateGrid(savedGrid.Value, savedGrid.Key);   
                }
            }

            // Events
            InputManager.Instance.OnCellClicked += this.ProcessCellClick;
        }

        public void GenerateGrid(GridData gridData, string gridId) 
        {
            WorldGrid newGrid = Instantiate(this.CombatGridPrefab, gridData.TopLeftOffset, Quaternion.identity, this._objectsHandler);
                
            newGrid.UName = gridId;
            newGrid.Init(gridData);

            this.WorldGrids.Add(newGrid.UName, newGrid);

            this.MainWorldGrid = this.WorldGrids.Values.First();
        }

        [Button]
        public void StartCombat()
        {
            CombatManager.Instance.CurrentPlayingGrid = this._currentCombatGrid;
            CombatManager.Instance.StartCombat();
        }

        public void OnNewCellHovered(CharacterEntity entity,Cell cell) 
        {
            if (this.LastHoveredCell != null && this.LastHoveredCell.Datas.state == CellState.Walkable)
                this.LastHoveredCell.ChangeStateColor(Color.grey);

            this.LastHoveredCell = cell;
            if (CardDraggingSystem.instance.DraggedCard != null && this.LastHoveredCell.Datas.state == CellState.Walkable)
                this.LastHoveredCell.ChangeStateColor(Color.cyan);

            if (entity.CurrentGrid.IsCombatGrid)
            {
                this.ShowPossibleCombatMovements(entity);

                // Make sure that we're not using a card so we don't show the player's path
                if (CardDraggingSystem.instance.DraggedCard == null)
                {
                    // Clear old path
                    for (int i = 0; i < this.Path.Count; i++)
                        if (this.Path[i] != null)
                            this.Path[i].ChangeStateColor(Color.grey);

                    if (!entity.CurrentGrid.IsCombatGrid)
                        this._possiblePath = this.Path;

                    this.FindPath(entity, cell.PositionInGrid, cell.RefGrid);

                    if (!entity.CurrentGrid.IsCombatGrid || this.Path.Count <= CombatManager.Instance.CurrentPlayingEntity.Movement)
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

        public void ProcessCellClick(PositionEventData data)
        {
            if (this.LastHoveredCell == null)
                return;

            PlayerBehavior selfPlayer = GameManager.Instance.SelfPlayer;

            // Combat behavior
            if (selfPlayer.CurrentGrid.IsCombatGrid)
            {
                // When not grabbing card
                if (CardDraggingSystem.instance.DraggedCard == null)
                {
                    if (this.Path.Contains(this.LastHoveredCell) && this.LastHoveredCell.Datas.state == CellState.Walkable)
                    {
                        //TODO: Rework the combat /out-of - combat network callbacks and structure
                        selfPlayer.EntityCell
                            .ChangeCellState(CellState.Walkable);

                        this.ShowPossibleCombatMovements(GameManager.Instance.SelfPlayer);

                        selfPlayer.CurrentGrid
                            .Cells[this.LastHoveredCell.Datas.heightPos, this.LastHoveredCell.Datas.widthPos]
                                .ChangeCellState(CellState.EntityIn);
                    }

                }
                else if (CardDraggingSystem.instance.DraggedCard != null)
                {
                    CombatManager.Instance.PlayCard(this.LastHoveredCell);
                }
                CardDraggingSystem.instance.DraggedCard = null;
            }
            // When out of combat
            else
            {
                // TODO: Verify if this works
                selfPlayer.AskToGo(this.LastHoveredCell);
                //NetworkManager.Instance.ProcessAskedPath(selfPlayer, this.LastHoveredCell);
            }
            
        }

        public void ShowPossibleCombatMovements(CharacterEntity entity) 
        {
            int movePoints = entity.Movement;
            Cell entityCell = entity.EntityCell;

            // Clear the highlighted cells
            foreach (Cell cell in this._possiblePath)
                cell.ChangeStateColor(Color.grey);
            this._possiblePath.Clear();

            for (int x = -movePoints;x <= movePoints;x++) {
                for (int y = -movePoints;y <= movePoints;y++) {
                    if (x == 0 && y == 0 || (Mathf.Abs(x) + Mathf.Abs(y) > movePoints))
                        continue;

                    int checkX = entityCell.Datas.widthPos + x;
                    int checkY = entityCell.Datas.heightPos + y;

                    if (checkX >= 0 && checkX < entity.CurrentGrid.GridWidth && checkY >= 0 && checkY < entity.CurrentGrid.GridHeight) {
                        this.FindPath(entity, entity.CurrentGrid.Cells[checkY,checkX].PositionInGrid, entity.CurrentGrid);

                        if (this.Path.Contains(entity.CurrentGrid.Cells[checkY,checkX]) && this.Path.Count <= movePoints && (entity.CurrentGrid.Cells[checkY,checkX].Datas.state == CellState.Walkable)) {
                            this._possiblePath.Add(entity.CurrentGrid.Cells[checkY,checkX]);
                            entity.CurrentGrid.Cells[checkY,checkX].ChangeStateColor(new Color(0.82f,0.796f,0.5f,0.8f));
                        }
                    }
                }
            }
        }
        public CellData CellDataAtPosition(GridPosition target) 
        {
            Cell targetCell = this._currentCombatGrid.Cells[target.longitude,target.latitude];
            return targetCell.Datas;
        }


        public int[] SerializePathData()
        {
            int[] positions = new int[this.Path.Count * 2];
            for (int i = 0; i < this.Path.Count; i++) {
                positions[i * 2] = this.Path[i].PositionInGrid.longitude;
                positions[i * 2 + 1] = this.Path[i].PositionInGrid.latitude;
            }
                
            return positions;
        }

        public List<Cell> DeserializePathData(PlayerBehavior player, int[] positions)
        {
            List<Cell> result = new List<Cell>();
            for (int i = 0; i < positions.Length; i += 2)
                result.Add(player.CurrentGrid.Cells[positions[i], positions[i + 1]]);

            return result;
        }

        /// <summary>
        /// While calculate the closest path to a target, storing it in the Path var of the GridManager
        /// </summary>
        /// <param name="target"></param>
        public void FindPath(CharacterEntity entity, GridPosition target, bool directPath = false) 
        {
            if (entity == null)
                return;

            Cell startCell = entity.IsMoving ? entity.NextCell : entity.EntityCell;
            Cell targetCell = entity.CurrentGrid.Cells[target.longitude,target.latitude];

            List<Cell> openSet = new List<Cell>();
            HashSet<Cell> closedSet = new HashSet<Cell>();

            openSet.Add(startCell);

            while (openSet.Count > 0) {
                Cell currentCell = openSet[0];
                for (int i = 1;i < openSet.Count;i++) {
                    if (openSet[i].fCost < currentCell.fCost || openSet[i].fCost == currentCell.fCost && openSet[i].hCost < currentCell.hCost) {
                        currentCell = openSet[i];
                    }
                }

                openSet.Remove(currentCell);
                closedSet.Add(currentCell);

                if (currentCell == targetCell) {
                    this.RetracePath(startCell,targetCell);
                    if (this.Path.Count > 0 && this.Path[^1].Datas.state == CellState.EntityIn) {
                        this.Path.RemoveAt(Path.Count - 1);
                    }
                    return;
                }

                List<Cell> actNeighbours = entity.CurrentGrid.IsCombatGrid ? GetCombatNeighbours(currentCell, entity.CurrentGrid) : GetNormalNeighbours(currentCell, entity.CurrentGrid);
                foreach (Cell neighbour in actNeighbours) {
                    if (neighbour.Datas.state == CellState.Blocked || closedSet.Contains(neighbour))
                            continue;

                    int newMovementCostToNeightbour = currentCell.gCost + GetDistance(currentCell,neighbour);
                    if (newMovementCostToNeightbour < neighbour.gCost || !openSet.Contains(neighbour)) {
                        neighbour.gCost = newMovementCostToNeightbour;
                        neighbour.hCost = GetDistance(neighbour,targetCell);
                        neighbour.parent = currentCell;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }

        }
        public void RetracePath(Cell startCell,Cell endCell) 
        {
            List<Cell> path = new List<Cell>();
            Cell currentCell = endCell;
            string debug = "";
            while (currentCell != startCell) {
                debug = debug.Insert(0,$"{currentCell}\n");
                path.Add(currentCell);
                currentCell = currentCell.parent;
            }
            path.Reverse();
            Debug.Log("PATH: " + debug);
            this.Path = path;
        }
        /// <summary>
        /// To get the 8 neighbours around a cell. Used for out of combat walk.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public List<Cell> GetNormalNeighbours(Cell cell,WorldGrid grid) 
        {
            List<Cell> neighbours = new List<Cell>();

            for (int x = -1;x <= 1;x++) {
                for (int y = -1;y <= 1;y++) {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = cell.Datas.widthPos + x;
                    int checkY = cell.Datas.heightPos + y;

                    if ((checkX >= 0 && checkX < grid.GridWidth && checkY >= 0 && checkY < grid.GridHeight) &&
                        grid.Cells[checkY, checkX].Datas.state != CellState.Blocked) 
                    {
                        if(Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                        {
                            if(grid.Cells[y == -1 ? checkY + 1 : checkY - 1, checkX].Datas.state == CellState.Walkable ||
                               grid.Cells[checkY, x == -1 ? checkX + 1 : checkX - 1].Datas.state == CellState.Walkable)
                            {
                                neighbours.Add(grid.Cells[checkY, checkX]);
                            }
                        }
                        else
                            neighbours.Add(grid.Cells[checkY,checkX]);
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
        public List<Cell> GetCombatNeighbours(Cell cell,WorldGrid grid) 
        {
            List<Cell> neighbours = new List<Cell>();

            for (int x = -1;x <= 1;x++) {
                for (int y = -1;y <= 1;y++) {
                    if ((x == 0 && y == 0) || (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1))
                        continue;

                    int checkX = cell.Datas.widthPos + x;
                    int checkY = cell.Datas.heightPos + y;

                    if (checkX >= 0 && checkX < grid.GridWidth && checkY >= 0 && checkY < grid.GridHeight) {
                        neighbours.Add(grid.Cells[checkY,checkX]);
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
        public int GetDistance(Cell cellA,Cell cellB) {
            int dstX = Mathf.Abs(cellA.Datas.widthPos - cellB.Datas.widthPos);
            int dstY = Mathf.Abs(cellA.Datas.heightPos - cellB.Datas.heightPos);

            // 14 is the diagonal weight, used in out of combat walk.
            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }

        #region DATAS_MANIPULATION
        public void LoadGridsFromJSON() 
        {
            this.SavedGrids = new Dictionary<string,GridData>();

            TextAsset[] jsons = Resources.LoadAll<TextAsset>("Saves/Grids/" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            foreach (TextAsset json in jsons) {
                // not used but it may help the GridData deserialization to works well, so keep it
                JObject obj = JsonConvert.DeserializeObject<JObject>(json.text);
                GridData loadedData = JsonConvert.DeserializeObject<GridData>(json.text);

                this.SavedGrids.Add(json.name,loadedData);
            }
        }

        public void SaveGridAsJSON(WorldGrid grid) 
        {
            if (grid.UName == "" && grid.UName == string.Empty)
                return;

            List<CellData> savedCells = new List<CellData>();

            // Get the non walkable cells only
            for (int i = 0;i < grid.Cells.GetLength(0);i++)
                for (int j = 0;j < grid.Cells.GetLength(1);j++)
                    if (grid.Cells[i,j].Datas.state != CellState.Walkable)
                        savedCells.Add(grid.Cells[i,j].Datas);

            GridData gridData = new GridData();
            gridData.GridHeight = grid.GridHeight;
            gridData.GridWidth = grid.GridWidth;
            gridData.CellDatas = savedCells;

            string gridJson = JsonConvert.SerializeObject(gridData);
            this._saveJSONFile(gridJson,grid.UName);
        }
        public void SaveGridAsJSON(CellData[,] cellDatas,string name) 
        {
            if (name == "" && name == string.Empty)
                return;

            List<CellData> savedCells = new List<CellData>();
            for (int i = 0;i < cellDatas.GetLength(0);i++)
                for (int j = 0;j < cellDatas.GetLength(1);j++)
                    if (cellDatas[i,j].state != CellState.Walkable)
                        savedCells.Add(cellDatas[i,j]);

            GridData gridData = new GridData();
            gridData.GridHeight = cellDatas.GetLength(0);
            gridData.GridWidth = cellDatas.GetLength(1);
            gridData.CellDatas = savedCells;

            string gridJson = JsonConvert.SerializeObject(gridData);
            this._saveJSONFile(gridJson,name);
        }

        public void SaveGridAsJSON(GridData grid,string uName) 
        {
            if (uName == "" && uName == string.Empty)
                return;

            // We have to ignore the loops because Vector3.Normalize would fck up the serialization :)
            JsonSerializerSettings Jss = new JsonSerializerSettings();
            Jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            string gridJson = JsonConvert.SerializeObject(grid, Jss);
            this._saveJSONFile(gridJson,uName);
        }

        private void _saveJSONFile(string json, string pathName) 
        {
            string currScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string path = Application.dataPath + "/Resources/Saves/Grids/" + currScene + "/" + pathName + ".json";
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllText(path,json);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif

            this.LoadGridsFromJSON();
        }

        public void LoadEveryEntities()
        {
            var enemyEntities = Resources.LoadAll<EntitySpawn>("Presets/Entity/Enemies").ToList();
            var NPCEntities = Resources.LoadAll<EntitySpawn>("Presets/Entity/NPCs").ToList();

            this.EnemiesSpawnSO = new Dictionary<Guid, EntitySpawn>();
            this.NPCsSpawnsSO = new Dictionary<Guid, EntitySpawn>();

            foreach (var enemy in enemyEntities)
                this.EnemiesSpawnSO.Add(enemy.UID, enemy);
            foreach (var npc in NPCEntities)
                this.NPCsSpawnsSO.Add(npc.UID, npc);
        }
        #endregion
    }
    
    [Serializable]
    public struct GridPosition
    {
        public GridPosition(int longitude, int latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }

        public static readonly GridPosition zero = new GridPosition(0,0);

        public int longitude { get; private set; }
        public int latitude { get; private set; }
        public override string ToString() {
            return $"({longitude},{latitude})";
        }

        public static object Deserialize(byte[] data) 
        {
            return new GridPosition(data[0], data[1]);
        }

        public static byte[] Serialize(object position)
        {
            GridPosition pos = (GridPosition)position;
            return new byte[] { (byte)pos.longitude, (byte)pos.latitude };
        }
    }
}
