using DownBelow.Entity;
using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

                for (int i = 0;i < newHeight;i++)
                    for (int j = 0;j < newWidth;j++)
                        if (oldCells[i, j] == null)
                            oldCells[i, j] = new CellData(i, j, CellState.Walkable);
            }
        }
        public static List<Cell> GetSurroundingCells(CellState? specifiedState, Cell cell)
        {
            List<Cell> foundCells = new List<Cell>();
            foundCells = GridManager.Instance.GetCombatNeighbours(cell, cell.RefGrid)
                .Where((cell) => specifiedState == null ||
                (cell != null && cell.Datas.state == specifiedState))
                .ToList();
            return foundCells;
        }

        public static WorldGrid GetIncludingGrid(WorldGrid worldGrid, Managers.GridPosition target)
        {
            foreach (CombatGrid innerGrid in worldGrid.InnerCombatGrids.Values)
            {
                if (target.longitude >= innerGrid.Longitude
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
        public static Cell GetClosestCellToShape(WorldGrid refGrid, int latitude, int longitude, int height, int width, GridPosition entityPos, bool closestOrNothing = false)
        {
            // T0DO: Width and height aren't returning the expected result
            Cell res = null;
            // Top
            if (closestOrNothing)
            {

                if (entityPos.latitude < latitude)
                {
                    // Are we vertically inside ?
                    if (entityPos.longitude >= longitude && entityPos.longitude <= (longitude - 1) + width)
                        res = refGrid.Cells[latitude - 1, entityPos.longitude];

                    // Are we on the right or left ?
                    else
                    {
                        if (entityPos.longitude < longitude)
                            res = refGrid.Cells[latitude - 1, longitude];
                        else
                            res = refGrid.Cells[latitude - 1, longitude + width - 1];
                    }
                }
                // Bottom
                else if (entityPos.latitude >= latitude + height)
                {
                    // Are we horizontally inside ?
                    if (entityPos.longitude >= longitude && entityPos.longitude <= (longitude - 1) + width)
                        res = refGrid.Cells[latitude + height, entityPos.longitude];
                    // Are we on the right or left ?
                    else
                    {
                        if (entityPos.longitude < longitude)
                            res = refGrid.Cells[latitude + height, longitude];
                        else
                            res = refGrid.Cells[latitude + height, longitude + width - 1];
                    }
                }
                // Right
                else if (entityPos.longitude >= longitude + width)
                {
                    res = refGrid.Cells[entityPos.latitude, longitude + width];
                }
                // Left
                else
                {
                    res = refGrid.Cells[entityPos.latitude, longitude - 1];
                }
            } else
            {
                List<Cell> possibleCells = new List<Cell>()
                {
                    refGrid.Cells[latitude - 1, longitude],
                    refGrid.Cells[latitude + 1, longitude],
                    refGrid.Cells[latitude, longitude+1],
                    refGrid.Cells[latitude, longitude-1],
            };
                return possibleCells.First((cell) => cell.Datas.state == CellState.Walkable);
            }
            if (res.Datas.state != CellState.Walkable)
            {
                //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
                //Do something like GetClosestCellToShape(DontReallyNeedToBeTHEclosest = true);
                return GetClosestCellToShape(refGrid, latitude, longitude, height, width, entityPos, true);
                //Yeah that
            } else
            {
                return res;
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
            } catch (Exception ex)
            {
                Debug.LogError("Couldn't find closest Cell because Entity[" + entityPos.latitude + ", " + entityPos.longitude +
                    "] failed innerGrid[" + innerGrid.Latitude + ", " + innerGrid.Longitude + "] for size of [" + innerGrid.GridHeight + ", " + innerGrid.GridWidth + "]\n" +
                    "top : " + top +
                    "bot : " + bot +
                    "left : " + left +
                    "right : " + right);
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
            int angle = GetRotationForMatrix(origin, target);

            // This means it's the current rotation
            if (angle == 0)
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
                    for (int i = 0;i < numRows;i++)
                    {
                        for (int j = 0;j < numCols;j++)
                        {
                            outputMatrix[j, numRows - i - 1] = baseMatrix[i, j];
                        }
                    }
                    break;

                case 180:
                    for (int i = 0;i < numRows;i++)
                    {
                        for (int j = 0;j < numCols;j++)
                        {
                            outputMatrix[numRows - i - 1, numCols - j - 1] = baseMatrix[i, j];
                        }
                    }
                    break;

                case 270:
                    for (int i = 0;i < numRows;i++)
                    {
                        for (int j = 0;j < numCols;j++)
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

        /// <summary>
        /// We assume to use this method for matrixes. So we'll return a rotation considering the base direction is top
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static int GetRotationForMatrix(Cell origin, Cell target)
        {
            int dx = target.Datas.widthPos - origin.Datas.widthPos;
            int dy = target.Datas.heightPos - origin.Datas.heightPos;

            // /!\ rotations are counterclockwise (as matrixes should rotate)

            // right
            if (dx > 0)
                return 270;
            // left
            else if (dx < 0)
                return 90;
            // top -> base
            else if (dy > 0)
                return 0;
            // bottom
            else
                return 180;
        }

        public static bool IsCellWithinPlayerRange(ref bool[,] playerRange, GridPosition playerPos, GridPosition targetCell)
        {
            int xMin = Mathf.RoundToInt(playerPos.longitude - playerRange.GetLength(1) / 2f);
            int yMin = Mathf.RoundToInt(playerPos.latitude - playerRange.GetLength(0) / 2f);

            int xOffset = targetCell.longitude - xMin;
            int yOffset = targetCell.latitude - yMin;

            if (xOffset >= 0 && yOffset >= 0 && xOffset < playerRange.GetLength(1) && yOffset < playerRange.GetLength(0))
            {
                return playerRange[xOffset, yOffset];
            }

            return false;
        }

        public static List<Cell> TransposeShapeToCell(ref bool[,] shape, Cell cell)
        {
            List<Cell> cells = new List<Cell>();

            // Calculate the position of the pattern relative to the player's cell
            int offsetX = shape.GetLength(0) / 2;
            int offsetY = shape.GetLength(1) / 2;

            // Iterate over the cells in the pattern and add the corresponding cells in the grid to the list
            for (int x = 0;x < shape.GetLength(0);x++)
            {
                for (int y = 0;y < shape.GetLength(1);y++)
                {
                    if (shape[x, y])
                    {
                        int gridX = cell.Datas.widthPos + x - offsetX;
                        int gridY = cell.Datas.heightPos + y - offsetY;

                        // Check if the grid position is within the bounds of the grid
                        if (gridX >= 0 && gridX < cell.RefGrid.GridWidth && gridY >= 0 && gridY < cell.RefGrid.GridHeight)
                        {
                            cells.Add(cell.RefGrid.Cells[gridY, gridX]);
                        }
                    }
                }
            }

            return cells;
        }

        #endregion
    }
}
