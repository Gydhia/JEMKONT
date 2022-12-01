using Jemkont.GridSystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubgridPlaceholder
{
    [HideInInspector]
    public CellData[,] CellDatas;

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

    public void ResizeGrid(CellData[,] newCells)
    {
        int oldHeight = this.CellDatas.GetLength(0);
        int oldWidth = this.CellDatas.GetLength(1);

        int newHeight = newCells.GetLength(0);
        int newWidth = newCells.GetLength(1);

        if (newHeight < oldHeight || newWidth < oldWidth)
            this.CellDatas = newCells;
        else if (newCells.Length != this.CellDatas.Length)
        {
            this.CellDatas = newCells;

            for (int i = 0; i < newHeight; i++)
                for (int j = 0; j < newWidth; j++)
                    if (this.CellDatas[i, j] == null)
                        this.CellDatas[i, j] = new CellData(i, j, CellState.Walkable);
        }
    }

}
