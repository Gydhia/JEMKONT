using DownBelow.GridSystem;
using DownBelow.Entity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using System;
using DownBelow.Events;
using UnityEngine.Rendering;
using EODE.Wonderland;
using MyBox;
using DownBelow.Events;

namespace DownBelow.Managers {
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
        public Dictionary<string,GridData> SavedGrids;

        public Dictionary<Guid, BaseSpawnablePreset> SpawnablesPresets;
        public Dictionary<Guid, ItemPreset> ItemsPresets;

        #endregion


        public List<Cell> Path;
        private List<Cell> _possiblePath = new List<Cell>();

        public Dictionary<string,WorldGrid> WorldGrids;

        private CombatGrid _currentCombatGrid;
        public WorldGrid MainWorldGrid;
        public GameObject TestPlane;

        public Cell LastHoveredCell;

        public override void Awake() {
            base.Awake();

            this.WorldGrids = new Dictionary<string,WorldGrid>();
            // Load the Grids and Entities SO
            this.LoadGridsFromJSON();
            this.LoadEveryEntities();

            Destroy(this._gridsDataHandler.gameObject);

            foreach (var savedGrid in this.SavedGrids) {
                // Only load the saves that are indicated so
                if (savedGrid.Value.ToLoad) {
                    this.GenerateGrid(savedGrid.Value,savedGrid.Key);
                }
            }

            // Events
            InputManager.Instance.OnCellClickedUp += this.ProcessCellClickUp;
            InputManager.Instance.OnCellClickedDown += this.ProcessCellClickDown;

            GameManager.Instance.OnEnteredGrid += this.OnEnteredNewGrid;
        }

        public void GenerateGrid(GridData gridData,string gridId) {
            WorldGrid newGrid = Instantiate(this.CombatGridPrefab,gridData.TopLeftOffset,Quaternion.identity,this._objectsHandler);

            newGrid.UName = gridId;
            newGrid.Init(gridData);

            this.WorldGrids.Add(newGrid.UName,newGrid);

            this.MainWorldGrid = this.WorldGrids.Values.First();
        }

        public void OnNewCellHovered(CharacterEntity entity,Cell cell) {
            if (this.LastHoveredCell != null && this.LastHoveredCell.Datas.state == CellState.Walkable)
                this.LastHoveredCell.ChangeStateColor(Color.grey);

            this.LastHoveredCell = cell;
            if (CardDraggingSystem.instance.DraggedCard != null && this.LastHoveredCell.Datas.state == CellState.Walkable)
                this.LastHoveredCell.ChangeStateColor(Color.cyan);

            if (entity.CurrentGrid is CombatGrid cGrid && cGrid.HasStarted && this.LastHoveredCell.RefGrid == entity.CurrentGrid) {
                this.ShowPossibleCombatMovements(entity);

                // Make sure that we're not using a card so we don't show the player's path
                if (CardDraggingSystem.instance.DraggedCard == null) {
                    // Clear old path
                    for (int i = 0;i < this.Path.Count;i++)
                        if (this.Path[i] != null)
                            this.Path[i].ChangeStateColor(Color.grey);

                    if (!entity.CurrentGrid.IsCombatGrid)
                        this._possiblePath = this.Path;

                    this.FindPath(entity,cell.PositionInGrid,cell.RefGrid);

                    if (!entity.CurrentGrid.IsCombatGrid || this.Path.Count <= entity.Speed) {
                        for (int i = 0;i < this.Path.Count;i++) {
                            if (this.Path[i] != null)
                                this.Path[i].ChangeStateColor(Color.green);
                        }
                    }
                }
            }
        }

