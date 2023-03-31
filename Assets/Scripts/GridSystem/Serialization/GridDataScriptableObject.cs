using DownBelow.GridSystem;
using DownBelow.Managers;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "EditorGridData", menuName = "DownBelow/Editor/EditorGridData", order = 0)]
public class GridDataScriptableObject : SerializedBigDataScriptableObject<EditorGridData>
{
    #region Comparator_values
    private int _oldHeight = -1;
    private int _oldWidth = -1;

    private Vector3 _oldPosition;
    #endregion

    public float cellsWidth => SettingsManager.Instance.GridsPreset.CellsSize;

    [HideInInspector]
    public bool IsPainting = false;

    [SerializeField]
    private GameObject _plane;
    public GameObject PlanePrefab;

    [ValueDropdown("GetSavedGrids"), OnValueChanged("LoadSelectedGrid")]
    public string SelectedGrid;

    [Button(ButtonSizes.Large), HorizontalGroup("De_Serialization", 0.5f), BoxGroup("De_Serialization/Modifications"), GUIColor(1f, 0.9f, 0.75f)]
    public void Apply()
    {
        this.ApplyModifications();
    }

    private string PenButtonName = "DRAW";
    [Button("$PenButtonName", ButtonSizes.Large), HorizontalGroup("De_Serialization", 0.5f), BoxGroup("De_Serialization/Modifications")]
    public void ActivatePen()
    {
        this.IsPainting = !this.IsPainting;
        this.PenButtonName = this.IsPainting ? "STOP" : "DRAW";
    }

    [Button(ButtonSizes.Large), HorizontalGroup("De_Serialization", 0.5f), BoxGroup("De_Serialization/De_Serialization"), GUIColor(0.8f, 0.5f, 0.3f)]
    public void ReloadGrid()
    {
        this.LoadSelectedGrid();
    }

    [Button(ButtonSizes.Large), HorizontalGroup("De_Serialization", 0.5f), BoxGroup("De_Serialization/De_Serialization"), GUIColor(0.1f, 1f, 0.1f)]
    public void SaveGrid()
    {
        GridManager.Instance.SaveGridAsJSON(this.GetGridData(), this.SelectedGrid);
        this.ReloadGrid();
    }


    [Button(ButtonSizes.Large, ButtonStyle.Box, Expanded = true), PropertyOrder(2)]
    public void CreateNewGrid(string name, int width = 4, int height = 4)
    {
        if (!GridManager.Instance.SavedGrids.ContainsKey(name))
            GridManager.Instance.SaveGridAsJSON(new GridData(false, height, width), name);
    }

    [Button]
    public void AddInnerGrid()
    {
        if (this.LazyLoadedData.InnerGrids == null)
            this.LazyLoadedData.InnerGrids = new List<SubgridPlaceholder>();

        this.LazyLoadedData.InnerGrids.Add(new SubgridPlaceholder());
        this.LazyLoadedData.InnerGrids[^1].GenerateGrid(2, 2, 0, 0);
    }

    private string ApplyTerrainButtonName = "Apply terrain to grid";
    [Button("$ApplyTerrainButtonName")]
    public void ApplyTerrainToGrid()
    {
        ApplyTerrainButtonName = "Getting gridTerrainApplier..";
        GridTerrainApplier gridTerrainApplier = GridManager.Instance.gameObject.GetComponent<GridTerrainApplier>();
        if (gridTerrainApplier == null)
        {
            ApplyTerrainButtonName = "No gridTerrainApplier found";
            return;
        }
        ApplyTerrainButtonName = "Applying terrain to grid..";
        gridTerrainApplier.ApplyTerrainToGrid(this.LazyLoadedData.CellDatas, this.LazyLoadedData.InnerGrids);
        ApplyTerrainButtonName = "Apply terrain to grid";
    }

    private IEnumerable<string> GetSavedGrids()
    {
        GridManager.Instance.LoadGridsFromJSON();
        return GridManager.Instance.SavedGrids.Keys;
    }

