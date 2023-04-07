using EODE.Wonderland;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoSingleton<PoolManager>
{
    public CellIndicatorPool CellIndicatorPool;

    private void Start()
    {
        CellIndicatorPool.InitPool(50);
    }
}
