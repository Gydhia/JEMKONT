using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellIndicatorPool : ObjectPool<CellIndicator>
{
    public override CellIndicator GetPooled()
    {
        return base.GetPooled();
    }
    public override void ReleasePooled(CellIndicator poolObject)
    {
        base.ReleasePooled(poolObject);
    }
}
