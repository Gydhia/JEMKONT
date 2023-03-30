using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorGridData
{
    public Vector3 TopLeftOffset;

    public CellData[,] CellDatas;

    public List<SubgridPlaceholder> InnerGrids;

    public Dictionary<GridPosition, BaseSpawnablePreset> Spawnables;
}
