using DownBelow.Pools;
using EODE.Wonderland;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Managers
{
    public class PoolManager : _baseManager<PoolManager>
    {
        public CellIndicatorPool CellIndicatorPool;

        private void Start()
        {
            CellIndicatorPool.InitPool(20);
        }
    }
}
