using Jemkont.GridSystem;
using Jemkont.Managers;
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
                SubgridVars g = this._oldInnerGrids[i];
                if (this._innerGridChanged(g, this._target.InnerGrids[i]))
                {
                    EditorUtility.SetDirty(this._target.gameObject);
                    bool resizeDown = false;
                    if (this._target.InnerGrids[i].GridWidth < g.oldWidth || this._target.InnerGrids[i].GridHeight < g.oldHeight)
                        resizeDown = true;

                    this._target.InnerGrids[i].ResizeGrid(this.ResizeArrayTwo(this._target.InnerGrids[i].CellDatas, this._target.InnerGrids[i].GridHeight, this._target.InnerGrids[i].GridWidth, resizeDown));

                    this._setInnerGrid(ref g, this._target.InnerGrids[i]);
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

            this._target.ResizeGrid(this.ResizeArrayTwo(_target.CellDatas, this._target.GridHeight, this._target.GridWidth, resizeDown));

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

    public void OnSceneGUI()
    {
        if (Event.current.shift && Event.current.type == EventType.MouseDown)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 6))
            {
                EditorUtility.SetDirty(this._target.gameObject);
                GridPosition pos = this._target.GetGridIndexFromWorld(hit.point);

                CellState currState = this._target.CellDatas[pos.longitude, pos.latitude].state;
                this._target.CellDatas[pos.longitude, pos.latitude].state = (currState == CellState.Blocked) ? CellState.Walkable : CellState.Blocked;
            }
        }
        else if(Event.current.shift && Event.current.keyCode == KeyCode.A && Event.current.type == EventType.KeyDown)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 6))
            {
                if (this._target.EntitySpawns == null)
                    this._target.EntitySpawns = new Dictionary<GridPosition, EntitySpawn>();

                GridPosition pos = this._target.GetGridIndexFromWorld(hit.point);
                if(!this._target.EntitySpawns.ContainsKey(pos))
                {
                    this._target.CellDatas[pos.longitude, pos.latitude].state = CellState.EntityIn;
                    this._target.EntitySpawns.Add(pos, null);
                }
                else
                {
                    this._target.CellDatas[pos.longitude, pos.latitude].state = CellState.Walkable;
                    this._target.EntitySpawns.Remove(pos);
                }
            }
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
