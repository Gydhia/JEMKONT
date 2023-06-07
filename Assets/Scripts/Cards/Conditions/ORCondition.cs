using DownBelow.GridSystem;
using DownBelow.Spells;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TargetConditions/ORCondition")]
public class ORCondition : ConditionBase
{
    [InfoBox("@ToString()")]
    public ConditionBase[] conditions;

    public override bool Validated(SpellResult Result, Cell cell)
    {
        foreach (var item in conditions)
        {
            if (item.Validated(Result, cell))
            {
                return true;
            }
        }
        return false;
    }

    public override string SimpleToString()
    {
        string res = "Condition Array is empty! This returns true if one of the conditions in the array is validated";

        if (conditions != null && conditions.Length != 0)
        {
            res = "either :";
            for (int i = 0;i < conditions.Length;i++)
            {
                ConditionBase cond = conditions[i];
                res += "\n\t- ";
                res += cond.SimpleToString();
                if (i != conditions.Length - 1)
                {
                    res += ";";
                }
            }
            res += "";
        }
        return res;
    }
}
