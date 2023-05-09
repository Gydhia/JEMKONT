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

namespace DownBelow.Managers
{
    public class GridManager : _baseManager<GridManager>
    {
        public static readonly string SavesPath = "/Resources/Saves/Grids/";

        #region Assets_reference
        public Cell CellPrefab;

        [SerializeField]
        public WorldGrid CombatGridPrefab;

        [SerializeField]
        private Transform _objectsHandler;

        [SerializeField]
        public Transform _gridsDataHandler;

#endregion

        public bool InCombat = false;

#region Datas
        public Dictionary<string, GridData> SavedGrids;

        public Dictionary<Guid, BaseSpawnablePreset> SpawnablesPresets;
        public Dictionary<Guid, ItemPreset> ItemsPresets;

#endregion

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

            this.LoadEveryEntities();

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
            // Load the Grids and Entities SO
            
            Destroy(this._gridsDataHandler.gameObject);

            // TODO : Plug it in a scriptable instead of hardcoding it like that
            var startingGrid = refGameDataContainer.Data.grids_data.SingleOrDefault(g => g.GridName == "FarmLand");

            this.CreateGrid(startingGrid, startingGrid.GridName);
            this.GenerateShaderBitmap(startingGrid);
        }

        public void CreateGrid(GridData gridData, string gridId)
        {
            WorldGrid newGrid = Instantiate(
                this.CombatGridPrefab,
                gridData.TopLeftOffset,
                Quaternion.identity,
                this._objectsHandler
            );

            newGrid.UName = gridId;
            newGrid.Init(gridData);

            this.WorldGrids.Add(newGrid.UName, newGrid);

            this.MainWorldGrid = this.WorldGrids.Values.First();
        }

