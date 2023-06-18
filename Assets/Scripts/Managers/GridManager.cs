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
using DownBelow.UI;
using System.Data.SqlTypes;
using UnityEditor;
using DownBelow.UI.Inventory;

namespace DownBelow.Managers
{
    public enum Border
    {
        None = 0,

        Left = 1,
        Right = 2,
        Top = 3,
        Bottom = 4
    }

    public class GridManager : _baseManager<GridManager>
    {
        public static readonly string SavesPath = "/Resources/Saves/Grids/";

        #region Assets_reference
        [ValueDropdown("GetSavedGrids")]
        public string MainGrid;
        private IEnumerable<string> GetSavedGrids()
        {
            LoadGridsFromJSON();
            return SavedGrids.Keys;
        }

        public Cell CellPrefab;

        public CombatGrid CombatGridPrefab;
        public WorldGrid WorldGridPrefab;

        [SerializeField]
        private Transform _gridsHandler;

        [SerializeField]
        public Transform _gridsDataHandler;

#endregion

        public bool InCombat = false;

#region Datas
        public Dictionary<string, GridData> SavedGrids;
        #endregion

        public static BaseStorage SavePurposeStorage;

        private List<Cell> _possiblePath = new List<Cell>();

        public Dictionary<string, WorldGrid> WorldGrids;

        private CombatGrid _currentCombatGrid;

        public WorldGrid MainWorldGrid;
        public Texture2D BitmapTexture;
        public Material BitmapShader;
        public Material BitmapShaderEditor;
        public GameObject GridShader;

        public Cell LastHoveredCell;

        private ArrowRenderer _spellArrow;

        public WorldGrid GetGridFromName(string name)
        {
            if (this.WorldGrids.ContainsKey(name))
                return this.WorldGrids[name];
            else
            {
                foreach (var worldGrid in this.WorldGrids)
                {
                    if (worldGrid.Value.InnerCombatGrids.ContainsKey(name))
                        return worldGrid.Value.InnerCombatGrids[name];
                }
            }

            return null;
        }

        public void Init()
        {
            base.Awake();

            // Events
            if(InputManager.Instance != null)
            {
                InputManager.Instance.OnCellClickedUp += this.ProcessCellClickUp;
                InputManager.Instance.OnCellClickedDown += this.ProcessCellClickDown;
                InputManager.Instance.OnNewCellHovered += this.ProcessNewCellHovered;
            }
            
            GameManager.Instance.OnEnteredGrid += this.OnEnteredNewGrid;
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnCellClickedUp -= this.ProcessCellClickUp;
                InputManager.Instance.OnCellClickedDown -= this.ProcessCellClickDown;
                InputManager.Instance.OnNewCellHovered -= this.ProcessNewCellHovered;
            }

            GameManager.Instance.OnEnteredGrid -= this.OnEnteredNewGrid;
        }

        public void CreateWholeWorld(GameData.GameDataContainer refGameDataContainer)
        {
            // Pre-instantiate the spell's arrow indicator
            this._spellArrow = Instantiate(
                SettingsManager.Instance.GridsPreset.SpellArrowPrefab,
                this.transform
            );
            this._spellArrow.Init();
            this._spellArrow.gameObject.SetActive(false);

            this.WorldGrids = new Dictionary<string, WorldGrid>();
            
            if(this._gridsDataHandler != null)
            {
                Destroy(this._gridsDataHandler.gameObject);
            }

            // TODO : Plug it in a scriptable instead of hardcoding it like that
            foreach (var grid in refGameDataContainer.Data.grids_data)
            {
                this.CreateGrid(grid, grid.GridName);
            }

            this.MainWorldGrid = this.WorldGrids[this.MainGrid];
            this.MainWorldGrid.gameObject.SetActive(true);

            UniversalRenderPipelineHelper.SetRendererFeatureActive("GridRender", false);
            //this.GenerateShaderBitmap(this.MainWorldGrid.SelfData);
        }

        public void CreateGrid(GridData gridData, string gridId)
        {
            var prefab = gridData.IsCombatGrid ? this.CombatGridPrefab : this.WorldGridPrefab;

            WorldGrid newGrid = Instantiate(
                prefab,
                this._gridsHandler
            );

            newGrid.UName = gridId;
            newGrid.Init(gridData);

            this.WorldGrids.Add(newGrid.UName, newGrid);

            newGrid.gameObject.SetActive(false);
        }