        public void ProcessCellClickUp(PositionEventData data) {
            if (this.LastHoveredCell == null)
                return;

            PlayerBehavior selfPlayer = GameManager.Instance.SelfPlayer;

            // Combat behavior
            if (selfPlayer.CurrentGrid.IsCombatGrid) {
                // When not grabbing card
                if (CardDraggingSystem.instance.DraggedCard == null) {
                    if (selfPlayer.IsAutoAttacking) {
                        if (LastHoveredCell.Datas.state == CellState.EntityIn) {
                            if (!selfPlayer.isInAttackRange(LastHoveredCell)) selfPlayer.IsAutoAttacking = false; else selfPlayer.AutoAttack(LastHoveredCell);
                        }
                    } else if (this.Path.Contains(this.LastHoveredCell) && this.LastHoveredCell.Datas.state == CellState.Walkable) {
                        //TODO: Rework the combat /out-of - combat network callbacks and structure
                        selfPlayer.AskToGo(this.LastHoveredCell,string.Empty);

                        selfPlayer.EntityCell
                            .ChangeCellState(CellState.Walkable);

                        this.ShowPossibleCombatMovements(GameManager.Instance.SelfPlayer);

                        selfPlayer.CurrentGrid
                            .Cells[this.LastHoveredCell.Datas.heightPos,this.LastHoveredCell.Datas.widthPos]
                                .ChangeCellState(CellState.EntityIn);
                    }

                } else if (CardDraggingSystem.instance.DraggedCard != null) {
                    CombatManager.Instance.PlayCard(this.LastHoveredCell);
                }
                CardDraggingSystem.instance.DraggedCard = null;
            }
            // When out of combat
            else {
                Cell closestCell = this.LastHoveredCell;
                string otherGrid = string.Empty;

                if (InputManager.Instance.LastInteractable != null)
                {
                    Cell cell = InputManager.Instance.LastInteractable.RefCell;
                    closestCell = GridUtility.GetClosestCellToShape(selfPlayer.CurrentGrid, cell.Datas.heightPos, cell.Datas.widthPos, 0, 0, selfPlayer.EntityCell.PositionInGrid);
                    selfPlayer._nextInteract = InputManager.Instance.LastInteractable;
                }
                else
                {
                    if (selfPlayer.CurrentGrid != this.LastHoveredCell.RefGrid)
                    {
                        closestCell = GridUtility.GetClosestCellToShape(selfPlayer.CurrentGrid, this.LastHoveredCell.RefGrid as CombatGrid, selfPlayer.EntityCell.PositionInGrid);
                        otherGrid = this.LastHoveredCell.RefGrid.UName;
                    }
                }
                if (closestCell != null)
                    selfPlayer.AskToGo(closestCell, otherGrid);
            }

        }
        public void ProcessCellClickDown(PositionEventData data) {
            if (this.LastHoveredCell == null)
                return;
            PlayerBehavior selfPlayer = GameManager.Instance.SelfPlayer;
            // Combat behavior
            if (selfPlayer.CurrentGrid.IsCombatGrid) {
                if (CombatManager.Instance.CurrentPlayingEntity == selfPlayer && selfPlayer.EntityCell == LastHoveredCell &&  selfPlayer.CanAutoAttack) {
                    selfPlayer.IsAutoAttacking = true;
                }
            }
        }

