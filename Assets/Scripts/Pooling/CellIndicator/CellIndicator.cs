using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class CellIndicator : MonoBehaviour, IPoolable
{
    public IObjectPool Pool { get; set; }
    public bool Pooled { get; set; }

    public Color Color
    {
        get
        {
            return Color;
        }
        set
        {
            //TODO : Call Material Change?
        }
    }

    public void DisableFromPool()
    {
        // Tu reinitialises toutes les valeurs de l'objet / ou tu desabonnes aux events auxquels tu t'es potentiellement plug
        Color = new(0, 0, 0, 0);
        Pooled = false;
        //TODO: ResetParent?
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