    public void LoadSelectedGrid()
    {
        if (GridManager.Instance.SavedGrids.TryGetValue(this.SelectedGrid, out GridData newGrid))
        {
            this.LazyLoadedData.TopLeftOffset = newGrid.TopLeftOffset;
            GridManager.Instance._gridsDataHandler.position = this.LazyLoadedData.TopLeftOffset;

            GridManager.Instance.LoadEveryEntities();

            this.GenerateGrid(newGrid.GridHeight, newGrid.GridWidth);

            foreach (CellData cellData in newGrid.CellDatas)
                this.LazyLoadedData.CellDatas[cellData.heightPos, cellData.widthPos].state = cellData.state;

            this.LazyLoadedData.InnerGrids = new List<SubgridPlaceholder>();
            if (newGrid.InnerGrids != null)
            {
                foreach (var innerGrid in newGrid.InnerGrids)
                {
                    this.LazyLoadedData.InnerGrids.Add(new SubgridPlaceholder());
                    this.LazyLoadedData.InnerGrids[^1].GenerateGrid(innerGrid.GridHeight, innerGrid.GridWidth, innerGrid.Longitude, innerGrid.Latitude);

                    foreach (CellData cellData in innerGrid.CellDatas)
                        this.LazyLoadedData.InnerGrids[^1].CellDatas[cellData.heightPos, cellData.widthPos].state = cellData.state;

                    this.LazyLoadedData.InnerGrids[^1].Spawnables = this._setSpawnablePresets(this.LazyLoadedData.InnerGrids[^1].CellDatas, innerGrid.SpawnablePresets);
                }
            }

            if (newGrid.SpawnablePresets != null)
                this.LazyLoadedData.Spawnables = this._setSpawnablePresets(this.LazyLoadedData.CellDatas, newGrid.SpawnablePresets);
            else
                this.LazyLoadedData.Spawnables.Clear();

            GridManager.Instance.GenerateShaderBitmap(newGrid, null, true);
            this.ResizePlane();

            this._oldHeight = this.LazyLoadedData.GridHeight;
            this._oldWidth = this.LazyLoadedData.GridWidth;
            this._oldPosition= this.LazyLoadedData.TopLeftOffset;
        }
    }

    private Dictionary<GridPosition, BaseSpawnablePreset> _setSpawnablePresets(CellData[,] refCells, Dictionary<GridPosition, Guid> refEntities)
    {
        Dictionary<GridPosition, BaseSpawnablePreset> setEntities = new Dictionary<GridPosition, BaseSpawnablePreset>();

        foreach (var spawnable in refEntities)
        {
            if (GridManager.Instance.SpawnablesPresets.ContainsKey(spawnable.Value))
            {
                setEntities.Add(spawnable.Key, GridManager.Instance.SpawnablesPresets[spawnable.Value]);
                refCells[spawnable.Key.latitude, spawnable.Key.longitude].state = GridManager.Instance.SpawnablesPresets[spawnable.Value].AffectingState;
            }
            else
            {
                Debug.LogError("Couldn't find a place spawnable because it's null.");
            }
        }

        return setEntities;
    }

    public void GenerateGrid(int height, int width)
    {
        this.LazyLoadedData.GridHeight = height;
        this.LazyLoadedData.GridWidth = width;

        this.LazyLoadedData.CellDatas = new CellData[height, width];

        // Generate the grid with new cells
        for (int i = 0; i < this.LazyLoadedData.CellDatas.GetLength(0); i++)
        {
            for (int j = 0; j < this.LazyLoadedData.CellDatas.GetLength(1); j++)
            {
                this.LazyLoadedData.CellDatas[i, j] = new CellData(i, j, CellState.Walkable);
            }
        }

        this.Save(this.LazyLoadedData);
    }

    public void ResizePlane()
    {
        if (this._plane == null)
        {
            this._plane = Instantiate(
                this.PlanePrefab,
                new Vector3(
                    (this.LazyLoadedData.GridWidth * cellsWidth) / 2,
                    -0.01f,
                    -(this.LazyLoadedData.GridHeight * cellsWidth) / 2 + (cellsWidth / 2)),
                Quaternion.identity,
                GridManager.Instance._gridsDataHandler.transform
            );
        }

        GridManager.Instance._gridsDataHandler.position = this.LazyLoadedData.TopLeftOffset;
        this._plane.transform.localScale = new Vector3(this.LazyLoadedData.GridWidth * (cellsWidth / 10f), 0f, this.LazyLoadedData.GridHeight * (cellsWidth / 10f));
        this._plane.transform.localPosition = new Vector3((this.LazyLoadedData.GridWidth * cellsWidth) / 2, 0f, -(this.LazyLoadedData.GridHeight * cellsWidth) / 2);
    }

    public GridPosition GetGridIndexFromWorld(Vector3 worldPos)
    {
        float cellSize = SettingsManager.Instance.GridsPreset.CellsSize;
        int height = (int)(Mathf.Abs(Mathf.Abs(worldPos.z) + Mathf.Abs(this.LazyLoadedData.TopLeftOffset.z)) / cellSize);
        int width = (int)(Mathf.Abs(Mathf.Abs(worldPos.x) - Mathf.Abs(this.LazyLoadedData.TopLeftOffset.x)) / cellSize);

        return new GridPosition(width, height);
    }

