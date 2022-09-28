using Jemkont.GridSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CombatGrid))]
public class GridEditor : Editor
{
    private CombatGrid _target;

    private int _oldHeight;
    private int _oldWidth;

    private Vector3 _oldPosition;

    public void OnEnable()
    {
        if (this._target == null)
            this._target = (CombatGrid)this.target;

        this._oldPosition = this._target.transform.position;

        this._target.Init(this._target.GridHeight, this._target.GridWidth);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(this._oldPosition != this._target.transform.position)
        {
            this._oldPosition = this._target.transform.position;
            if(this._target.Cells != null && this._target.Cells.Length > 0)
                this._target.TopLeftOffset = this._target.Cells[0, 0].WorldPosition;
        }

        if(this._target.GridWidth != this._oldWidth || this._target.GridHeight != this._oldHeight)
        {
            this._target.ResizeGrid(this.ResizeArrayTwo(_target.Cells, this._target.GridHeight, this._target.GridWidth));

            this._oldWidth = this._target.GridWidth;
            this._oldHeight = this._target.GridHeight;
        }
    }

    protected T[,] ResizeArray<T>(T[,] original, int x, int y)
    {
        T[,] newArray = new T[x, y];
        int minX = Math.Min(original.GetLength(0), newArray.GetLength(0));
        int minY = Math.Min(original.GetLength(1), newArray.GetLength(1));

        for (int i = 0; i < minY; ++i)
            Array.Copy(original, i * original.GetLength(0), newArray, i * newArray.GetLength(0), minX);

        return newArray;
    }

    protected T[,] ResizeArrayTwo<T>(T[,] original, int newCoNum, int newRoNum)
    {
        var newArray = new T[newCoNum, newRoNum];
        int columnCount = original.GetLength(1);
        int columnCount2 = newRoNum;
        int columns = original.GetUpperBound(0);
        for (int co = 0; co <= columns; co++)
            Array.Copy(original, co * columnCount, newArray, co * columnCount2, columnCount);
        return newArray;
    }
}
