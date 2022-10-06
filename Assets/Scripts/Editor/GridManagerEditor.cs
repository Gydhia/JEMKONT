using Jemkont.GridSystem;
using Jemkont.Managers;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CombatGrid))]
public class GridEditor : Editor
{
    private CombatGrid _target;

    private int _oldHeight = -1;
    private int _oldWidth = -1;

    private Vector3 _oldPosition;

    public void OnEnable()
    {
        if (this._target == null)
            this._target = (CombatGrid)this.target;

        this._oldWidth = this._target.GridWidth;
        this._oldHeight = this._target.GridHeight;
        this._oldPosition = this._target.transform.position;    
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUI.Button(GUILayoutUtility.GetRect(0, int.MaxValue, 20, 20), "Init Grid"))
        {
            EditorUtility.SetDirty(this._target.gameObject);
            this._target.DestroyChildren();

            this._target.GenerateGrid(this._target.GridHeight, this._target.GridWidth, true);
            this._oldWidth = this._target.GridWidth;
            this._oldHeight = this._target.GridHeight;
            this._target.ResizePlane();
        }

        if (GUI.Button(GUILayoutUtility.GetRect(0, int.MaxValue, 20, 20), "Save Grid"))
        {
            GridManager.Instance.SaveGridAsJSON(this._target);
        }

        if (GUI.Button(GUILayoutUtility.GetRect(0, int.MaxValue, 20, 20), "Load Grids"))
        {
            GridManager.Instance.LoadGridsFromJSON();
        }

        if (this._target.Cells == null || this._target.Cells.Length == 0 || this._target.GridWidth <= 0 || this._target.GridHeight <= 0)
            return;

        if (this._target.GridWidth != this._oldWidth || this._target.GridHeight != this._oldHeight)
        {
            this._target.ResizePlane();
        }

        if (this._oldPosition != this._target.transform.position)
        {
            this._oldPosition = this._target.transform.position;
            if (this._target.Cells != null && this._target.Cells.Length > 0)
                this._target.TopLeftOffset = this._target.transform.position;
        }

        if (this._target.GridWidth != this._oldWidth || this._target.GridHeight != this._oldHeight)
        {
            EditorUtility.SetDirty(this._target.gameObject);
            bool resizeDown = false;
            if (this._target.GridWidth < this._oldWidth || this._target.GridHeight < this._oldHeight)
                resizeDown = true;

            this._target.ResizeGrid(this.ResizeArrayTwo(_target.Cells, this._target.GridHeight, this._target.GridWidth, resizeDown), true);

            this._oldWidth = this._target.GridWidth;
            this._oldHeight = this._target.GridHeight;
        }
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

                CellState currState = this._target.Cells[pos.x, pos.y].Datas.State;
                this._target.Cells[pos.x, pos.y].ChangeCellState(currState == CellState.Blocked ?
                    CellState.Walkable : CellState.Blocked
                );
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
