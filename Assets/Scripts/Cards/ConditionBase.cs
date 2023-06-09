using DownBelow.GridSystem;
using DownBelow.Spells;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ConditionBase : SerializedScriptableObject
{
    public abstract bool Validated(SpellResult Result, Cell cell);
    public override string ToString()
    {
        return $"Validated if {SimpleToString()}.";
    }
    public abstract string SimpleToString();
}
