using DownBelow.GridSystem;
using DownBelow.Managers;
using mattmc3.dotmore.Collections.Generic;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "EditorGridData", menuName = "DownBelow/Editor/EditorGridData", order = 0)]
public class GridDataScriptableObject : SerializedBigDataScriptableObject<EditorGridData>
{
#if UNITY_EDITOR
    public static bool IsSubscribed = false;

    #region Comparator_values
    private int _oldHeight = -1;
    private int _oldWidth = -1;

    private Vector3 _oldPosition;
    #endregion

    public float cellsWidth => SettingsManager.Instance.GridsPreset.CellsSize;

    [SerializeField]
    private GameObject _plane;
    public GameObject PlanePrefab;

    [FoldoutGroup("README")]
    [DetailedInfoBox("READ-ME", "Click on \"DRAW\", then use the numpad to paint cells\n" +
        "   0 - Walkable\n" +
        "   1 - Blocked\n" +
        "   2 - Preset In -> Cross in inspector to delete")]
    public string HowToUse = "";


    [ValueDropdown("GetSavedGrids"), OnValueChanged("LoadSelectedGrid")]
    public string SelectedGrid;

    [Button(ButtonSizes.Large), HorizontalGroup("De_Serialization", 0.5f), BoxGroup("De_Serialization/Modifications"), GUIColor(1f, 0.9f, 0.75f)]
    public void Apply()
    {
        this.ApplyModifications();
    }

    private string PenButtonName = "START EDITING";
    [Button("$PenButtonName", ButtonSizes.Large), HorizontalGroup("De_Serialization", 0.5f), BoxGroup("De_Serialization/Modifications")]
    public void StartEditing()
    {
        if (IsSubscribed)
        {
            PenButtonName = "START EDITING";

            IsSubscribed = false;

            SceneView.duringSceneGui -= OnSceneGUI;
            SceneManager.sceneUnloaded -= _stopEditOnSceneUnload;
        }
        else
        {
            PenButtonName = "STOP EDITING";

            IsSubscribed = true;

            SceneView.duringSceneGui += OnSceneGUI;
            SceneManager.sceneUnloaded += _stopEditOnSceneUnload;
        }
    }

    private void _stopEditOnSceneUnload(Scene scene)
    {
        IsSubscribed = false;
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneManager.sceneUnloaded -= _stopEditOnSceneUnload;
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
            GridManager.Instance.SaveGridAsJSON(new GridData(name, false, height, width), name);
    }

    [Button]
    public void AddInnerGrid()
    {
        if (this.LazyLoadedData.InnerGrids == null)
            this.LazyLoadedData.InnerGrids = new List<InnerGridData>();

        this.LazyLoadedData.InnerGrids.Add(new InnerGridData());
        this.LazyLoadedData.InnerGrids[^1].GenerateGrid(2, 2, 0, 0);
    }

