using DownBelow.GridSystem;
using DownBelow.Managers;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridPlaceholder))]
public class GridEditor : OdinEditor
{
    private GridPlaceholder _target;

    private int _oldHeight = -1;
    private int _oldWidth = -1;

    private Vector3 _oldPosition;

    private bool PenTypeChoosed = false;
    private CellState PenType = CellState.Walkable;

    private class SubgridVars
    {
        public SubgridVars(int height, int width, int longitude, int latitude)
        {
            this.oldHeight = height;
            this.oldWidth = width;
            this.oldLongitude = longitude;
            this.oldLatitude = latitude;
        }
        public int oldHeight;
        public int oldWidth;
        public int oldLongitude;
        public int oldLatitude;
    }
    private List<SubgridVars> _oldInnerGrids; 

    public void OnEnable()
    {
        if (this._target == null)
            this._target = (GridPlaceholder)this.target;

        this._oldWidth = this._target.GridWidth;
        this._oldHeight = this._target.GridHeight;
        this._oldPosition = this._target.transform.position;    
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.BeginHorizontal();
        if (GUI.Button(GUILayoutUtility.GetRect(0, int.MaxValue, 20, 20), "Init"))
        {
            EditorUtility.SetDirty(this._target.gameObject);

            this._target.GenerateGrid(this._target.GridHeight, this._target.GridWidth);
            this._oldWidth = this._target.GridWidth;
            this._oldHeight = this._target.GridHeight;
            this._target.ResizePlane();
        }
        if (GUI.Button(GUILayoutUtility.GetRect(0, int.MaxValue, 20, 20), "Save"))
        {
            GridManager.Instance.SaveGridAsJSON(this._target.GetGridData(), this._target.SelectedGrid);
        }
        if(GUI.Button(GUILayoutUtility.GetRect(0, int.MaxValue, 20, 20), "Reload"))
        {
            this._target.LoadSelectedGrid();
        }
        GUILayout.EndHorizontal();

        if (this._target.CellDatas == null || this._target.CellDatas.Length == 0 || this._target.GridWidth <= 0 || this._target.GridHeight <= 0)
            return;

        // Actualize the subgrids
        if(this._target.InnerGrids != null)
        {
            if (this._oldInnerGrids == null)
                this._oldInnerGrids = new List<SubgridVars>();

            if (this._oldInnerGrids.Count != this._target.InnerGrids.Count)
            {
                if (this._oldInnerGrids.Count < this._target.InnerGrids.Count)
                    this._oldInnerGrids.Add(new SubgridVars(this._target.InnerGrids[^1].GridHeight, this._target.InnerGrids[^1].GridWidth, this._target.InnerGrids[^1].Longitude, this._target.InnerGrids[^1].Latitude));
                else
                    this._oldInnerGrids.RemoveAt(this._oldInnerGrids.Count - 1);
            }

            for (int i = 0; i < this._target.InnerGrids.Count; i++)
            {
                SubgridPlaceholder currGrid = this._target.InnerGrids[i];
                if (currGrid.GridWidth <= 0 || currGrid.GridHeight <= 0)
                    continue;

                SubgridVars g = this._oldInnerGrids[i];
                if (this._innerGridChanged(g, currGrid))
                {
                    EditorUtility.SetDirty(this._target.gameObject);
                    bool resizeDown = false;
                    if (currGrid.GridWidth < g.oldWidth || currGrid.GridHeight < g.oldHeight)
                        resizeDown = true;

                    GridUtility.ResizeGrid(
                        ref this._target.InnerGrids[i].CellDatas,
                        this.ResizeArrayTwo(currGrid.CellDatas, currGrid.GridHeight, currGrid.GridWidth, resizeDown));

                    this._setInnerGrid(ref g, currGrid);
                }
            }
        }
        
     
        if (this._target.GridWidth != this._oldWidth || this._target.GridHeight != this._oldHeight)
        {
            this._target.ResizePlane();
        }

        if (this._oldPosition != this._target.transform.position)
        {
            this._oldPosition = this._target.transform.position;
            if (this._target.CellDatas != null && this._target.CellDatas.Length > 0)
                this._target.TopLeftOffset = this._target.transform.position;
        }

        if (this._target.GridWidth != this._oldWidth || this._target.GridHeight != this._oldHeight)
        {
            EditorUtility.SetDirty(this._target.gameObject);
            bool resizeDown = false;
            if (this._target.GridWidth < this._oldWidth || this._target.GridHeight < this._oldHeight)
                resizeDown = true;

            GridUtility.ResizeGrid(ref this._target.CellDatas,
                this.ResizeArrayTwo(_target.CellDatas, this._target.GridHeight, this._target.GridWidth, resizeDown));

            this._oldWidth = this._target.GridWidth;
            this._oldHeight = this._target.GridHeight;
        }
    }

    private bool _innerGridChanged(SubgridVars oldSubgrid, SubgridPlaceholder newSubgrid)
    {
        return oldSubgrid.oldHeight != newSubgrid.GridHeight ||
                oldSubgrid.oldWidth != newSubgrid.GridWidth ||
                oldSubgrid.oldLongitude != newSubgrid.Longitude ||
                oldSubgrid.oldLatitude != newSubgrid.Latitude;
    }

