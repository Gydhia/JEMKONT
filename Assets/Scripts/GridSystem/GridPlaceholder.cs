using Jemkont.GridSystem;
using Jemkont.Managers;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPlaceholder : SerializedMonoBehaviour
{
    #region GRID_ODIN_INSPECTOR
    #if UNITY_EDITOR
    [HideInInspector]
    public CellData[,] CellDatas;

    public Dictionary<GridPosition, EntitySpawn> EntitySpawns;

    [SerializeField, HideInInspector]
    private string _selectedGrid;
    [ValueDropdown("GetSavedGrids"), OnValueChanged("LoadSelectedGrid")]
    public string SelectedGrid;

    [Button, PropertyOrder(2)]
    public void CreateNewGrid(string name)
    {
        if (!GridManager.Instance.SavedGrids.ContainsKey(name))
            GridManager.Instance.SaveGridAsJSON(new GridData(false, 2, 2), name);
    }

    private IEnumerable<string> GetSavedGrids()
    {
        GridManager.Instance.LoadGridsFromJSON();
        return GridManager.Instance.SavedGrids.Keys;
    }

    public void LoadSelectedGrid()
    {
        if(GridManager.Instance.SavedGrids.TryGetValue(SelectedGrid, out GridData newGrid))
        {
            this.GenerateGrid(newGrid.GridHeight, newGrid.GridWidth);
            foreach (CellData cellData in newGrid.CellDatas)
                this.CellDatas[cellData.heightPos, cellData.widthPos].state = cellData.state;
            if (newGrid.EntitiesSpawns != null)
            {
                this.EntitySpawns = new Dictionary<GridPosition, EntitySpawn>();
                foreach (var entitySpawn in newGrid.EntitiesSpawns)
                {
                    if(CombatManager.Instance.EntitiesSpawnsSO.ContainsKey(entitySpawn.Value))
                        this.EntitySpawns.Add(entitySpawn.Key, CombatManager.Instance.EntitiesSpawnsSO[entitySpawn.Value]);
                    else
                        this.EntitySpawns.Add(entitySpawn.Key, null);

                    this.CellDatas[entitySpawn.Key.longitude, entitySpawn.Key.latitude].state = CellState.EntityIn;
                }
            }
            else
                this.EntitySpawns.Clear();

            this._selectedGrid = SelectedGrid;
        }
    }
#endif
    #endregion
    public bool IsCombatGrid = false;

    public int GridHeight = 8;
    public int GridWidth = 5;

    private float cellsWidth => SettingsManager.Instance.GridsPreset.CellsSize;

    public Vector3 TopLeftOffset;

    [HideInInspector]
    public GameObject Plane;

    private void Start()
    {
        if(GridManager.Instance != null)
        {
            GridManager.Instance.GenerateGrid(this.TopLeftOffset, GridManager.Instance.SavedGrids[this._selectedGrid]);

            Destroy(this.gameObject);
        }
    }

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

        this.TopLeftOffset = this.transform.position;
    }

    public void ResizePlane()
    {
        if (this.Plane == null)
            this.Plane = Instantiate(
                GridManager.Instance.Plane,
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

        return new GridPosition(height, width);
    }

    public void ResizeGrid(CellData[,] newCells)
    {
        int oldHeight = this.CellDatas.GetLength(0);
        int oldWidth = this.CellDatas.GetLength(1);

        int newHeight = newCells.GetLength(0);
        int newWidth = newCells.GetLength(1);

        if(newHeight < oldHeight || newWidth < oldWidth)
            this.CellDatas = newCells;
        else if(newCells.Length != this.CellDatas.Length)
        {
            this.CellDatas = newCells;

            for (int i = 0; i < newHeight; i++)
                for (int j = 0; j < newWidth; j++)
                    if(this.CellDatas[i, j] == null)
                        this.CellDatas[i, j] = new CellData(i, j, CellState.Walkable);    
        }
    }

    public GridData GetGridData()
    {
        List<CellData> cellData = new List<CellData>();
        for (int i = 0; i < this.CellDatas.GetLength(0); i++)
            for (int j = 0; j < this.CellDatas.GetLength(1); j++)
                if(this.CellDatas[i, j].state != CellState.Walkable)
                    cellData.Add(this.CellDatas[i, j]);

        Dictionary<GridPosition, Guid> entitiesSpawns = new Dictionary<GridPosition, Guid>();
        if(this.EntitySpawns != null)
            foreach (var entitySpawn in this.EntitySpawns)
                entitiesSpawns.Add(entitySpawn.Key, entitySpawn.Value != null ? entitySpawn.Value.UID : Guid.Empty) ;

        return new GridData(
            this.IsCombatGrid,
            this.GridHeight,
            this.GridWidth,
            cellData,
            entitiesSpawns
        );
    }

    private void OnDrawGizmos()
    {
        if (this.CellDatas == null)
            return;

        Color red = new Color(Color.red.r, Color.red.g, Color.red.b, 0.4f);
        Color white = new Color(Color.white.r, Color.white.g, Color.white.b, 0.4f);
        Color blue = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.4f);

        float cellsWidth = SettingsManager.Instance.GridsPreset.CellsSize;
        Vector3 cellBounds = new Vector3(cellsWidth - 1f, 2f, cellsWidth - 1f);

        for (int i = 0; i < this.GridHeight; i++)
        {
            for (int j = 0; j < this.GridWidth; j++)
            {
                if (this.CellDatas[i, j] == null)
                    continue;

                if (i == 0 && j == 0)
                    Gizmos.color = Color.yellow;
                else if (this.CellDatas[i, j].state == CellState.Walkable)
                    Gizmos.color = white;
                else if (this.CellDatas[i, j].state == CellState.Blocked)
                    Gizmos.color = red;
                else
                    Gizmos.color = blue;

                Vector3 pos = new Vector3(j * cellsWidth + TopLeftOffset.x + (cellsWidth / 2), 0.1f, -i * cellsWidth + TopLeftOffset.z - (cellsWidth / 2));

                Gizmos.DrawCube(pos, cellBounds);
            }
        }

        if(this.EntitySpawns != null)
        {
            int counter = 0;
            foreach (var entity in this.EntitySpawns)
            {
                drawString(counter.ToString(), new Vector3(entity.Key.latitude * cellsWidth + TopLeftOffset.x + (cellsWidth / 2), 2f, -entity.Key.longitude * cellsWidth + TopLeftOffset.z - (cellsWidth / 2)));
                counter++;
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
}
