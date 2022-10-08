using Jemkont.GridSystem;
using Jemkont.Managers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPlaceholder : SerializedMonoBehaviour
{
    #region GRID_ODIN_INSPECTOR
    #if UNITY_EDITOR
    [HideInInspector]
    public CellData[,] CellDatas;

    [SerializeField, HideInInspector]
    private string _selectedGrid;
    [ValueDropdown("GetSavedGrids"), OnValueChanged("LoadSelectedGrid")]
    public string SelectedGrid;

    [Button, PropertyOrder(2)]
    public void CreateNewGrid(string name)
    {
        if(!GridManager.Instance.SavedGrids.ContainsKey(name))
            GridManager.Instance.SaveGridAsJSON(new GridData(2, 2), name);
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
            this._selectedGrid = SelectedGrid;
        }
    }
#endif
    #endregion

    public int GridHeight = 8;
    public int GridWidth = 5;

    private float widthOffset => SettingsManager.Instance.GridsPreset.CellsSize / 2f;
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

    private void OnDrawGizmos()
    {
        if (this.CellDatas == null)
            return;

        Color red = new Color(Color.red.r, Color.red.g, Color.red.b, 0.4f);
        Color white = new Color(Color.white.r, Color.white.g, Color.white.b, 0.4f);
        Color blue = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.4f);

        float cellsWidth = SettingsManager.Instance.GridsPreset.CellsSize;
        Vector3 cellBounds = new Vector3(cellsWidth - 1f, 2f, cellsWidth - 1f);

        float widthOffset = (cellsWidth / 2);

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
    }
}