        public void ProcessCellClickUp(CellEventData data)
        {
            if (this.LastHoveredCell == null)
                return;

            PlayerBehavior selfPlayer = GameManager.SelfPlayer;

            if (selfPlayer.CurrentGrid.IsCombatGrid)
                this.ProcessCellClickUp_Combat(selfPlayer);
            else
                this.ProcessCellClickUp_Exploration(selfPlayer);
        }

        public void ProcessCellClickUp_Exploration(PlayerBehavior selfPlayer)
        {
            if (this.LastHoveredCell.Datas.state != CellState.Blocked)
            {
                Cell closestCell = this.LastHoveredCell;

                UIManager.Instance.HideInteractables();

                if (InputManager.Instance.LastInteractable != null && InputManager.Instance.LastInteractable.RefCell != null)
                {
                    Cell cell = InputManager.Instance.LastInteractable.RefCell;

                    closestCell = GridUtility.GetNearestWalkableCell(cell, selfPlayer.EntityCell.PositionInGrid);

                    if(closestCell != null)
                    {
                        selfPlayer.NextInteract = InputManager.Instance.LastInteractable;

                        EntityAction[] actions = new EntityAction[2];

                        // Buff the movement action if we're too far away
                        if (!GetCombatNeighbours(selfPlayer.EntityCell, selfPlayer.EntityCell.RefGrid).Contains(cell))
                            actions[0] = new MovementAction(selfPlayer, closestCell);

                        // Then buff the interact/gather action to process after the movement
                        if (cell.AttachedInteract is InteractableResource iResource)
                        {
                            if (selfPlayer.CanGatherThisResource(iResource.LocalPreset.GatherableBy))
                            {
                                if (iResource.isMature)
                                {
                                    var gatherAction = new GatheringAction(selfPlayer, cell);
                                    gatherAction.Init(3);

                                    actions[1] = gatherAction;
                                }
                                else
                                {
                                    UIManager.Instance.DatasSection.ShowWarningText(iResource.LocalPreset.UName + " is not available");
                                }
                            }
                            else
                            {
                                UIManager.Instance.DatasSection.ShowWarningText("[REQUIRES " + iResource.LocalPreset.GatherableBy.ToString() + "]\nYou haven't the right tool to gather this");
                            }
                        }
                        else
                        {
                            actions[1] = new InteractAction(selfPlayer, cell);
                        }

                        NetworkManager.Instance.EntityAskToBuffActions(actions);
                    }
                    else
                    {
                        UIManager.Instance.DatasSection.ShowWarningText("You can't interact with an unreachable cell");
                    }

                    return;
                } 
                else if (this.LastHoveredCell.HasItem(out var item))
                {
                    //If we have a dropped item on the cell.
                    EntityAction[] actions = new EntityAction[2];
                    actions[0] = new MovementAction(selfPlayer, LastHoveredCell);
                    actions[1] = new PickupItemAction(selfPlayer, this.LastHoveredCell);
                    NetworkManager.Instance.EntityAskToBuffActions(actions);
                    return;
                }


                if (closestCell != null)
                {
                    if (closestCell.RedirectedGrid == null)
                    {
                        NetworkManager.Instance.EntityAskToBuffAction(
                            new MovementAction(selfPlayer, closestCell)
                        );
                    }
                    else
                    {
                        var enterAction = new EnterGridAction(selfPlayer, closestCell);
                        enterAction.Init(closestCell.RedirectedGrid.UName);
                        
                        NetworkManager.Instance.EntityAskToBuffActions(
                            new EntityAction[2] {
                                new MovementAction(selfPlayer, closestCell),
                                enterAction
                            });
                    }
                }
            }
        }