    private string ApplyTerrainButtonName = "Apply terrain to grid";
    [Button("$ApplyTerrainButtonName")]
    public void ApplyTerrainToGrid()
    {
        ApplyTerrainButtonName = "Getting gridTerrainApplier..";
        GridTerrainApplier gridTerrainApplier = GridManager.Instance._gridsDataHandler.GetComponent<GridTerrainApplier>();
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

            GridManager.Instance.LoadEveryEntities();

            this.GenerateGrid(newGrid.GridHeight, newGrid.GridWidth);

            foreach (CellData cellData in newGrid.CellDatas)
                this.LazyLoadedData.CellDatas[cellData.heightPos, cellData.widthPos].state = cellData.state;

            this.LazyLoadedData.InnerGrids = new List<InnerGridData>();
            if (newGrid.InnerGrids != null)
            {
                foreach (var innerGrid in newGrid.InnerGrids)
                {
                    this.LazyLoadedData.InnerGrids.Add(new InnerGridData());
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
            this._oldPosition = this.LazyLoadedData.TopLeftOffset;
        }
    }

    private OrderedDictionary<GridPosition, BaseSpawnablePreset> _setSpawnablePresets(CellData[,] refCells, Dictionary<GridPosition, Guid> refEntities)
    {
        OrderedDictionary<GridPosition, BaseSpawnablePreset> setEntities = new OrderedDictionary<GridPosition, BaseSpawnablePreset>();

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
            this._plane = GridManager.Instance._gridsDataHandler.transform.Find("Plane(Clone)")?.gameObject;
            if(this._plane == null)
            {
                this._plane = Instantiate(
                this.PlanePrefab,
                    new Vector3(
                        (this.LazyLoadedData.GridWidth * cellsWidth) / 2,
                        this.LazyLoadedData.TopLeftOffset.y,
                        -(this.LazyLoadedData.GridHeight * cellsWidth) / 2 + (cellsWidth / 2)),
                    Quaternion.identity,
                    GridManager.Instance._gridsDataHandler.transform
                );
            }
        }

        this._plane.transform.localScale = new Vector3(this.LazyLoadedData.GridWidth * (cellsWidth / 10f), 0f, this.LazyLoadedData.GridHeight * (cellsWidth / 10f));
        this._plane.transform.localPosition = new Vector3((this.LazyLoadedData.GridWidth * cellsWidth) / 2, this.LazyLoadedData.TopLeftOffset.y, -(this.LazyLoadedData.GridHeight * cellsWidth) / 2);
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
        void affectSpawnStates(CellData[,] cellDatas, OrderedDictionary<GridPosition, BaseSpawnablePreset> Spawnables)
        {
            if(Spawnables != null)
            {
                foreach (var spawnable in Spawnables)
                    cellDatas[spawnable.Key.latitude, spawnable.Key.longitude].state = spawnable.Value.AffectingState;
            }
        }

        affectSpawnStates(this.LazyLoadedData.CellDatas, this.LazyLoadedData.Spawnables);
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

            affectSpawnStates(this.LazyLoadedData.InnerGrids[i].CellDatas, this.LazyLoadedData.InnerGrids[i].Spawnables);
            for (int j = 0; j < this.LazyLoadedData.InnerGrids[i].CellDatas.GetLength(0); j++)
                for (int k = 0; k < this.LazyLoadedData.InnerGrids[i].CellDatas.GetLength(1); k++)
                    if (this.LazyLoadedData.InnerGrids[i].CellDatas[j, k].state != CellState.Walkable)
                        innerCellData.Add(this.LazyLoadedData.InnerGrids[i].CellDatas[j, k]);

            Dictionary<GridPosition, Guid> innerSpawnables = new Dictionary<GridPosition, Guid>();
            if (this.LazyLoadedData.InnerGrids[i].Spawnables != null)
                foreach (var spawnable in this.LazyLoadedData.InnerGrids[i].Spawnables)
                    innerSpawnables.Add(spawnable.Key, spawnable.Value != null ? spawnable.Value.UID : Guid.Empty);

            GridData innerData = new GridData(
                this.SelectedGrid + "_" + i,
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
            this.SelectedGrid,
            false,
            this.LazyLoadedData.GridHeight,
            this.LazyLoadedData.GridWidth,
            this.LazyLoadedData.TopLeftOffset,
            cellData,
            innerGridsData,
            interactableSpawns
        );
    }

    private bool PenTypeChoosed = false;
    private CellState PenType = CellState.Walkable;

