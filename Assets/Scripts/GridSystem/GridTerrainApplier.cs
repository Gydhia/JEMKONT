using System.Collections;
using System.Collections.Generic;
using DownBelow.GridSystem;
using DownBelow.Managers;
using UnityEngine;

public class GridTerrainApplier : MonoBehaviour
{
    //Properties
    private float CellsSize => SettingsManager.Instance.GridsPreset.CellsSize;

    public int RayDistance = 2;
    public int RayStartHeight = -1;
    public int BoxCastDistance = 5;
    public int BoxCastStartHeight = -5;



    public void ApplyTerrainToGrid(EditorGridData Datas)
    {
        foreach (CellData cell in Datas.CellDatas)
        {
            RaycastHit TerrainHit = RayCastFromCell(cell, 1 << LayerMask.NameToLayer("Terrain"), Datas.TopLeftOffset);
            ApplyCellDataFromTerrainTag(cell, TerrainHit, Datas.InnerGrids);
            RaycastHit PropsHit = BoxCastFromCell(cell, 1 << LayerMask.NameToLayer("Props"), Datas.TopLeftOffset);
            ApplyCellDataFromPropsTag(cell, PropsHit, Datas.InnerGrids);
        }
    }

    private RaycastHit RayCastFromCell(CellData Cell, LayerMask layer, Vector3 offset)
    {
        Vector3 position = (transform.position + offset) + new Vector3(Cell.widthPos * CellsSize + CellsSize/2, RayStartHeight, -Cell.heightPos-1 * CellsSize + CellsSize / 2);
        Vector3 Direction = Vector3.up * RayDistance;
        Physics.Raycast(position, Direction, out RaycastHit hit, RayDistance, layer);
        return hit;
    }
    private RaycastHit BoxCastFromCell(CellData Cell, LayerMask layer, Vector3 offset)
    {
        Vector3 position = (transform.position + offset) + new Vector3(Cell.widthPos * CellsSize + CellsSize / 2, BoxCastStartHeight, -Cell.heightPos - 1 * CellsSize + CellsSize / 2);
        Vector3 Direction = Vector3.up * (BoxCastDistance);
        this.DrawBox(position, Quaternion.identity, new Vector3(CellsSize, BoxCastDistance,CellsSize), Color.blue);
        Physics.BoxCast(position, new Vector3(CellsSize, CellsSize, CellsSize), Direction, out RaycastHit hit, new Quaternion(), BoxCastDistance, layer);
        return hit;
    }

    private void ApplyCellDataFromTerrainTag(CellData cell, RaycastHit terrain, List<InnerGridData> InnerGrids)
    {
        GridPosition pos = new GridPosition(cell.widthPos, cell.heightPos);
        if (GridUtility.GetIncludingSubGrid(InnerGrids, pos, out InnerGridData includingGrid))
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

    private void ApplyCellDataFromPropsTag(CellData cell, RaycastHit terrain, List<InnerGridData> InnerGrids)
    {
        GridPosition pos = new GridPosition(cell.widthPos, cell.heightPos);
        if (GridUtility.GetIncludingSubGrid(InnerGrids, pos, out InnerGridData includingGrid))
        {
            GridPosition positionInCurrentGrid = new GridPosition(pos.longitude - includingGrid.Longitude, pos.latitude - includingGrid.Latitude);
            cell = includingGrid.CellDatas[positionInCurrentGrid.latitude, positionInCurrentGrid.longitude];
        }
        if (terrain.transform == null || terrain.transform.tag == "Walkable")
        {
            return;
        }
       
        cell.state = CellState.Blocked;
    }

    public void DrawBox(Vector3 pos, Quaternion rot, Vector3 scale, Color c)
    {
        // create matrix
        Matrix4x4 m = new Matrix4x4();
        m.SetTRS(pos, rot, scale);

        var point1 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f));
        var point2 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f));
        var point3 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f));
        var point4 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f));

        var point5 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f));
        var point6 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f));
        var point7 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f));
        var point8 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f));

        Debug.DrawLine(point1, point2, c, 3f);
        Debug.DrawLine(point2, point3, c, 3f);
        Debug.DrawLine(point3, point4, c, 3f);
        Debug.DrawLine(point4, point1, c, 3f);

        Debug.DrawLine(point5, point6, c, 3f);
        Debug.DrawLine(point6, point7, c, 3f);
        Debug.DrawLine(point7, point8, c, 3f);
        Debug.DrawLine(point8, point5, c, 3f);

        Debug.DrawLine(point1, point5, c, 3f);
        Debug.DrawLine(point2, point6, c, 3f);
        Debug.DrawLine(point3, point7, c, 3f);
        Debug.DrawLine(point4, point8, c, 3f);

        // optional axis display
        //Debug.DrawRay(m.GetPosition(), m.GetForward(), Color.magenta);
        //Debug.DrawRay(m.GetPosition(), m.GetUp(), Color.yellow);
        //Debug.DrawRay(m.GetPosition(), m.GetRight(), Color.red);
    }
}