        public void ProcessCellClickUp(CellEventData data)
        {
            if (this.LastHoveredCell == null)
                return;

            PlayerBehavior selfPlayer = GameManager.Instance.SelfPlayer;

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
                string otherGrid = string.Empty;

                if (InputManager.Instance.LastInteractable != null)
                {
                    Cell cell = InputManager.Instance.LastInteractable.RefCell;
                    closestCell = GridUtility.GetClosestCellToShape(
                        selfPlayer.CurrentGrid,
                        cell.Datas.heightPos,
                        cell.Datas.widthPos,
                        1,
                        1,
                        selfPlayer.EntityCell.PositionInGrid
                    );
                    selfPlayer.NextInteract = InputManager.Instance.LastInteractable;

                    EntityAction[] actions = new EntityAction[2];

                    // Buff the movement action if we're too far away
                    if (!GetCombatNeighbours(selfPlayer.EntityCell, selfPlayer.EntityCell.RefGrid).Contains(cell))
                        actions[0] = new MovementAction(selfPlayer, closestCell);

                    // Then buff the interact/gather action to process after the movement
                    actions[1] =
                        cell.AttachedInteract is InteractableResource
                            ? new GatheringAction(selfPlayer, cell)
                            : new InteractAction(selfPlayer, cell);

                    NetworkManager.Instance.EntityAskToBuffActions(actions);

                    return;
                } else if (this.LastHoveredCell.HasItem(out var item))
                {
                    //If we have a dropped item on the cell.
                    EntityAction[] actions = new EntityAction[2];
                    actions[0] = new MovementAction(selfPlayer, LastHoveredCell);
                    actions[1] = new PickupItemAction(selfPlayer, this.LastHoveredCell);
                    NetworkManager.Instance.EntityAskToBuffActions(actions);
                    return;
                } else
                {
                    if (selfPlayer.CurrentGrid != this.LastHoveredCell.RefGrid)
                    {
                        closestCell = GridUtility.GetClosestCellToShape(
                            selfPlayer.CurrentGrid,
                            this.LastHoveredCell.RefGrid as CombatGrid,
                            selfPlayer.EntityCell.PositionInGrid
                        );
                        otherGrid = this.LastHoveredCell.RefGrid.UName;
                    }
                }

                if (closestCell != null)
                {
                    if (string.IsNullOrEmpty(otherGrid))
                        NetworkManager.Instance.EntityAskToBuffAction(
                            new MovementAction(selfPlayer, closestCell)
                        );
                    else
                        NetworkManager.Instance.EntityAskToBuffActions(
                            new EntityAction[2]
                            {
                                new MovementAction(selfPlayer, closestCell),
                                new EnterGridAction(selfPlayer, closestCell, otherGrid)
                            }
                        );
                }
            }
        }

        public void ProcessCellClickUp_Combat(PlayerBehavior selfPlayer)
        {
            if (selfPlayer.IsPlayingEntity)
            {
                // When not grabbing card
                if (UI.DraggableCard.SelectedCard == null)
                {
                    if (selfPlayer.IsAutoAttacking)
                    {
                        if (LastHoveredCell.Datas.state == CellState.EntityIn)
                        {
                            if (!selfPlayer.isInAttackRange(LastHoveredCell))
                                selfPlayer.IsAutoAttacking = false;
                            else
                                selfPlayer.AutoAttack(LastHoveredCell);
                        }
                    } else if (
                          this.LastHoveredCell.Datas.state == CellState.Walkable
                          && this._possiblePath.Contains(this.LastHoveredCell)
                      )
                    {
                        Debug.Log("CREATED COMBAT MOVEMENT ACTION");
                        NetworkManager.Instance.EntityAskToBuffAction(
                            new CombatMovementAction(selfPlayer, this.LastHoveredCell)
                        );
                    }
                }
            }
        }

        public void ProcessCellClickDown(CellEventData data)
        {
            if (this.LastHoveredCell == null)
                return;

            PlayerBehavior selfPlayer = GameManager.Instance.SelfPlayer;

            if (selfPlayer.CurrentGrid.IsCombatGrid)
                this.ProcessCellClickDown_Combat(selfPlayer);
            else
                this.ProcessCellClickDown_Exploration(selfPlayer);
        }

        public void ProcessCellClickDown_Exploration(PlayerBehavior selfPlayer) { }

        public void ProcessCellClickDown_Combat(PlayerBehavior selfPlayer)
        {
            if (
                CombatManager.Instance.CurrentPlayingEntity == selfPlayer
                && selfPlayer.EntityCell == LastHoveredCell
                && selfPlayer.CanAutoAttack
            )
                selfPlayer.IsAutoAttacking = true;
        }

        public void ProcessNewCellHovered(CellEventData Data)
        {
            if (!GameManager.GameStarted)
                return;

            PlayerBehavior selfPlayer = GameManager.Instance.SelfPlayer;
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
            if (Data.Entity == GameManager.Instance.SelfPlayer)
            {
                if (Data.Entity.CurrentGrid.IsCombatGrid)
                    this.GenerateShaderBitmap(
                        ((CombatGrid)Data.Entity.CurrentGrid).ParentGrid.SelfData,
                        Data.Entity.CurrentGrid.SelfData
                    );
                else
                    this.GenerateShaderBitmap(Data.Entity.CurrentGrid.SelfData);

                foreach (PlayerBehavior player in GameManager.Instance.Players.Values)
                {
                    if (player.CurrentGrid != Data.Entity.CurrentGrid)
                        player.gameObject.SetActive(false);
                    else
                        player.gameObject.SetActive(true);
                }
            }
            // Make that stranger disappear / appear according to our grid
            else
            {
                // IMPORTANT : Remember that Disabled GameObjects would disable their scripts to.
                // Only MasterClient have to handle combat Datas, we'll do as it follows :
                // When joining a grid already in combat, load the values from MasterClient if we're not, else handle everything.
                if (GameManager.Instance.SelfPlayer.CurrentGrid != Data.Entity.CurrentGrid)
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
                if (currentCell == targetCell || Range > 0 && IsInRange(currentCell.PositionInGrid, targetCell.PositionInGrid, Range))
                {
                    if (Range > 0)
                        targetCell = currentCell;
                    // Once done, get the correct path
                    finalPath = this.RetracePath(startCell, targetCell);
                    // If the last cell of the path isn't walkable, stop right before
                    if (finalPath.Count > 0 && finalPath[^1].Datas.state == CellState.EntityIn)
                    {
                        finalPath.RemoveAt(finalPath.Count - 1);
                    }

                    return finalPath;
                }

                List<Cell> actNeighbours = entity.CurrentGrid.IsCombatGrid
                    ? GetCombatNeighbours(currentCell, entity.CurrentGrid)
                    : GetNormalNeighbours(currentCell, entity.CurrentGrid);
                foreach (Cell neighbour in actNeighbours)
                {
                    if (CellState.NonWalkable.HasFlag(neighbour.Datas.state) && directPath == false || closedSet.Contains(neighbour))
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

        public void SaveGridAsJSON(WorldGrid grid)
        {
            if (grid.UName == "" && grid.UName == string.Empty)
                return;

            List<CellData> savedCells = new List<CellData>();

            // Get the non walkable cells only
            for (int i = 0;i < grid.Cells.GetLength(0);i++)
                for (int j = 0;j < grid.Cells.GetLength(1);j++)
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
            for (int i = 0;i < cellDatas.GetLength(0);i++)
                for (int j = 0;j < cellDatas.GetLength(1);j++)
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
            if (editor)
            {
                this.BitmapShaderEditor.SetVector(
                    "_Offset",
                    new Vector2(-world.TopLeftOffset.x, -(world.TopLeftOffset.z - 1))
                );
                this.BitmapShaderEditor.SetTexture("_Texture2D", this.BitmapTexture);
            } else
            {
                this.BitmapShader.SetVector(
                    "_Offset",
                    new Vector2(-world.TopLeftOffset.x, -(world.TopLeftOffset.z - 1))
                );
                this.BitmapShader.SetTexture("_Texture2D", this.BitmapTexture);
            }
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

        public static readonly GridPosition zero = new GridPosition(0, 0);

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