        public void ProcessCellClickUp_Combat(PlayerBehavior player)
        {
            if (player.IsPlayingEntity)
            {
                // When not grabbing card
                if (UI.DraggableCard.SelectedCard == null)
                {
                    if (player.IsAutoAttacking)
                    {
                        if (LastHoveredCell.Datas.state == CellState.EntityIn)
                        {
                            if (!player.isInAttackRange(LastHoveredCell))
                                player.IsAutoAttacking = false;
                            else
                                player.AutoAttack(LastHoveredCell);
                        }
                    } else if (
                          this.LastHoveredCell.Datas.state == CellState.Walkable
                          && this._possiblePath.Contains(this.LastHoveredCell)
                      )
                    {
                        NetworkManager.Instance.EntityAskToBuffAction(
                            new CombatMovementAction(player, this.LastHoveredCell)
                        );
                    }
                }
            }
            // Clicking to select placement
            else if (player.CurrentGrid is CombatGrid cGrid && !cGrid.HasStarted)
            {
                if (LastHoveredCell.IsPlacementCell && player.EntityCell != LastHoveredCell)
                {
                    player.Teleport(LastHoveredCell);
                }
            }
        }

        public void ProcessCellClickDown(CellEventData data)
        {
            if (this.LastHoveredCell == null)
                return;

            PlayerBehavior selfPlayer = GameManager.RealSelfPlayer;

            if (selfPlayer.CurrentGrid.IsCombatGrid)
                this.ProcessCellClickDown_Combat(selfPlayer);
            else
                this.ProcessCellClickDown_Exploration(selfPlayer);
        }

        public void ProcessCellClickDown_Exploration(PlayerBehavior selfPlayer) { }

        public void ProcessCellClickDown_Combat(PlayerBehavior selfPlayer)
        {
            if (
                CombatManager.CurrentPlayingEntity == selfPlayer
                && selfPlayer.EntityCell == LastHoveredCell
                && selfPlayer.CanAutoAttack
            )
                selfPlayer.IsAutoAttacking = true;
        }

        public void ProcessNewCellHovered(CellEventData Data)
        {
            if (!GameManager.GameStarted)
                return;

            PlayerBehavior selfPlayer = GameManager.SelfPlayer;
            this.LastHoveredCell = Data.Cell;

            if (
                selfPlayer.CurrentGrid is CombatGrid cGrid
                && cGrid.HasStarted
                && this.LastHoveredCell.RefGrid == selfPlayer.CurrentGrid
            )
            {
                if (DraggableCard.SelectedCard == null && selfPlayer.IsPlayingEntity)
                {
                    if (!selfPlayer.IsMoving && this._possiblePath.Contains(LastHoveredCell))
                        PoolManager.Instance.CellIndicatorPool.DisplayPathIndicators(
                            this.FindPath(selfPlayer, LastHoveredCell.PositionInGrid)
                        );
                    else
                        PoolManager.Instance.CellIndicatorPool.HidePathIndicators();
                }
            }
        }

        /// <summary>
        /// To pre-calculate the possible cells where the entity can go in combat
        /// </summary>
        /// <param name="entity">The entity focused to calcualte the cells</param>
        /// <param name="cell">an override to the entity cell</param>
        public void CalculatePossibleCombatMovements(CharacterEntity entity)
        {
            int movePoints = entity.Speed;
            Cell entityCell = entity.EntityCell;
            List<Cell> path = new List<Cell>();

            this._possiblePath.Clear();

            for (int x = -movePoints;x <= movePoints;x++)
            {
                for (int y = -movePoints;y <= movePoints;y++)
                {
                    if (x == 0 && y == 0 || (Mathf.Abs(x) + Mathf.Abs(y) > movePoints))
                        continue;

                    int checkX = entityCell.Datas.widthPos + x;
                    int checkY = entityCell.Datas.heightPos + y;

                    if (
                        checkX >= 0
                        && checkX < entity.CurrentGrid.GridWidth
                        && checkY >= 0
                        && checkY < entity.CurrentGrid.GridHeight
                    )
                    {
                        path = this.FindPath(
                            entity,
                            entity.CurrentGrid.Cells[checkY, checkX].PositionInGrid
                        );

                        if (
                            path != null
                            && path.Contains(entity.CurrentGrid.Cells[checkY, checkX])
                            && path.Count <= movePoints
                            && (
                                entity.CurrentGrid.Cells[checkY, checkX].Datas.state
                                == CellState.Walkable
                            )
                        )
                        {
                            this._possiblePath.Add(entity.CurrentGrid.Cells[checkY, checkX]);
                        }
                    }
                }
            }
        }

