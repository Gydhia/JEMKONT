using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConditionBase<T> : SerializedScriptableObject
{
    public abstract bool Validated(T obj);
    public override string ToString()
    {
        return $"Validated if {SimpleToString()}.";
    }
    public abstract string SimpleToString();
}
