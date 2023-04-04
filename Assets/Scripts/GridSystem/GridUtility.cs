using DownBelow.Entity;
using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DownBelow.GridSystem
{
    public static class GridUtility
    {
        public static void ResizeGrid(ref CellData[,] oldCells, CellData[,] newCells)
        {
            int oldHeight = oldCells.GetLength(0);
            int oldWidth = oldCells.GetLength(1);

            int newHeight = newCells.GetLength(0);
            int newWidth = newCells.GetLength(1);

            if (newHeight < oldHeight || newWidth < oldWidth)
                oldCells = newCells;
            else if (newCells.Length != oldCells.Length)
            {
                oldCells = newCells;

                for (int i = 0; i < newHeight; i++)
                    for (int j = 0; j < newWidth; j++)
                        if (oldCells[i, j] == null)
                            oldCells[i, j] = new CellData(i, j, CellState.Walkable);
            }
        }
        public static List<Cell> GetSurroundingCells(CellState? specifiedState, Cell cell)
        {
            List<Cell> foundCells = new List<Cell>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int checkX = cell.Datas.widthPos + x;
                    int checkY = cell.Datas.heightPos + y;

                    if (checkX >= 0 && checkX < cell.RefGrid.GridWidth && checkY >= 0 && checkY < cell.RefGrid.GridHeight)
                    {
                        if (specifiedState == null || (cell.RefGrid.Cells[checkY, checkX] != null && cell.RefGrid.Cells[checkY, checkX].Datas.state == specifiedState))
                            foundCells.Add(cell.RefGrid.Cells[checkY, checkX]);
                    }
                }
            }
            return foundCells;
        }

        public static WorldGrid GetIncludingGrid(WorldGrid worldGrid, Managers.GridPosition target)
        {
            foreach (CombatGrid innerGrid in worldGrid.InnerCombatGrids.Values)
            {
                if(target.longitude >= innerGrid.Longitude 
                 && target.longitude <= innerGrid.Longitude + innerGrid.GridWidth 
                 && target.latitude >= innerGrid.Latitude 
                 && target.latitude <= innerGrid.Latitude + innerGrid.GridHeight)
                {
                    return innerGrid;
                }
            }

            return worldGrid;
        }

        public static bool GetIncludingSubGrid(List<InnerGridData> innerGrids, Managers.GridPosition target, out InnerGridData includingGrid)
        {
            foreach (InnerGridData innerGrid in innerGrids)
            {
                if (target.longitude >= innerGrid.Longitude
                 && target.longitude <= innerGrid.Longitude + innerGrid.GridWidth - 1
                 && target.latitude >= innerGrid.Latitude
                 && target.latitude <= innerGrid.Latitude + innerGrid.GridHeight - 1)
                {
                    includingGrid = innerGrid;
                    return true;
                }
            }

            includingGrid = null;
            return false;
        }


        /// <summary>
        /// Find the closest cell to reach the shape (=innerGrid). Priority for top and bottom
        /// </summary>
        /// <returns></returns>
        public static Cell GetClosestCellToShape(WorldGrid refGrid, CombatGrid innerGrid, GridPosition entityPos)
        {
            return GetClosestCellToShape(refGrid, innerGrid.Latitude, innerGrid.Longitude, innerGrid.GridHeight, innerGrid.GridWidth, entityPos);
        }
        public static Cell GetClosestCellToShape(WorldGrid refGrid, int latitude, int longitude, int height, int width, GridPosition entityPos)
        {
            // T0DO: Width and height aren't returning the expected result

            // Top
            if (entityPos.latitude < latitude)
            {
                // Are we horizontally inside ?
                if (entityPos.longitude >= longitude && entityPos.longitude <= longitude + width)
                    return refGrid.Cells[latitude - 1, entityPos.longitude];
                // Are we on the right or left ?
                else
                {
                    if (entityPos.longitude < longitude)
                        return refGrid.Cells[latitude - 1, longitude];
                    else
                        return refGrid.Cells[latitude - 1, longitude + width - 1];
                }
            }
            // Bottom
            else if (entityPos.latitude >= latitude + height)
            {
                // Are we horizontally inside ?
                if (entityPos.longitude >= longitude && entityPos.longitude <= longitude + width)
                    return refGrid.Cells[latitude + height, entityPos.longitude];
                // Are we on the right or left ?
                else
                {
                    if (entityPos.longitude < longitude)
                        return refGrid.Cells[latitude + height, longitude];
                    else
                        return refGrid.Cells[latitude + height, longitude + width - 1];
                }
            }
            // Right
            else if (entityPos.longitude > longitude + width)
            {
                return refGrid.Cells[entityPos.latitude, longitude + width];
            }
            // Left
            else
            {
                return refGrid.Cells[entityPos.latitude, longitude - 1];
            }
        }
        /// <summary> 
        /// This method assume that the player is adjacent to the shape, don't use it outside of this context 
        /// </summary>
        /// <param name="refGrid"></param>
        /// <param name="innerGrid"></param>
        /// <param name="entityPos"></param>
        /// <returns></returns>
        public static Cell GetClosestAvailableCombatCell(WorldGrid refGrid, CombatGrid innerGrid, GridPosition entityPos)
        {
            bool top = false;
            bool bot = false;
            bool left = false;
            bool right = false;
            try
            {
                // Top
                if (entityPos.latitude < innerGrid.Latitude)
                {
                    top = true;
                    return innerGrid.Cells[0, entityPos.longitude - innerGrid.Longitude];
                }
                // Bottom
                else if (entityPos.latitude >= innerGrid.Latitude + innerGrid.GridHeight)
                {
                    bot = true;
                    return innerGrid.Cells[innerGrid.GridHeight - 1, entityPos.longitude - innerGrid.Longitude];
                }
                // Right
                else if (entityPos.longitude >= innerGrid.Longitude + innerGrid.GridWidth)
                {
                    right = true;
                    return innerGrid.Cells[entityPos.latitude - innerGrid.Latitude, innerGrid.GridWidth - 1];
                }
                // Left
                else
                {
                    left = true;
                    return innerGrid.Cells[entityPos.latitude - innerGrid.Latitude, 0];
                }
            }
            catch(Exception ex)
            {
                Debug.LogError("Couldn't find closest Cell because Entity[" + entityPos.latitude + ", " + entityPos.longitude +
                    "] failed innerGrid[" + innerGrid.Latitude + ", " + innerGrid.Longitude + "] for size of [" + innerGrid.GridHeight + ", " + innerGrid.GridWidth + "]\n" +
                    "top : " + top +
                    "bot : " + bot +
                    "left : " + left +
                    "right : " + right) ;
                return null;
            }
        }

        public static bool IsNeighbourCell(Cell fCell, Cell sCell)
        {
            return Mathf.Abs(fCell.Datas.widthPos - sCell.Datas.widthPos) <= 1 && Mathf.Abs(fCell.Datas.heightPos - sCell.Datas.heightPos) <= 1;
        }

        #region SPELLS

        public static bool[,] RotateSpellMatrix(bool[,] baseMatrix, Cell origin, Cell target)
        {
            int angle = GetMostFittingAngle(origin, target);

            // This means it's the current rotation
            if (angle == -1)
                return baseMatrix;
            else
                return RotateSpellMatrix(baseMatrix, angle);
        }

        public static bool[,] RotateSpellMatrix(bool[,] baseMatrix, int angle)
        {
            int numRows = baseMatrix.GetLength(0);
            int numCols = baseMatrix.GetLength(1);
            bool[,] outputMatrix = new bool[numCols, numRows];

            switch (angle)
            {
                case 90:
                    for (int i = 0; i < numRows; i++)
                    {
                        for (int j = 0; j < numCols; j++)
                        {
                            outputMatrix[j, numRows - i - 1] = baseMatrix[i, j];
                        }
                    }
                    break;

                case 180:
                    for (int i = 0; i < numRows; i++)
                    {
                        for (int j = 0; j < numCols; j++)
                        {
                            outputMatrix[numRows - i - 1, numCols - j - 1] = baseMatrix[i, j];
                        }
                    }
                    break;

                case 270:
                    for (int i = 0; i < numRows; i++)
                    {
                        for (int j = 0; j < numCols; j++)
                        {
                            outputMatrix[numCols - j - 1, i] = baseMatrix[i, j];
                        }
                    }
                    break;

                default:
                    Console.WriteLine("Invalid angle. Please enter 90, 180, or 270.");
                    break;
            }

            return outputMatrix;
        }

        public static int GetMostFittingAngle(Cell origin, Cell target)
        {
            int dx = target.Datas.widthPos - origin.Datas.widthPos;
            int dy = target.Datas.heightPos - origin.Datas.heightPos;

            if (dx == 0 && dy == 0)
            {
                return -1;
            }
            else if (dx == 0)
            {
                return dy > 0 ? 90 : 270;
            }
            else if (dy == 0)
            {
                return dx > 0 ? -1 : 180;
            }
            else if (dx > 0 && dy > 0)
            {
                return Math.Abs(dx) == Math.Abs(dy) ? -1 : 90;
            }
            else if (dx < 0 && dy > 0)
            {
                return Math.Abs(dx) == Math.Abs(dy) ? 180 : 270;
            }
            else if (dx < 0 && dy < 0)
            {
                return Math.Abs(dx) == Math.Abs(dy) ? -1 : 270;
            }
            else
            {
                return Math.Abs(dx) == Math.Abs(dy) ? 180 : 90;
            }
        }
        #endregion
    }
}
