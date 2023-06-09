using DownBelow.GridSystem;
using DownBelow.Spells;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "TargetConditions/NOTCondition")]

public class NOTCondition : ConditionBase
{
    [InfoBox("@ToString()")]
    public ConditionBase condition;

    public override bool Validated(SpellResult Result, Cell cell)
    {
        return !condition.Validated(Result, cell);
    }
    public override string SimpleToString()
    {
        if (condition == null) return "";
        return "Not " + condition.ToString();
    }
}
