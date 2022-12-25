using DownBelow.GridSystem;
using DownBelow.Managers;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPlaceholder : SerializedMonoBehaviour
{
    [SerializeField] 
    private GameObject _planePrefab;

    public Vector3 TopLeftOffset;


#if UNITY_EDITOR
    #region GRID_ODIN_INSPECTOR

    [HideInInspector]
    public CellData[,] CellDatas;

    public List<SubgridPlaceholder> InnerGrids;

    public Dictionary<GridPosition, BaseSpawnablePreset> Spawnables;

    [ValueDropdown("GetSavedGrids"), OnValueChanged("LoadSelectedGrid")]
    public string SelectedGrid;

    [Button, PropertyOrder(2)]
    public void CreateNewGrid(string name)
    {
        if (!GridManager.Instance.SavedGrids.ContainsKey(name))
            GridManager.Instance.SaveGridAsJSON(new GridData(false, 2, 2), name);
    }

    [Button]
    public void AddInnerGrid()
    {
        if(this.InnerGrids == null)
            this.InnerGrids = new List<SubgridPlaceholder>();

        this.InnerGrids.Add(new SubgridPlaceholder());
        this.InnerGrids[^1].GenerateGrid(2, 2, 0, 0);
    }

    private string PenButtonName = "Enable Pen";
    [Button("$PenButtonName")]
    public void ActivatePen()
    {
        IsPainting = !IsPainting;
        PenButtonName = IsPainting? "Disable Pen" : "Enable Pen";
    }

    private string ApplyTerrainButtonName = "Apply terrain to grid";
    [Button("$ApplyTerrainButtonName")]
    public void ApplyTerrainToGrid()
    {
        ApplyTerrainButtonName = "Getting gridTerrainApplier..";
        GridTerrainApplier gridTerrainApplier = gameObject.GetComponent<GridTerrainApplier>();
        if (gridTerrainApplier == null)
        {
            ApplyTerrainButtonName = "No gridTerrainApplier found";
            return;
        }
        ApplyTerrainButtonName = "Applying terrain to grid..";
        gridTerrainApplier.ApplyTerrainToGrid(this.CellDatas, this.InnerGrids);
        ApplyTerrainButtonName = "Apply terrain to grid";
    }

    private IEnumerable<string> GetSavedGrids()
    {
        GridManager.Instance.LoadGridsFromJSON();
        return GridManager.Instance.SavedGrids.Keys;
    }

    public void LoadSelectedGrid()
    {
        if(GridManager.Instance.SavedGrids.TryGetValue(this.SelectedGrid, out GridData newGrid))
        {
            this.TopLeftOffset = newGrid.TopLeftOffset;
            this.transform.position = this.TopLeftOffset;
            if (GridManager.Instance.SpawnablesPresets == null)
                GridManager.Instance.LoadEveryEntities();

            this.GenerateGrid(newGrid.GridHeight, newGrid.GridWidth);

            foreach (CellData cellData in newGrid.CellDatas)
                this.CellDatas[cellData.heightPos, cellData.widthPos].state = cellData.state;

            this.InnerGrids = new List<SubgridPlaceholder>();
            if (newGrid.InnerGrids != null)
            {
                foreach (var innerGrid in newGrid.InnerGrids)
                {
                    this.InnerGrids.Add(new SubgridPlaceholder());
                    this.InnerGrids[^1].GenerateGrid(innerGrid.GridHeight, innerGrid.GridWidth, innerGrid.Longitude, innerGrid.Latitude);

                    foreach (CellData cellData in innerGrid.CellDatas)
                        this.InnerGrids[^1].CellDatas[cellData.heightPos, cellData.widthPos].state = cellData.state;

                    this.InnerGrids[^1].Spawnables = this._setSpawnablePresets(this.InnerGrids[^1].CellDatas, innerGrid.SpawnablePresets);
                }
            }

            if (newGrid.SpawnablePresets != null)
                this.Spawnables = this._setSpawnablePresets(this.CellDatas, newGrid.SpawnablePresets);
            else
                this.Spawnables.Clear();

            GridManager.Instance.GenerateShaderBitmap(newGrid, null, true);
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

    #endregion
    public bool ToLoad = false;
    public bool IsCombatGrid = false;

    public int GridHeight = 8;
    public int GridWidth = 5;

    [HideInInspector]
    public bool IsPainting;
    

    private float cellsWidth => SettingsManager.Instance.GridsPreset.CellsSize;

    [HideInInspector]
    public GameObject Plane;

    public void GenerateGrid(int height, int width)
    {
        this.GridHeight = height;
        this.GridWidth = width;

        this.CellDatas = new CellData[height, width];

        // Generate the grid with new cells
        for (int i = 0; i < this.CellDatas.GetLength(0); i++)
        {
            for (int j = 0; j < this.CellDatas.GetLength(1); j++)
            {
                this.CellDatas[i, j] = new CellData(i, j, CellState.Walkable);
            }
        }
    }

    public void ResizePlane()
    {
        if (this.Plane == null)
            this.Plane = Instantiate(
                this._planePrefab,
                new Vector3((this.GridWidth * cellsWidth) / 2, 0f, -(this.GridHeight * cellsWidth) / 2 + (cellsWidth / 2)), Quaternion.identity, this.transform
            );

        this.Plane.transform.localScale = new Vector3(this.GridWidth * (cellsWidth / 10f), 0f, this.GridHeight * (cellsWidth / 10f));
        this.Plane.transform.localPosition = new Vector3((this.GridWidth * cellsWidth) / 2, 0f, -(this.GridHeight * cellsWidth) / 2);
    }

    public GridPosition GetGridIndexFromWorld(Vector3 worldPos)
    {
        float cellSize = SettingsManager.Instance.GridsPreset.CellsSize;
        int height = (int)(Mathf.Abs(Mathf.Abs(worldPos.z) - Mathf.Abs(this.TopLeftOffset.z)) / cellSize);
        int width = (int)(Mathf.Abs(Mathf.Abs(worldPos.x) - Mathf.Abs(this.TopLeftOffset.x)) / cellSize);

        return new GridPosition(width, height);
    }

    public GridData GetGridData()
    {
        // Main grid datas
        List<CellData> cellData = new List<CellData>();
        for (int i = 0; i < this.CellDatas.GetLength(0); i++)
            for (int j = 0; j < this.CellDatas.GetLength(1); j++)
                if(this.CellDatas[i, j].state != CellState.Walkable)
                    cellData.Add(this.CellDatas[i, j]);

        // Inner grids data
        List<GridData> innerGridsData = new List<GridData>();
        for (int i = 0; i < this.InnerGrids.Count; i++)
        {
            List<CellData> innerCellData = new List<CellData>();

            for (int j = 0; j < this.InnerGrids[i].CellDatas.GetLength(0); j++)
                for (int k = 0; k < this.InnerGrids[i].CellDatas.GetLength(1); k++)
                    if (this.InnerGrids[i].CellDatas[j, k].state != CellState.Walkable)
                        innerCellData.Add(this.InnerGrids[i].CellDatas[j, k]);

            Dictionary<GridPosition, Guid> innerSpawnables = new Dictionary<GridPosition, Guid>();
            if (this.InnerGrids[i].Spawnables != null)
                foreach (var spawnable in this.InnerGrids[i].Spawnables)
                    innerSpawnables.Add(spawnable.Key, spawnable.Value != null ? spawnable.Value.UID : Guid.Empty);

            // TODO: ADD entities
            GridData innerData = new GridData(
                true,
                this.InnerGrids[i].GridHeight,
                this.InnerGrids[i].GridWidth,
                this.InnerGrids[i].Longitude,
                this.InnerGrids[i].Latitude,
                innerCellData,
                innerSpawnables
                );

            innerGridsData.Add(innerData);
        }

        // All the spawnables presets.
        Dictionary<GridPosition, Guid> interactableSpawns = new Dictionary<GridPosition, Guid>();
        if (this.Spawnables != null)
            foreach (var interactableSpawn in this.Spawnables)
                interactableSpawns.Add(interactableSpawn.Key, interactableSpawn.Value != null ? interactableSpawn.Value.UID : Guid.Empty);

        // /!\ By default, the grid containing InnerGrids is not a combatgrid
        return new GridData(
            false,
            this.GridHeight,
            this.GridWidth,
            this.TopLeftOffset,
            this.ToLoad,
            cellData,
            innerGridsData,
            interactableSpawns
        );
    }

    private void OnDrawGizmos()
    {
        if (this.CellDatas == null)
            return;

        float cellsWidth = SettingsManager.Instance.GridsPreset.CellsSize;
        Vector3 cellBounds = new Vector3(cellsWidth - cellsWidth/15f, cellsWidth/6f, cellsWidth - cellsWidth/15f);

        for (int i = 0; i < this.InnerGrids.Count; i++)
        {
            Gizmos.color = Color.black;

            Vector3 topLeft = new Vector3(this.TopLeftOffset.x + this.InnerGrids[i].Longitude * cellsWidth, TopLeftOffset.y, this.InnerGrids[i].Latitude * cellsWidth - this.TopLeftOffset.z);
            Vector3 botRight = new Vector3(this.TopLeftOffset.x + (this.InnerGrids[i].Longitude + this.InnerGrids[i].GridWidth) * cellsWidth, TopLeftOffset.y, (this.InnerGrids[i].Latitude + this.InnerGrids[i].GridHeight) * cellsWidth - this.TopLeftOffset.z);

            float midLong = this.InnerGrids[i].GridWidth * cellsWidth / 2f;
            float midLat = this.InnerGrids[i].GridHeight * cellsWidth / 2f;

            Vector3 left = new Vector3(topLeft.x, cellBounds.y / 3f + topLeft.y, -(topLeft.z + midLat));
            Vector3 right = new Vector3(botRight.x, cellBounds.y / 3f + botRight.y, -(botRight.z - midLat));
            Vector3 top = new Vector3(topLeft.x + midLong, cellBounds.y / 3f + topLeft.y, -topLeft.z);
            Vector3 bot = new Vector3(botRight.x - midLong, cellBounds.y / 3f + botRight.y, -botRight.z);
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(top, new Vector3(this.InnerGrids[i].GridWidth, 0.05f, 0.05f));
            Gizmos.DrawCube(bot, new Vector3(this.InnerGrids[i].GridWidth, 0.05f, 0.05f));
            Gizmos.DrawCube(left, new Vector3(0.05f, 0.05f, this.InnerGrids[i].GridHeight));
            Gizmos.DrawCube(right, new Vector3(0.05f, 0.05f, this.InnerGrids[i].GridHeight));
        }

        if(this.Spawnables != null)
        {
            int counter = 0;
            foreach (var entity in this.Spawnables)
            {
                drawString(counter.ToString(), new Vector3(entity.Key.longitude * cellsWidth + TopLeftOffset.x + (cellsWidth / 2), cellBounds.y / 2f + 0.15f, -entity.Key.latitude * cellsWidth + TopLeftOffset.z - (cellsWidth / 2)));
                counter++;
            }
            foreach (var grid in this.InnerGrids)
            {
                counter = 0;
                if (grid.Spawnables != null)
                {
                    foreach (var entity in grid.Spawnables)
                    {
                        drawString(counter.ToString(), new Vector3((entity.Key.longitude + grid.Longitude) * cellsWidth + TopLeftOffset.x + (cellsWidth / 2), cellBounds.y / 2f + 0.15f, -(entity.Key.latitude + grid.Latitude) * cellsWidth + TopLeftOffset.z - (cellsWidth / 2)));
                        counter++;
                    }
                }
            }
        }
    }

    #region draw_string_editor
    static public void drawString(string text, Vector3 worldPos, float oX = 0, float oY = 0, Color? colour = null)
    {

#if UNITY_EDITOR
        UnityEditor.Handles.BeginGUI();

        var restoreColor = GUI.color;

        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
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
#endif
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
#endif
}
