using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NotConditionBase<T> : ConditionBase<T>
{
    [InfoBox("@ToString()")]
    public ConditionBase<T> condition;

    public override bool Validated(T obj)
    {
        return !condition.Validated(obj);
    }
    private void OnValidate()
    {
        if (condition != null && condition == this)
        {
            condition = null;
        }
    }
    public override string SimpleToString()
    {
        string res;
        if(condition != null)
        {
            res = condition.SimpleToString();
        }
        else
        {
            res = "Condition is empty. This accepts conditions with type : " + typeof(T);
        }
        Debug.Log(res);
        return res;
    }
    
}
