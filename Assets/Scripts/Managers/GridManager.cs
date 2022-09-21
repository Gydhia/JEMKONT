using Jemkont.GridSystem;
using Jemkont.Player;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Managers
{
    public class GridManager : _baseManager<GridManager>
    {
      
        public List<Cell> Path;

        #region Assets_reference
        public Cell GridPrefab;
        public GameObject GridHandler;
        public GameObject Plane;
        public PlayerBehavior PlayerPrefab;
        #endregion
        public CombatGrid MainGrid;
        public GameObject TestPlane;

        public PlayerBehavior Player;
        

        private void Start()
        {
            this._generateGrid(10, 15);
        }

        [Button]
        private void _generateGrid(int height, int width)
        {
            this.MainGrid = new CombatGrid(height, width);

            float cellsWidth = SettingsManager.Instance.GridsPreset.CellsSize;

            // Place the test plane
            this.TestPlane = Instantiate(
                this.Plane,
                new Vector3(
                    (width * cellsWidth) / 2,
                    0f,
                    (height * cellsWidth) / 2
                ),
                Quaternion.identity,
                this.GridHandler.transform
            );
            this.TestPlane.transform.localScale = new Vector3(width * (cellsWidth / 10f), 0f, height * (cellsWidth / 10f));

            this.Player = Instantiate(this.PlayerPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity, this.transform);
            this.Player.Init(this.MainGrid.Cells[0, 0].PositionInGrid, this.MainGrid.Cells[0, 0].WorldPosition);
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // layer 6 = ground
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 6))
                {
                    for (int i = 0; i < this.Path.Count; i++)
                    {
                        if (this.Path[i] != null)
                            this.Path[i].ChangeStateColor(false);
                    }
                    float cellSize = SettingsManager.Instance.GridsPreset.CellsSize;
                    GridPosition clickPosition = new GridPosition((int)((hit.point.x - this.MainGrid.TopLeftOffset.x) / cellSize), (int)((hit.point.z - this.MainGrid.TopLeftOffset.z) / cellSize));
                    this.FindPath(clickPosition);

                    for (int i = 0; i < this.Path.Count; i++)
                    {
                        if (this.Path[i] != null)
                            this.Path[i].ChangeStateColor(true);
                    }
                }
            }
        }

        public void ClearMainGrid()
        {
            this.MainGrid.ClearCells();
            Destroy(this.TestPlane); 
        }

        /// <summary>
        /// While calculate the closest path to a target, storing it in the Path var of the GridManager
        /// </summary>
        /// <param name="target"></param>
        public void FindPath(GridPosition target)
        {
            Cell startCell = this.MainGrid.Cells[Player.PlayerPosition.x, Player.PlayerPosition.y];
            Cell targetCell = this.MainGrid.Cells[target.x, target.y];

            List<Cell> openSet = new List<Cell>();
            HashSet<Cell> closedSet = new HashSet<Cell>();

            openSet.Add(startCell);

            while (openSet.Count > 0)
            {
                Cell currentCell = openSet[0];
                for (int i = 1; i < openSet.Count; i++) {
                    if(openSet[i].fCost < currentCell.fCost || openSet[i].fCost == currentCell.fCost && openSet[i].hCost < currentCell.hCost) {
                        currentCell = openSet[i];
                    }
                }

                openSet.Remove(currentCell);
                closedSet.Add(currentCell);

                if (currentCell == targetCell) {
                    this.RetracePath(startCell, targetCell);
                    return;
                }

                foreach (Cell neighbour in this.GetNeighbours(currentCell))
                {
                    if (!neighbour.Walkable || closedSet.Contains(neighbour))
                        continue;

                    int newMovementCostToNeightbour = currentCell.gCost + GetDistance(currentCell, neighbour);
                    if (newMovementCostToNeightbour < neighbour.gCost || !openSet.Contains(neighbour)) {
                        neighbour.gCost = newMovementCostToNeightbour;
                        neighbour.hCost = GetDistance(neighbour, targetCell);
                        neighbour.parent = currentCell;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }
        }

        public void RetracePath(Cell startCell, Cell endCell)
        {
            List<Cell> path = new List<Cell>();
            Cell currentCell = endCell;

            while(currentCell != startCell) {
                path.Add(currentCell);
                currentCell = currentCell.parent;
            }
            path.Reverse();

            this.Path = path;
        }

        public List<Cell> GetNeighbours(Cell cell)
        {
            List<Cell> neighbours = new List<Cell>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = cell.xPos + x;
                    int checkY = cell.yPos + y;
                
                    if(checkX >= 0 && checkX < this.MainGrid.GridWidth && checkY >= 0 && checkY < this.MainGrid.GridHeight)
                    {
                        neighbours.Add(this.MainGrid.Cells[checkX, checkY]);
                    }
                }
            }

            return neighbours;
        }

        public int GetDistance(Cell cellA, Cell cellB)
        {
            int dstX = Mathf.Abs(cellA.xPos - cellB.xPos);
            int dstY = Mathf.Abs(cellA.yPos - cellB.yPos);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }
    public struct GridPosition
    {
        public GridPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static readonly GridPosition zero = new GridPosition(0, 0);

        public int x { get; private set; }
        public int y { get; private set; }
    }
}