    /// <summary>
    /// launches the behaviors from Inputs in editor.
    /// </summary>
    public void OnSceneGUI(SceneView sceneView)
    {
        if (GridManager.Instance == null || SettingsManager.Instance == null)
            return;

        this.DrawGUIDatas();
        
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
        if (Event.current.type != EventType.KeyDown && Event.current.type != EventType.MouseMove)
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
            case KeyCode.F:
                this.FindDictionaryKey(hit.point);
                break;
        }
    }

    public void FindDictionaryKey(Vector3 WorldPosition)
    {
        GridPosition refPos = this.GetGridIndexFromWorld(WorldPosition);

        if (GridUtility.GetIncludingSubGrid(this.LazyLoadedData.InnerGrids, refPos, out InnerGridData includingGrid))
        {
            refPos = new GridPosition(refPos.longitude - includingGrid.Longitude, refPos.latitude - includingGrid.Latitude);
            if (includingGrid.Spawnables.ContainsKey(refPos))
                Debug.Log(this.LazyLoadedData.Spawnables[refPos]);
        }
        else
        {
            if (this.LazyLoadedData.Spawnables.ContainsKey(refPos))
                Debug.Log(this.LazyLoadedData.Spawnables[refPos]);
        }
    }

    public void DrawGUIDatas()
    {
        if (this.LazyLoadedData.CellDatas == null)
            return;

        float cellsWidth = SettingsManager.Instance.GridsPreset.CellsSize;
        Vector3 cellBounds = new Vector3(cellsWidth - cellsWidth / 15f, cellsWidth / 6f, cellsWidth - cellsWidth / 15f);

        for (int i = 0; i < this.LazyLoadedData.InnerGrids.Count; i++)
        {
            Gizmos.color = Color.black;

            Vector3 topLeft = new Vector3(this.LazyLoadedData.TopLeftOffset.x + this.LazyLoadedData.InnerGrids[i].Longitude * cellsWidth, this.LazyLoadedData.TopLeftOffset.y, this.LazyLoadedData.InnerGrids[i].Latitude * cellsWidth - this.LazyLoadedData.TopLeftOffset.z);
            Vector3 botRight = new Vector3(this.LazyLoadedData.TopLeftOffset.x + (this.LazyLoadedData.InnerGrids[i].Longitude + this.LazyLoadedData.InnerGrids[i].GridWidth) * cellsWidth, this.LazyLoadedData.TopLeftOffset.y, (this.LazyLoadedData.InnerGrids[i].Latitude + this.LazyLoadedData.InnerGrids[i].GridHeight) * cellsWidth - this.LazyLoadedData.TopLeftOffset.z);

            float midLong = this.LazyLoadedData.InnerGrids[i].GridWidth * cellsWidth / 2f;
            float midLat = this.LazyLoadedData.InnerGrids[i].GridHeight * cellsWidth / 2f;

            Vector3 left = new Vector3(topLeft.x, cellBounds.y / 3f + topLeft.y, -(topLeft.z + midLat));
            Vector3 right = new Vector3(botRight.x, cellBounds.y / 3f + botRight.y, -(botRight.z - midLat));
            Vector3 top = new Vector3(topLeft.x + midLong, cellBounds.y / 3f + topLeft.y, -topLeft.z);
            Vector3 bot = new Vector3(botRight.x - midLong, cellBounds.y / 3f + botRight.y, -botRight.z);
            Gizmos.color = Color.cyan;
            Handles.DrawWireCube(top, new Vector3(this.LazyLoadedData.InnerGrids[i].GridWidth, 0.05f, 0.05f));
            Handles.DrawWireCube(bot, new Vector3(this.LazyLoadedData.InnerGrids[i].GridWidth, 0.05f, 0.05f));
            Handles.DrawWireCube(left, new Vector3(0.05f, 0.05f, this.LazyLoadedData.InnerGrids[i].GridHeight));
            Handles.DrawWireCube(right, new Vector3(0.05f, 0.05f, this.LazyLoadedData.InnerGrids[i].GridHeight));
        }

        if (this.LazyLoadedData.Spawnables != null)
        {
            float height = this.LazyLoadedData.TopLeftOffset.y + (cellBounds.y / 2f + 0.15f);

            int counter = 0;
            foreach (var entity in this.LazyLoadedData.Spawnables)
            {
                drawString(
                    counter.ToString() + " - " + (entity.Value == null ? "NONE" : entity.Value.UName), 
                    new Vector3(entity.Key.longitude * cellsWidth + this.LazyLoadedData.TopLeftOffset.x + (cellsWidth / 2), height, -entity.Key.latitude * cellsWidth + this.LazyLoadedData.TopLeftOffset.z - (cellsWidth / 2)));
                counter++;
            }
            foreach (var grid in this.LazyLoadedData.InnerGrids)
            {
                counter = 0;
                if (grid.Spawnables != null)
                {
                    foreach (var entity in grid.Spawnables)
                    {
                        drawString(
                            counter.ToString() + " - " + (entity.Value == null ? "NONE" : entity.Value.UName),
                            new Vector3((entity.Key.longitude + grid.Longitude) * cellsWidth + this.LazyLoadedData.TopLeftOffset.x + (cellsWidth / 2), height, -(entity.Key.latitude + grid.Latitude) * cellsWidth + this.LazyLoadedData.TopLeftOffset.z - (cellsWidth / 2)));
                        counter++;
                    }
                }
            }
        }
    }

    

    #region draw_string_editor
    static public void drawString(string text, Vector3 worldPos, float oX = 0, float oY = 0, Color? colour = null)
    {


        UnityEditor.Handles.BeginGUI();

        var restoreColor = Color.white;

        if (colour.HasValue) GUI.color = colour.Value;
        var view = SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
        {
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
            return;
        }

        UnityEditor.Handles.Label(TransformByPixel(worldPos, oX, oY), text);

        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();

    }

    static Vector3 TransformByPixel(Vector3 position, float x, float y)
    {
        return TransformByPixel(position, new Vector3(x, y));
    }

    static Vector3 TransformByPixel(Vector3 position, Vector3 translateBy)
    {
        Camera cam = UnityEditor.SceneView.currentDrawingSceneView.camera;
        if (cam)
            return cam.ScreenToWorldPoint(cam.WorldToScreenPoint(position) + translateBy);
        else
            return position;
    }
    #endregion

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
        if (GridUtility.GetIncludingSubGrid(this.LazyLoadedData.InnerGrids, pos, out InnerGridData includingGrid))
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
            this.LazyLoadedData.Spawnables = new OrderedDictionary<GridPosition, BaseSpawnablePreset>();

        GridPosition pos = this.GetGridIndexFromWorld(WorldPosition);

        if (GridUtility.GetIncludingSubGrid(this.LazyLoadedData.InnerGrids, pos, out InnerGridData includingGrid))
        {
            if (includingGrid.Spawnables == null)
                includingGrid.Spawnables = new OrderedDictionary<GridPosition, BaseSpawnablePreset>();

            this.AllocateSpawnable(includingGrid.Spawnables, includingGrid.CellDatas, new GridPosition(pos.longitude - includingGrid.Longitude, pos.latitude - includingGrid.Latitude));
        }
        else
        {
            this.AllocateSpawnable(this.LazyLoadedData.Spawnables, this.LazyLoadedData.CellDatas, pos);
        }

        GridManager.Instance.ChangeBitmapCell(pos, this.LazyLoadedData.GridHeight, CellState.EntityIn, true);
    }



    public void AllocateSpawnable(OrderedDictionary<GridPosition, BaseSpawnablePreset> spawnablesRef, CellData[,] cellsRef, GridPosition pos)
    {
        // Then we remove it
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
#endif
}