        public Cell RandomCellInPossiblePath()
        {
            return _possiblePath.Random();
        }

        public void OnEnteredNewGrid(EntityEventData Data)
        {
            // Affect the visuals ONLY if we are the player transitionning
            if (Data.Entity == GameManager.SelfPlayer)
            {
                if (Data.Entity.CurrentGrid.IsCombatGrid)
                {
                    this.GenerateShaderBitmap(
                        ((CombatGrid)Data.Entity.CurrentGrid).ParentGrid.SelfData,
                        Data.Entity.CurrentGrid.SelfData
                    );
                }
                else
                {
                    // We decided to not have grid when out of combat
                    this._disableGridTexture();
                    //this.GenerateShaderBitmap(Data.Entity.CurrentGrid.SelfData);
                }

                foreach (PlayerBehavior player in GameManager.Instance.Players.Values)
                {
                    //if (player.CurrentGrid != Data.Entity.CurrentGrid)
                    //    player.gameObject.SetActive(false);
                    //else
                        player.gameObject.SetActive(true);
                }
            }
            // Make that stranger disappear / appear according to our grid
            else
            {
                // IMPORTANT : Remember that Disabled GameObjects would disable their scripts to.
                // Only MasterClient have to handle combat Datas, we'll do as it follows :
                // When joining a grid already in combat, load the values from MasterClient if we're not, else handle everything.
                if (GameManager.SelfPlayer.CurrentGrid != Data.Entity.CurrentGrid)
                {
                    if (Photon.Pun.PhotonNetwork.IsMasterClient)
                        Data.Entity.gameObject.SetActive(false);
                    else
                        Data.Entity.gameObject.SetActive(false);
                } else
                {
                    if (Photon.Pun.PhotonNetwork.IsMasterClient)
                        Data.Entity.gameObject.SetActive(true);
                    else
                        Data.Entity.gameObject.SetActive(true);
                }
            }
        }

#region PATH_FINDING
        public CellData CellDataAtPosition(GridPosition target)
        {
            Cell targetCell = this._currentCombatGrid.Cells[target.longitude, target.latitude];
            return targetCell.Datas;
        }

        public int[] SerializePathData(List<Cell> path)
        {
            int[] positions = new int[path.Count * 2];
            for (int i = 0;i < path.Count;i++)
            {
                positions[i * 2] = path[i].PositionInGrid.longitude;
                positions[i * 2 + 1] = path[i].PositionInGrid.latitude;
            }

            return positions;
        }

        public List<Cell> DeserializePathData(CharacterEntity entity, int[] positions)
        {
            List<Cell> result = new List<Cell>();
            for (int i = 0;i < positions.Length;i += 2)
                result.Add(entity.CurrentGrid.Cells[positions[i + 1], positions[i]]);

            return result;
        }

