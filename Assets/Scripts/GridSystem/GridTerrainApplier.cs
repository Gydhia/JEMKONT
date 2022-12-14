using System.Collections;
using System.Collections.Generic;
using Jemkont.GridSystem;
using Jemkont.Managers;
using UnityEngine;

public class GridTerrainApplier : MonoBehaviour
{
    //Properties
    private float CellsSize => SettingsManager.Instance.GridsPreset.CellsSize;

    public int RayDistance = -100;
    public int RayStartHeight = 50;


    // Start is called before the first frame update
    void Start()
    {
    }
    public void ApplyTerrainToGrid(CellData[,] cellDatas)
    {
        foreach (CellData cell in cellDatas)
        {
            RaycastHit TerrainHit = RayCastFromCell(cell);
            ApplyCellDataFromTerrainTag(cell, TerrainHit);
        }
    }

    private RaycastHit RayCastFromCell(CellData Cell)
    {
        Vector3 position = transform.position + new Vector3(Cell.widthPos * CellsSize + CellsSize/2, RayStartHeight, -Cell.heightPos-1 * CellsSize + CellsSize / 2);
        Vector3 Direction = new Vector3(0, RayDistance, 0);
        Debug.DrawRay(position, Direction, Color.red, 5);
        if (Physics.BoxCast(position, new Vector3(CellsSize, CellsSize, CellsSize), Direction, out RaycastHit hit, new Quaternion(), RayDistance, 1 << LayerMask.NameToLayer("Terrain")))
        {
            Debug.Log("Yes ");
        }
        return hit;
    }

    private void ApplyCellDataFromTerrainTag(CellData cell, RaycastHit terrain)
    {
        Debug.Log("3 : "+ terrain.transform);
        if (terrain.transform == null || terrain.transform.tag == "Walkable")
        {
            Debug.Log("Walkable");
            cell.state = CellState.Walkable;
            return;
        }
        if (terrain.transform.tag == "Blocked")
        {
            Debug.Log("Blocked");
            cell.state = CellState.Blocked;
        }
    }
}
