using DownBelow.GridSystem;
using DownBelow.Managers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
using mattmc3.dotmore.Collections.Generic;
using System;

[Serializable]
public class InnerGridData
{
    [HideInInspector]
    public CellData[,] CellDatas;

    public OrderedDictionary<GridPosition, BaseSpawnablePreset> Spawnables;

    public int GridHeight;
    public int GridWidth;

    public int Longitude;
    public int Latitude;

    public void GenerateGrid(int height, int width, int longitude, int latitude)
    {
        this.GridHeight = height;
        this.GridWidth = width;
        this.Longitude = longitude;
        this.Latitude = latitude;

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
}