        /// <summary>
        /// While calculate the closest path to a target, storing it in the Path var of the GridManager
        /// </summary>
        /// <param name="target"></param>
        public List<Cell> FindPath(CharacterEntity entity, GridPosition target, bool directPath = false, int Range = -1)
        {
            if (entity == null)
                return null;

            Cell startCell = entity.IsMoving && entity.NextCell != null ? entity.NextCell : entity.EntityCell;
            Cell targetCell = entity.CurrentGrid.Cells[target.latitude, target.longitude];

            List<Cell> finalPath;

            List<Cell> openSet = new List<Cell>();
            HashSet<Cell> closedSet = new HashSet<Cell>();

            openSet.Add(startCell);

            while (openSet.Count > 0)
            {
                Cell currentCell = openSet[0];
                for (int i = 1;i < openSet.Count;i++)
                {
                    if (
                        openSet[i].fCost < currentCell.fCost
                        || openSet[i].fCost == currentCell.fCost
                        && openSet[i].hCost < currentCell.hCost
                    )
                    {
                        currentCell = openSet[i];
                    }
                }

                openSet.Remove(currentCell);
                closedSet.Add(currentCell);

                // We go there at the end of the path
                if (currentCell == targetCell || Range >= 0 && IsInRange(currentCell.PositionInGrid, targetCell.PositionInGrid, Range))
                {
                    if (Range >= 0)
                        targetCell = currentCell;

                    return this.RetracePath(startCell, targetCell);
                }

                List<Cell> actNeighbours = entity.CurrentGrid.IsCombatGrid
                    ? GetCombatNeighbours(currentCell, entity.CurrentGrid)
                    : GetNormalNeighbours(currentCell, entity.CurrentGrid);
                foreach (Cell neighbour in actNeighbours)
                {
                    if (CellState.NonWalkable.HasFlag(neighbour.Datas.state) && (!directPath || (directPath && neighbour.Datas.state == CellState.Blocked)) || closedSet.Contains(neighbour))
                        continue;

                    int newMovementCostToNeightbour =
                        currentCell.gCost + GetDistance(currentCell, neighbour);
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

            // We couldn't find a path to the target
            return null;
        }

        /// <summary>
        /// Cells have an assigned parent when calculating the path, this method is used to retrace this path parent from parent
        /// </summary>
        /// <param name="startCell"></param>
        /// <param name="endCell"></param>
        /// <returns></returns>
        public List<Cell> RetracePath(Cell startCell, Cell endCell)
        {
            List<Cell> path = new List<Cell>();
            Cell currentCell = endCell;

            while (currentCell != startCell)
            {
                path.Add(currentCell);
                currentCell = currentCell.parent;
            }

            path.Reverse();
            return path;
        }

        private bool IsInRange(GridPosition CurrentPosition, GridPosition TargetPosition, int Range)
        {
            bool latitudeOnRange = false;
            bool longitudeOnRange = false;

            if (
                TargetPosition.latitude - Range <= CurrentPosition.latitude
                && CurrentPosition.latitude <= TargetPosition.latitude + Range
            )
                latitudeOnRange = true;

            if (
                TargetPosition.longitude - Range <= CurrentPosition.longitude
                && CurrentPosition.longitude <= TargetPosition.longitude + Range
            )
                longitudeOnRange = true;

            return latitudeOnRange && longitudeOnRange;
        }

        /// <summary>
        /// To get the 8 neighbours around a cell. Used for out of combat walk.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public List<Cell> GetNormalNeighbours(Cell cell, WorldGrid grid, bool directpath = false)
        {
            List<Cell> neighbours = new List<Cell>();

            for (int x = -1;x <= 1;x++)
            {
                for (int y = -1;y <= 1;y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = cell.Datas.widthPos + x;
                    int checkY = cell.Datas.heightPos + y;

                    if (
                        directpath
                        || (
                            (
                                checkX >= 0
                                && checkX < grid.GridWidth
                                && checkY >= 0
                                && checkY < grid.GridHeight
                            )
                            && (
                                grid.Cells[checkY, checkX] != null
                                && grid.Cells[checkY, checkX].Datas.state != CellState.NonWalkable
                            )
                        )
                    )
                    {
                        if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                        {
                            int nX = x == -1 ? checkX + 1 : checkX - 1;
                            int nY = y == -1 ? checkY + 1 : checkY - 1;

                            if (
                                directpath
                                || (
                                    (
                                        grid.Cells[nY, checkX] != null
                                        && grid.Cells[nY, checkX].Datas.state == CellState.Walkable
                                    )
                                    || (
                                        grid.Cells[checkY, nX] != null
                                        && grid.Cells[checkY, nX].Datas.state == CellState.Walkable
                                    )
                                )
                            )
                            {
                                neighbours.Add(grid.Cells[checkY, checkX]);
                            }
                        } else
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
        public List<Cell> GetCombatNeighbours(Cell cell, WorldGrid grid)
        {
            List<Cell> neighbours = new List<Cell>();

            for (int x = -1;x <= 1;x++)
            {
                for (int y = -1;y <= 1;y++)
                {
                    if ((x == 0 && y == 0) || (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1))
                        continue;

                    int checkX = cell.Datas.widthPos + x;
                    int checkY = cell.Datas.heightPos + y;

                    if (
                        checkX >= 0
                        && checkX < grid.GridWidth
                        && checkY >= 0
                        && checkY < grid.GridHeight
                    )
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
#endregion

#region DATAS_MANIPULATION
        public void LoadGridsFromJSON()
        {
            this.SavedGrids = new Dictionary<string, GridData>();

            TextAsset[] jsons = Resources.LoadAll<TextAsset>("Saves/Grids/");
            foreach (TextAsset json in jsons)
            {
                // not used but it may help the GridData deserialization to works well, so keep it
                JObject obj = JsonConvert.DeserializeObject<JObject>(json.text);
                GridData loadedData = JsonConvert.DeserializeObject<GridData>(json.text);

                this.SavedGrids.Add(json.name, loadedData);
            }
        }

        public GridData[] GetGridDatas()
        {
            GridData[] grids = new GridData[this.WorldGrids.Count];
            for (int i = 0; i < this.WorldGrids.Count; i++)
            {
                grids[i] = this.GetGridData(this.WorldGrids.Values.ElementAt(i));
            }

            return grids;
        }

        public GridData GetGridData(WorldGrid grid)
        {
            List<GridData> innerGrids = new List<GridData>();

            List<CellData> cellsData;
            Dictionary<GridPosition, Guid> savedSpawnables;
            List<StorageData> storages;

            foreach (var innerGrid in grid.InnerCombatGrids)
            {
                this._getCellsDatas(innerGrid.Value, out cellsData, out savedSpawnables, out storages);

                innerGrids.Add(
                    new GridData(
                        innerGrid.Key,
                        true,
                        innerGrid.Value.GridHeight,
                        innerGrid.Value.GridWidth,
                        innerGrid.Value.Latitude,
                        innerGrid.Value.Longitude,
                        innerGrid.Value.SelfData.Entrances,
                        cellsData,
                        savedSpawnables
                ));
            }


            this._getCellsDatas(grid, out cellsData, out savedSpawnables, out storages);

            return new GridData(
                grid.UName,
                grid.SelfData.GridLevelPath,
                false,
                grid.GridHeight,
                grid.GridWidth,
                grid.TopLeftOffset,
                cellsData,
                innerGrids,
                savedSpawnables,
                storages
            );
        }

        private bool _getCellsDatas(WorldGrid grid, out List<CellData> cellsData, out Dictionary<GridPosition, Guid> savedSpawnables, out List<StorageData> storages)
        {
            cellsData = new List<CellData>();
            savedSpawnables = new Dictionary<GridPosition, Guid>();
            storages = new List<StorageData>();

            // Look for every spawnable that should remain the same as the base save
            foreach (var spawnable in grid.SelfData.SpawnablePresets)
            {
                var corrSpawn = SettingsManager.Instance.SpawnablesPresets[spawnable.Value];

                if (corrSpawn.OverrideSave)
                {
                    savedSpawnables.Add(spawnable.Key, spawnable.Value);
                }
            }

            // Iterate over each cell to save
            for (int i = 0; i < grid.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < grid.Cells.GetLength(1); j++)
                {
                    // The ones that override save have been processed already
                    if (grid.Cells[i, j] != null && grid.Cells[i, j].AttachedInteract != null && !grid.Cells[i, j].AttachedInteract.InteractablePreset.OverrideSave)
                    {
                        savedSpawnables.Add(grid.Cells[i, j].PositionInGrid, grid.Cells[i, j].AttachedInteract.InteractablePreset.UID);

                        if (grid.Cells[i, j] != null && grid.Cells[i, j].Datas.state != CellState.Walkable)
                            cellsData.Add(grid.Cells[i, j].Datas);

                        if (grid.Cells[i, j].AttachedInteract is InteractableStorage iStorage)
                        {
                            storages.Add(iStorage.Storage.GetData());
                        }
                    }
                }
            }

            return true;
        }


        public string GetGridJson(CellData[,] cellDatas, string name)
        {
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

            return gridJson;
        }

        public void SaveGridAsJSON(WorldGrid grid)
        {
            if (grid.UName == "" && grid.UName == string.Empty)
                return;
            
            this._saveJSONFile(JsonConvert.SerializeObject(this.GetGridData(grid)), grid.UName);
        }

        public void SaveGridAsJSON(CellData[,] cellDatas, string name)
        {
            if (name == "" && name == string.Empty)
                return;

            this._saveJSONFile(this.GetGridJson(cellDatas, name), name);
        }

        public void SaveGridAsJSON(GridData grid, string uName)
        {
            if (uName == "" && uName == string.Empty)
                return;

            // We have to ignore the loops because Vector3.Normalize would fck up the serialization :)
            JsonSerializerSettings Jss = new JsonSerializerSettings();
            Jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            string gridJson = JsonConvert.SerializeObject(grid, Jss);
            this._saveJSONFile(gridJson, uName);
        }

        private void _saveJSONFile(string json, string pathName)
        {
            string path = Application.dataPath + SavesPath + pathName + ".json";

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

#region SHADERS_BITMAP

        public void GenerateShaderBitmap(
            GridData world,
            GridData innerGrid = null,
            bool editor = false
        )
        {
            float cellSize = SettingsManager.Instance.GridsPreset.CellsSize;

            foreach (Transform child in this.transform)
            {
                // TODO : still need this ?
                if (!child.TryGetComponent(out ArrowRenderer rend))
                {
#if UNITY_EDITOR
                    DestroyImmediate(child.gameObject);
#else
                    Destroy(child.gameObject);
#endif
                }
            }

            if (!Application.isPlaying)
            {
                if (this.GridShader == null)
                {
                    GameObject gridChild = null;
                    foreach (Transform child in this._gridsDataHandler)
                        if (child.name.Contains("GridDisplay"))
                            gridChild = child.gameObject;

                    if (gridChild == null)
                        this.GridShader = Instantiate(
                            SettingsManager.Instance.GridsPreset.GridShader,
                            this._gridsDataHandler
                        );
                    else

                        this.GridShader = gridChild;
                }

                this.GridShader.transform.localScale = new Vector3(
                    (float)world.GridWidth / 10f,
                    1f,
                    (float)world.GridHeight / 10f
                );
                this.GridShader.transform.position = new Vector3(
                    world.TopLeftOffset.x + (world.GridWidth * cellSize) / 2f,
                    world.TopLeftOffset.y,
                    world.TopLeftOffset.z + -(world.GridHeight * cellSize) / 2f
                );
            }

            this.BitmapTexture = new Texture2D(
                world.GridWidth,
                world.GridHeight,
                TextureFormat.ARGB32,
                false
            );

            Color[] Colors = new Color[world.GridHeight * world.GridWidth];
            for (int i = 0;i < world.GridHeight * world.GridWidth;i++)
                Colors[i] = editor ? Color.black : Color.green;

            this.BitmapTexture.SetPixels(0, 0, world.GridWidth, world.GridHeight, Colors);

            if (editor)
                this._generateShaderEditorBitmap(world);
            else
                this._generateShaderBitmap(world, innerGrid);

            this.BitmapTexture.Apply();

            UniversalRenderPipelineHelper.SetRendererFeatureActive("GridRender", true);

            if (editor)
            {
                this.BitmapShaderEditor.SetVector(
                    "_Offset",
                    new Vector2(-world.TopLeftOffset.x, -(world.TopLeftOffset.z - 1))
                );
                this.BitmapShaderEditor.SetTexture("_Texture2D", this.BitmapTexture);
            } 
            else
            {
                this.BitmapShader.SetVector(
                    "_Offset",
                    new Vector2(-world.TopLeftOffset.x, -(world.TopLeftOffset.z - 1))
                );
                this.BitmapShader.SetTexture("_Texture2D", this.BitmapTexture);
            }
        }

        private void _disableGridTexture()
        {
            UniversalRenderPipelineHelper.SetRendererFeatureActive("GridRender", false);
        }

        private void _generateShaderBitmap(GridData world, GridData innerGrid)
        {
            // We use innergrid to determine we have a parent

            if (innerGrid == null)
            {
                foreach (var item in world.CellDatas)
                {
                    this.BitmapTexture.SetPixel(
                        item.widthPos,
                        world.GridHeight - item.heightPos,
                        this._getBitmapColor(item.state)
                    );
                }
            }
            for (int y = 0;y < world.GridHeight;y++)
            {
                for (int x = 0;x < world.GridWidth;x++)
                {
                    if (innerGrid != null)
                        this.BitmapTexture.SetPixel(x, world.GridHeight - y, Color.black);
                    //else if (this.BitmapTexture.GetPixel(x, world.GridHeight - y) == Color.black)
                    //    this.BitmapTexture.SetPixel(x, world.GridHeight - y, Color.green);
                }
            }

            if (innerGrid != null)
            {
                int xOffset = innerGrid.Longitude;
                int yOffset = innerGrid.Latitude;

                this.BitmapTexture.SetPixels(
                    innerGrid.Longitude,
                    world.GridHeight - (innerGrid.Latitude - 1) - innerGrid.GridHeight,
                    innerGrid.GridWidth,
                    innerGrid.GridHeight,
                    Enumerable
                        .Repeat(Color.green, innerGrid.GridWidth * innerGrid.GridHeight)
                        .ToArray()
                );

                foreach (CellData cellData in innerGrid.CellDatas)
                {
                    this.BitmapTexture.SetPixel(
                        cellData.widthPos + xOffset,
                        world.GridHeight - (cellData.heightPos + yOffset),
                        this._getBitmapColor(cellData.state)
                    );
                }
            }
            // If this is a main grid, iterate over its CombatGrid to hide them
            else
            {
                for (int i = 0;i < world.InnerGrids.Count;i++)
                {
                    this.BitmapTexture.SetPixels(
                        world.InnerGrids[i].Longitude,
                        world.GridHeight
                            - (world.InnerGrids[i].Latitude - 1)
                            - world.InnerGrids[i].GridHeight,
                        world.InnerGrids[i].GridWidth,
                        world.InnerGrids[i].GridHeight,
                        Enumerable
                            .Repeat(
                                Color.black,
                                world.InnerGrids[i].GridWidth * world.InnerGrids[i].GridHeight
                            )
                            .ToArray()
                    );
                }
            }
        }

        public void _generateShaderEditorBitmap(GridData world)
        {
            foreach (var item in world.CellDatas)
            {
                this.BitmapTexture.SetPixel(
                    item.widthPos,
                    world.GridHeight - item.heightPos,
                    this._getBitmapEditorColor(item.state)
                );
            }

            for (int y = 0;y < world.GridHeight;y++)
            {
                for (int x = 0;x < world.GridWidth;x++)
                {
                    if (world.IsCombatGrid)
                        this.BitmapTexture.SetPixel(x, world.GridHeight - y, Color.black);
                    if (this.BitmapTexture.GetPixel(x, world.GridHeight - y) == Color.black)
                        this.BitmapTexture.SetPixel(x, world.GridHeight - y, Color.green);
                }
            }

            if(world.InnerGrids != null)
            {
                for (int i = 0; i < world.InnerGrids.Count; i++)
                {
                    int xOffset = world.InnerGrids[i].Longitude;
                    int yOffset = world.InnerGrids[i].Latitude;

                    foreach (CellData cellData in world.InnerGrids[i].CellDatas)
                    {
                        this.BitmapTexture.SetPixel(
                            cellData.widthPos + xOffset,
                            world.GridHeight - (cellData.heightPos + yOffset),
                            this._getBitmapEditorColor(cellData.state)
                        );
                    }
                }
            }
        }

        private Color _getBitmapColor(CellState state)
        {
            Color newColor = Color.black;
            switch (state)
            {
                case CellState.Blocked:
                    newColor = Color.black;
                    break;
                case CellState.EntityIn:
                    newColor = Color.green;
                    break;
                case CellState.Interactable:
                    newColor = Color.red;
                    break;
            }

            return newColor;
        }

        private Color _getBitmapEditorColor(CellState state)
        {
            Color newColor = Color.green;
            switch (state)
            {
                case CellState.Walkable:
                    newColor = Color.green;
                    break;

                case CellState.EntityIn:
                case CellState.Interactable:
                    newColor = Color.red;
                    break;

                case CellState.Blocked:
                    newColor = Color.blue;
                    break;
            }

            return newColor;
        }

        public void ChangeBitmapCell(
            GridPosition pos,
            int gridHeight,
            CellState state,
            bool editor = false
        )
        {
            this.BitmapTexture.SetPixel(
                pos.longitude,
                gridHeight - pos.latitude,
                editor ? this._getBitmapEditorColor(state) : _getBitmapColor(state)
            );
            this.BitmapTexture.Apply();
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

        public int[] GetData() { return new int[2] { latitude, longitude }; }

        public static readonly GridPosition zero = new GridPosition(0, 0);
        public static readonly GridPosition Null = new GridPosition(-1, -1);

        public int longitude { get; private set; }
        public int latitude { get; private set; }

        public override string ToString()
        {
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