    public GridData GetGridData()
    {
        // Main grid datas
        List<CellData> cellData = new List<CellData>();
        for (int i = 0; i < this.LazyLoadedData.CellDatas.GetLength(0); i++)
            for (int j = 0; j < this.LazyLoadedData.CellDatas.GetLength(1); j++)
                if (this.LazyLoadedData.CellDatas[i, j].state != CellState.Walkable)
                    cellData.Add(this.LazyLoadedData.CellDatas[i, j]);

        // Inner grids data
        List<GridData> innerGridsData = new List<GridData>();
        for (int i = 0; i < this.LazyLoadedData.InnerGrids.Count; i++)
        {
            List<CellData> innerCellData = new List<CellData>();

            for (int j = 0; j < this.LazyLoadedData.InnerGrids[i].CellDatas.GetLength(0); j++)
                for (int k = 0; k < this.LazyLoadedData.InnerGrids[i].CellDatas.GetLength(1); k++)
                    if (this.LazyLoadedData.InnerGrids[i].CellDatas[j, k].state != CellState.Walkable)
                        innerCellData.Add(this.LazyLoadedData.InnerGrids[i].CellDatas[j, k]);

            Dictionary<GridPosition, Guid> innerSpawnables = new Dictionary<GridPosition, Guid>();
            if (this.LazyLoadedData.InnerGrids[i].Spawnables != null)
                foreach (var spawnable in this.LazyLoadedData.InnerGrids[i].Spawnables)
                    innerSpawnables.Add(spawnable.Key, spawnable.Value != null ? spawnable.Value.UID : Guid.Empty);

            GridData innerData = new GridData(
                true,
                this.LazyLoadedData.InnerGrids[i].GridHeight,
                this.LazyLoadedData.InnerGrids[i].GridWidth,
                this.LazyLoadedData.InnerGrids[i].Longitude,
                this.LazyLoadedData.InnerGrids[i].Latitude,
                innerCellData,
                innerSpawnables
                );

            innerGridsData.Add(innerData);
        }

        // All the spawnables presets.
        Dictionary<GridPosition, Guid> interactableSpawns = new Dictionary<GridPosition, Guid>();
        if (this.LazyLoadedData.Spawnables != null)
            foreach (var interactableSpawn in this.LazyLoadedData.Spawnables)
                interactableSpawns.Add(interactableSpawn.Key, interactableSpawn.Value != null ? interactableSpawn.Value.UID : Guid.Empty);

        // /!\ By default, the grid containing InnerGrids is not a combatgrid
        return new GridData(
            false,
            this.LazyLoadedData.GridHeight,
            this.LazyLoadedData.GridWidth,
            this.LazyLoadedData.TopLeftOffset,
            this.LazyLoadedData.ToLoad,
            cellData,
            innerGridsData,
            interactableSpawns
        );
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private bool PenTypeChoosed = false;
    private CellState PenType = CellState.Walkable;

    /// <summary>
    /// launches the behaviors from Inputs in editor.
    /// </summary>
    public void OnSceneGUI(SceneView sceneView)
    {
        /// check if the mouse is on a cell
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 6))
        {
            return;
        }
        if (Event.current.type == EventType.KeyUp)
        {
            PenTypeChoosed = false;
        }
        /// depending on the current input call block or make entity
        if (!this.IsPainting || Event.current.type != EventType.KeyDown && Event.current.type != EventType.MouseMove)
        {
            return;
        }
        switch (Event.current.keyCode)
        {
            case KeyCode.Keypad0:
                PenType = CellState.Walkable;
                this.MakeWalkable(hit.point);
                break;
            case KeyCode.Keypad1:
                PenType = CellState.Blocked;
                this.MakeBlock(hit.point);
                break;
            case KeyCode.Keypad2:
                PenType = CellState.EntityIn;
                this.MakeEntity(hit.point); ;
                break;
        }
    }


    public void MakeBlock(Vector3 WorldPosition)
    {

        GridPosition refPos = this.GetGridIndexFromWorld(WorldPosition);
        // Longitude = x, latitude = y. Array is [height, width]

        CellData[,] refDatas = this.GetRefDatasOfGRid(refPos, out GridPosition positionInCurrentGrid);
        GridPosition processPose = positionInCurrentGrid;
        CellState currState = refDatas[processPose.latitude, processPose.longitude].state;
        if (PenType == currState || currState == CellState.EntityIn)
        {
            return;
        }
        refDatas[processPose.latitude, processPose.longitude].state = CellState.Blocked;
        GridManager.Instance.ChangeBitmapCell(refPos, this.LazyLoadedData.GridHeight, CellState.Blocked, true);
    }

    public void MakeWalkable(Vector3 WorldPosition)
    {
        GridPosition pos = this.GetGridIndexFromWorld(WorldPosition);
        // Longitude = x, latitude = y. Array is [height, width]

        CellData[,] refDatas = this.GetRefDatasOfGRid(pos, out GridPosition positionInCurrentGrid);
        GridPosition processPose = positionInCurrentGrid;
        CellState currState = refDatas[processPose.latitude, processPose.longitude].state;
        if (currState == CellState.EntityIn)
        {
            MakeEntity(WorldPosition);
        }
        if (PenType == currState)
        {
            return;
        }
        refDatas[processPose.latitude, processPose.longitude].state = CellState.Walkable;
        GridManager.Instance.ChangeBitmapCell(pos, this.LazyLoadedData.GridHeight, CellState.Walkable, true);

    }

    public CellData[,] GetRefDatasOfGRid(GridPosition pos, out GridPosition positionInCurrentGrid)
    {
        CellData[,] refDatas;
        //Either in a sub grid or in the original grid : apply position and ref datas variables.
        if (GridUtility.GetIncludingSubGrid(this.LazyLoadedData.InnerGrids, pos, out SubgridPlaceholder includingGrid))
        {
            refDatas = includingGrid.CellDatas;
            positionInCurrentGrid = new GridPosition(pos.longitude - includingGrid.Longitude, pos.latitude - includingGrid.Latitude);
        }
        else
        {
            refDatas = this.LazyLoadedData.CellDatas;
            positionInCurrentGrid = pos;
        }
        return refDatas;
    }

    /// <Summary>
    /// Will Check if the cell has an Entity or not and delete it or place one.
    /// </Summary>
    public void MakeEntity(Vector3 WorldPosition)
    {
        if (this.LazyLoadedData.Spawnables == null)
            this.LazyLoadedData.Spawnables = new Dictionary<GridPosition, BaseSpawnablePreset>();

        GridPosition pos = this.GetGridIndexFromWorld(WorldPosition);

        if (GridUtility.GetIncludingSubGrid(this.LazyLoadedData.InnerGrids, pos, out SubgridPlaceholder includingGrid))
        {
            if (includingGrid.Spawnables == null)
                includingGrid.Spawnables = new Dictionary<GridPosition, BaseSpawnablePreset>();

            this.AllocateSpawnable(includingGrid.Spawnables, includingGrid.CellDatas, new GridPosition(pos.longitude - includingGrid.Longitude, pos.latitude - includingGrid.Latitude));
        }
        else
        {
            this.AllocateSpawnable(this.LazyLoadedData.Spawnables, this.LazyLoadedData.CellDatas, pos);
        }
        GridManager.Instance.ChangeBitmapCell(pos, this.LazyLoadedData.GridHeight, CellState.EntityIn, true);

    }

    public void AllocateSpawnable(Dictionary<GridPosition, BaseSpawnablePreset> spawnablesRef, CellData[,] cellsRef, GridPosition pos)
    {
        if (cellsRef[pos.latitude, pos.longitude].state == PenType)
        {
            return;
        }
        if (!spawnablesRef.ContainsKey(pos))
        {
            cellsRef[pos.latitude, pos.longitude].state = CellState.EntityIn;
            spawnablesRef.Add(pos, null);
        }
    }


    public void ApplyModifications()
    {
        if (this._oldPosition != this.LazyLoadedData.TopLeftOffset)
        {
            this._oldPosition = this.LazyLoadedData.TopLeftOffset;
        }

        if (this.LazyLoadedData.GridWidth != this._oldWidth || this.LazyLoadedData.GridHeight != this._oldHeight)
        {
            bool resizeDown = false;
            if (this.LazyLoadedData.GridWidth < this._oldWidth || this.LazyLoadedData.GridHeight < this._oldHeight)
                resizeDown = true;

            GridUtility.ResizeGrid(ref this.LazyLoadedData.CellDatas,
                ArrayHelper.ResizeArrayTwo(this.LazyLoadedData.CellDatas, this.LazyLoadedData.GridHeight, this.LazyLoadedData.GridWidth, resizeDown));

            this._oldWidth = this.LazyLoadedData.GridWidth;
            this._oldHeight = this.LazyLoadedData.GridHeight;
        }

        GridManager.Instance.GenerateShaderBitmap(this.GetGridData(), null, true);

        this._oldHeight = this.LazyLoadedData.GridHeight;
        this._oldWidth = this.LazyLoadedData.GridWidth;
        this._oldPosition = this.LazyLoadedData.TopLeftOffset;

        this.Save(this.LazyLoadedData);
        
        this.ResizePlane();
    }
    
}
