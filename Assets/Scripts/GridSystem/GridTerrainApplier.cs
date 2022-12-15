using System.Collections;
using System.Collections.Generic;
using Jemkont.GridSystem;
using Jemkont.Managers;
using UnityEngine;

public class GridTerrainApplier : MonoBehaviour
{
    //Properties
    private float CellsSize => SettingsManager.Instance.GridsPreset.CellsSize;

    public int RayDistance = 2;
    public int RayStartHeight = -1;
    public int BoxCastDistance = 5;
    public int BoxCastStartHeight = -5;


    // Start is called before the first frame update
    void Start()
    {
    }
    public void ApplyTerrainToGrid(CellData[,] cellDatas, List<SubgridPlaceholder> InnerGrids)
    {
        foreach (CellData cell in cellDatas)
        {
            RaycastHit TerrainHit = RayCastFromCell(cell, 1 << LayerMask.NameToLayer("Terrain"));
            ApplyCellDataFromTerrainTag(cell, TerrainHit, InnerGrids);
            RaycastHit PropsHit = BoxCastFromCell(cell, 1 << LayerMask.NameToLayer("Props"));
            ApplyCellDataFromPropsTag(cell, PropsHit, InnerGrids);
        }
    }

    private RaycastHit RayCastFromCell(CellData Cell, LayerMask layer)
    {
        Vector3 position = transform.position + new Vector3(Cell.widthPos * CellsSize + CellsSize/2, RayStartHeight, -Cell.heightPos-1 * CellsSize + CellsSize / 2);
        Vector3 Direction = Vector3.up * RayDistance;
        Physics.Raycast(position, Direction, out RaycastHit hit, RayDistance, layer);
        return hit;
    }
    private RaycastHit BoxCastFromCell(CellData Cell, LayerMask layer)
    {
        Vector3 position = transform.position + new Vector3(Cell.widthPos * CellsSize + CellsSize / 2, BoxCastStartHeight, -Cell.heightPos - 1 * CellsSize + CellsSize / 2);
        Vector3 Direction = Vector3.up * (BoxCastDistance);
        Debug.DrawRay(position, Direction, Color.red, 3);
        Physics.BoxCast(position, new Vector3(CellsSize, CellsSize, CellsSize), Direction, out RaycastHit hit, new Quaternion(), BoxCastDistance, layer);
        return hit;
    }

    private void ApplyCellDataFromTerrainTag(CellData cell, RaycastHit terrain, List<SubgridPlaceholder> InnerGrids)
    {
        GridPosition pos = new GridPosition(cell.widthPos, cell.heightPos);
        if (GridUtility.GetIncludingSubGrid(InnerGrids, pos, out SubgridPlaceholder includingGrid))
        {
            GridPosition positionInCurrentGrid = new GridPosition(pos.longitude - includingGrid.Longitude, pos.latitude - includingGrid.Latitude);
            cell = includingGrid.CellDatas[positionInCurrentGrid.latitude, positionInCurrentGrid.longitude];
        }
        if (terrain.transform == null || terrain.transform.tag == "Blocked")
        {
            cell.state = CellState.Blocked;
            return;
        }
        if (terrain.transform.tag == "Walkable")
        {
            cell.state = CellState.Walkable;
        }
    }

    private void ApplyCellDataFromPropsTag(CellData cell, RaycastHit terrain, List<SubgridPlaceholder> InnerGrids)
    {
        GridPosition pos = new GridPosition(cell.widthPos, cell.heightPos);
        if (GridUtility.GetIncludingSubGrid(InnerGrids, pos, out SubgridPlaceholder includingGrid))
        {
            GridPosition positionInCurrentGrid = new GridPosition(pos.longitude - includingGrid.Longitude, pos.latitude - includingGrid.Latitude);
            cell = includingGrid.CellDatas[positionInCurrentGrid.latitude, positionInCurrentGrid.longitude];
        }
        if (terrain.transform == null || terrain.transform.tag == "Walkable")
        {
            return;
        }
        if (terrain.transform.tag == "Blocked")
        {
            cell.state = CellState.Blocked;
        }
    }
}