        public void ShowPossibleCombatMovements(CharacterEntity entity) {
            int movePoints = entity.Speed;
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
                        this.FindPath(entity,entity.CurrentGrid.Cells[checkY,checkX].PositionInGrid,entity.CurrentGrid);

                        if (this.Path.Contains(entity.CurrentGrid.Cells[checkY,checkX]) && this.Path.Count <= movePoints && (entity.CurrentGrid.Cells[checkY,checkX].Datas.state == CellState.Walkable)) {
                            this._possiblePath.Add(entity.CurrentGrid.Cells[checkY,checkX]);
                            entity.CurrentGrid.Cells[checkY,checkX].ChangeStateColor(new Color(0.82f,0.796f,0.5f,0.8f));
                        }
                    }
                }
            }
            if (entity.Confused) {
                if(entity.Is<PlayerBehavior>())
                    NetworkManager.Instance.PlayerAsksForPath((PlayerBehavior)entity,_possiblePath.Random(),string.Empty);
                else {
                    NetworkManager.Instance.EntityAsksForPath(entity, _possiblePath.Random(), entity.CurrentGrid);
                }
            }
        }
        public Cell RandomCellInPossiblePath() {
            return _possiblePath.Random();
        }
        public void OnEnteredNewGrid(EntityEventData Data) {
            // Affect the visuals ONLY if we are the player transitionning
            if (Data.Entity == GameManager.Instance.SelfPlayer) {
                Data.Entity.CurrentGrid.ShowHideGrid(true);
                if (Data.Entity.CurrentGrid.IsCombatGrid) {
                    ((CombatGrid)Data.Entity.CurrentGrid).ParentGrid.ShowHideGrid(false);
                } else {
                    foreach (CombatGrid grid in Data.Entity.CurrentGrid.InnerCombatGrids.Values) {
                        grid.ShowHideGrid(false);
                    }
                }
                foreach (PlayerBehavior player in GameManager.Instance.Players.Values) {
                    if (player.CurrentGrid != Data.Entity.CurrentGrid)
                        player.gameObject.SetActive(false);
                    else
                        player.gameObject.SetActive(true);
                }
            }
            // Make that stranger disappear / appear according to our grid
            else {
                // IMPORTANT : Remember that Disabled GameObjects would disable their scripts to.
                // Only MasterClient have to handle combat Datas, we'll do as it follows : 
                // When joining a grid already in combat, load the values from MasterClient if we're not, else handle everything.
                if (GameManager.Instance.SelfPlayer.CurrentGrid != Data.Entity.CurrentGrid) {
                    if (Photon.Pun.PhotonNetwork.IsMasterClient)
                        Data.Entity.gameObject.SetActive(false);
                    else
                        Data.Entity.gameObject.SetActive(false);
                } else {
                    if (Photon.Pun.PhotonNetwork.IsMasterClient)
                        Data.Entity.gameObject.SetActive(true);
                    else
                        Data.Entity.gameObject.SetActive(true);
                }
            }
        }

        #region PATH_FINDING
        public CellData CellDataAtPosition(GridPosition target) {
            Cell targetCell = this._currentCombatGrid.Cells[target.longitude,target.latitude];
            return targetCell.Datas;
        }


        public int[] SerializePathData() {
            int[] positions = new int[this.Path.Count * 2];
            for (int i = 0;i < this.Path.Count;i++) {
                positions[i * 2] = this.Path[i].PositionInGrid.longitude;
                positions[i * 2 + 1] = this.Path[i].PositionInGrid.latitude;
            }

            return positions;
        }

        public List<Cell> DeserializePathData(CharacterEntity entity,int[] positions) {
            List<Cell> result = new List<Cell>();
            for (int i = 0;i < positions.Length;i += 2)
                result.Add(entity.CurrentGrid.Cells[positions[i + 1],positions[i]]);

            return result;
        }

        /// <summary>
        /// While calculate the closest path to a target, storing it in the Path var of the GridManager
        /// </summary>
        /// <param name="target"></param>
        public void FindPath(CharacterEntity entity,GridPosition target,bool directPath = false) {
            if (entity == null)
                return;

            Cell startCell = entity.IsMoving ? entity.NextCell : entity.EntityCell;
            Cell targetCell = entity.CurrentGrid.Cells[target.latitude,target.longitude];

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

                List<Cell> actNeighbours = entity.CurrentGrid.IsCombatGrid ? GetCombatNeighbours(currentCell,entity.CurrentGrid) : GetNormalNeighbours(currentCell,entity.CurrentGrid);
                foreach (Cell neighbour in actNeighbours) {
                    if ((neighbour.Datas.state == CellState.Blocked || neighbour.Datas.state == CellState.Shared || closedSet.Contains(neighbour)) || directPath)
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

        public void RetracePath(Cell startCell,Cell endCell) {
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
        public List<Cell> GetNormalNeighbours(Cell cell,WorldGrid grid,bool directpath = false) {
            List<Cell> neighbours = new List<Cell>();

            for (int x = -1;x <= 1;x++) {
                for (int y = -1;y <= 1;y++) {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = cell.Datas.widthPos + x;
                    int checkY = cell.Datas.heightPos + y;

                    if (directpath || ((checkX >= 0 && checkX < grid.GridWidth && checkY >= 0 && checkY < grid.GridHeight)
                        && (grid.Cells[checkY,checkX] != null && grid.Cells[checkY,checkX].Datas.state != CellState.Blocked))) {
                        if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1) {
                            int nX = x == -1 ? checkX + 1 : checkX - 1;
                            int nY = y == -1 ? checkY + 1 : checkY - 1;

                            if (directpath || ((grid.Cells[nY,checkX] != null && grid.Cells[nY,checkX].Datas.state == CellState.Walkable) ||
                                (grid.Cells[checkY,nX] != null && grid.Cells[checkY,nX].Datas.state == CellState.Walkable))) {
                                neighbours.Add(grid.Cells[checkY,checkX]);
                            }
                        } else
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
        public List<Cell> GetCombatNeighbours(Cell cell,WorldGrid grid) {
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
        #endregion

        #region DATAS_MANIPULATION
        public void LoadGridsFromJSON() {
            this.SavedGrids = new Dictionary<string,GridData>();

            TextAsset[] jsons = Resources.LoadAll<TextAsset>("Saves/Grids/" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            foreach (TextAsset json in jsons) {
                // not used but it may help the GridData deserialization to works well, so keep it
                JObject obj = JsonConvert.DeserializeObject<JObject>(json.text);
                GridData loadedData = JsonConvert.DeserializeObject<GridData>(json.text);

                this.SavedGrids.Add(json.name,loadedData);
            }
        }

        public void SaveGridAsJSON(WorldGrid grid) {
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
        public void SaveGridAsJSON(CellData[,] cellDatas,string name) {
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

        public void SaveGridAsJSON(GridData grid,string uName) {
            if (uName == "" && uName == string.Empty)
                return;

            // We have to ignore the loops because Vector3.Normalize would fck up the serialization :)
            JsonSerializerSettings Jss = new JsonSerializerSettings();
            Jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            string gridJson = JsonConvert.SerializeObject(grid,Jss);
            this._saveJSONFile(gridJson,uName);
        }

        private void _saveJSONFile(string json,string pathName) {
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
            var spawnablesPresets = Resources.LoadAll<BaseSpawnablePreset>("Presets").ToList();
            var itemsPresets = Resources.LoadAll<ItemPreset>("Presets/Inventory/Items");

            this.SpawnablesPresets = new Dictionary<Guid, BaseSpawnablePreset>();
            this.ItemsPresets = new Dictionary<Guid, ItemPreset>();

            foreach (var spawnable in spawnablesPresets)
                this.SpawnablesPresets.Add(spawnable.UID, spawnable);
            foreach (var item in itemsPresets)
                this.ItemsPresets.Add(item.UID, item);
        }
        #endregion
    }

    [Serializable]
    public struct GridPosition {
        public GridPosition(int longitude,int latitude) {
            this.longitude = longitude;
            this.latitude = latitude;
        }

        public static readonly GridPosition zero = new GridPosition(0,0);

        public int longitude { get; private set; }
        public int latitude { get; private set; }
        public override string ToString() {
            return $"({longitude},{latitude})";
        }

        public static object Deserialize(byte[] data) {
            return new GridPosition(data[0],data[1]);
        }

        public static byte[] Serialize(object position) {
            GridPosition pos = (GridPosition)position;
            return new byte[] { (byte)pos.longitude,(byte)pos.latitude };
        }
    }
}
