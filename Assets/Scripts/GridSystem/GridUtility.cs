using Jemkont.Entity;
using Jemkont.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Jemkont.GridSystem
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

        public static bool GetIncludingSubGrid(List<SubgridPlaceholder> innerGrids, Managers.GridPosition target, out SubgridPlaceholder includingGrid)
        {
            foreach (SubgridPlaceholder innerGrid in innerGrids)
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
            // Top
            if (entityPos.latitude < innerGrid.Latitude)
            {
                // Are we horizontally inside ?
                if (entityPos.longitude >= innerGrid.Longitude && entityPos.longitude <= innerGrid.Longitude + innerGrid.GridWidth)
                    return refGrid.Cells[innerGrid.Latitude - 1, entityPos.longitude];
                // Are we on the right or left ?
                else
                {
                    if(entityPos.longitude < innerGrid.Longitude)
                        return refGrid.Cells[innerGrid.Latitude - 1, innerGrid.Longitude];
                    else
                        return refGrid.Cells[innerGrid.Latitude - 1, innerGrid.Longitude + innerGrid.GridWidth - 1];
                }
            }
            // Bottom
            else if (entityPos.latitude > innerGrid.Latitude + innerGrid.GridHeight)
            {
                // Are we horizontally inside ?
                if (entityPos.longitude >= innerGrid.Longitude && entityPos.longitude <= innerGrid.Longitude + innerGrid.GridWidth)
                    return refGrid.Cells[innerGrid.Latitude + innerGrid.GridHeight + 1, entityPos.longitude];
                // Are we on the right or left ?
                else
                {
                    if (entityPos.longitude < innerGrid.Longitude)
                        return refGrid.Cells[innerGrid.Latitude + innerGrid.GridHeight + 1, innerGrid.Longitude];
                    else
                        return refGrid.Cells[innerGrid.Latitude + innerGrid.GridHeight + 1, innerGrid.Longitude + innerGrid.GridWidth - 1];
                }
            }
            // Right
            else if (entityPos.longitude > innerGrid.Longitude + innerGrid.GridWidth)
            {
                return refGrid.Cells[entityPos.latitude, innerGrid.Longitude + innerGrid.GridWidth];
            }
            // Left
            else
            {
                return refGrid.Cells[entityPos.latitude, innerGrid.Longitude - 1];
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
            // Top
            if (entityPos.latitude < innerGrid.Latitude)
            {
               return innerGrid.Cells[0, entityPos.longitude - innerGrid.Longitude];
            }
            // Bottom
            else if (entityPos.latitude > innerGrid.Latitude + innerGrid.GridHeight)
            {
               return innerGrid.Cells[innerGrid.GridHeight - 1, entityPos.longitude - innerGrid.Longitude];

            }
            // Right
            else if (entityPos.longitude > innerGrid.Longitude + innerGrid.GridWidth)
            {
                return innerGrid.Cells[entityPos.latitude - innerGrid.Latitude, innerGrid.GridWidth - 1];
            }
            // Left
            else
            {
                return innerGrid.Cells[entityPos.latitude - innerGrid.Latitude, 0];
            }
        }
        
    }
}
