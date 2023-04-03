using DownBelow.GridSystem;
using DownBelow.Managers;
using mattmc3.dotmore.Collections.Generic;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorGridData
{
    [HideInInspector]
    public CellData[,] CellDatas;

    [FoldoutGroup("Transform")]
    public Vector3 TopLeftOffset;

    [FoldoutGroup("Transform")]
    public int GridHeight = 8;
    [FoldoutGroup("Transform")]
    public int GridWidth = 5;

    [FoldoutGroup("Datas")]
    public bool ToLoad = false;
    [FoldoutGroup("Datas")]
    public bool IsCombatGrid = false;
    [FoldoutGroup("Datas")]
    public List<InnerGridData> InnerGrids;
    [FoldoutGroup("Datas")]
    public OrderedDictionary<GridPosition, BaseSpawnablePreset> Spawnables;
}