    private void _setInnerGrid(ref SubgridVars oldSubgrid, SubgridPlaceholder newSubgrid)
    {
        oldSubgrid.oldHeight = newSubgrid.GridHeight;
        oldSubgrid.oldWidth = newSubgrid.GridWidth;
        oldSubgrid.oldLongitude = newSubgrid.Longitude;
        oldSubgrid.oldLatitude = newSubgrid.Latitude;
    }

    /// <summary>
    /// launches the behaviors from Inputs in editor.
    /// </summary>
    public void OnSceneGUI()
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
        if (!this._target.IsPainting || Event.current.type != EventType.KeyDown && Event.current.type != EventType.MouseMove)
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
                this.MakeEntity(hit.point);;
                break;
        }
    }


    public void MakeBlock(Vector3 WorldPosition)
    {
        EditorUtility.SetDirty(this._target.gameObject);

        GridPosition pos = this._target.GetGridIndexFromWorld(WorldPosition);
        // Longitude = x, latitude = y. Array is [height, width]

        CellData[,] refDatas = this.GetRefDatasOfGRid(pos, out GridPosition positionInCurrentGrid);
        pos = positionInCurrentGrid;
        CellState currState = refDatas[pos.latitude, pos.longitude].state;
        if (PenType == currState || currState == CellState.EntityIn)
        {
            return;
        }
        refDatas[pos.latitude, pos.longitude].state = CellState.Blocked;
    }

    public void MakeWalkable(Vector3 WorldPosition)
    {
        EditorUtility.SetDirty(this._target.gameObject);

        GridPosition pos = this._target.GetGridIndexFromWorld(WorldPosition);
        // Longitude = x, latitude = y. Array is [height, width]

        CellData[,] refDatas = this.GetRefDatasOfGRid(pos, out GridPosition positionInCurrentGrid);
        pos = positionInCurrentGrid;
        CellState currState = refDatas[pos.latitude, pos.longitude].state;
        if (currState == CellState.EntityIn)
        {
            MakeEntity(WorldPosition);
        }
        if (PenType == currState)
        {
            return;
        }
        refDatas[pos.latitude, pos.longitude].state = CellState.Walkable;
    }

    public CellData[,] GetRefDatasOfGRid(GridPosition pos, out GridPosition positionInCurrentGrid)
    {
        CellData[,] refDatas;
        //Either in a sub grid or in the original grid : apply position and ref datas variables.
        if (GridUtility.GetIncludingSubGrid(this._target.InnerGrids, pos, out SubgridPlaceholder includingGrid))
        {
            refDatas = includingGrid.CellDatas;
            positionInCurrentGrid = new GridPosition(pos.longitude - includingGrid.Longitude, pos.latitude - includingGrid.Latitude);
        }
        else
        {
            refDatas = this._target.CellDatas;
            positionInCurrentGrid = pos;
        }
        return refDatas;
    }

    /// <Summary>
    /// Will Check if the cell has an Entity or not and delete it or place one.
    /// </Summary>
    public void MakeEntity(Vector3 WorldPosition)
    {
        if (this._target.Spawnables == null)
            this._target.Spawnables = new Dictionary<GridPosition, BaseSpawnablePreset>();

        GridPosition pos = this._target.GetGridIndexFromWorld(WorldPosition);

        if (GridUtility.GetIncludingSubGrid(this._target.InnerGrids, pos, out SubgridPlaceholder includingGrid))
        {
            if (includingGrid.Spawnables == null)
                includingGrid.Spawnables = new Dictionary<GridPosition, BaseSpawnablePreset>();

            this.AllocateSpawnable(includingGrid.Spawnables, includingGrid.CellDatas, new GridPosition(pos.longitude - includingGrid.Longitude, pos.latitude - includingGrid.Latitude));
        }
        else
        {
            this.AllocateSpawnable(this._target.Spawnables, this._target.CellDatas, pos);
        }
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
        else
        {
            cellsRef[pos.latitude, pos.longitude].state = CellState.Walkable;
            spawnablesRef.Remove(pos);
        }
    }

    protected T[,] ResizeArrayTwo<T>(T[,] original, int newCoNum, int newRoNum, bool resizeDown)
    {
        var newArray = new T[newCoNum, newRoNum];
        int columnCount = original.GetLength(1);
        int columnCount2 = newRoNum;
        int columns = original.GetUpperBound(0);
        if (resizeDown)
        {
            // Since 2D arrays are only one line of elements, we need to offset the difference if we go from 5 to 3 or else
            if(newRoNum < columnCount)
                for (int co = 0; co <= columns; co++)
                    Array.Copy(original, co * newRoNum + (co * (columnCount - newRoNum)), newArray, co * columnCount2, newRoNum);
            // For columns we shouldn't try to copy in the new array more than we have
            else
                for (int co = 0; co < newCoNum; co++)
                    Array.Copy(original, co * columnCount, newArray, co * columnCount2, columnCount);
        }
        else
            for (int co = 0; co <= columns; co++)
                Array.Copy(original, co * columnCount, newArray, co * columnCount2, columnCount);
        
        return newArray;
    }
}
