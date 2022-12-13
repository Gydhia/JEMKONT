using Jemkont.GridSystem;
using Jemkont.Managers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubgridPlaceholder
{
    [HideInInspector]
    public CellData[,] CellDatas;

    public Dictionary<GridPosition, BaseSpawnablePreset> Spawnables;

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
