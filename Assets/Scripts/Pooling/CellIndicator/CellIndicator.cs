using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class CellIndicator : MonoBehaviour, IPoolable
{
    public IObjectPool Pool { get; set; }
    public bool Pooled { get; set; }

    public MeshRenderer CellRenderer;
    public Color Color
    {
        get
        {
            return CellRenderer.material.color;
        }
        set
        {
            // TODO : change this through Material Property Block instead
            CellRenderer.material.color = value;
        }
    }

    public void DisableFromPool()
    {
        Pooled = false;
        gameObject.SetActive(false);
    }

    public bool TryReleaseToPool()
    {
        if (this.Pool != null)
        {
            return this.Pool.TryReleaseToPool(this);
        }
        return false;
    }
}
