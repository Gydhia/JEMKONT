using DownBelow.Spells;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "CastingConditions/DamageCondition")]
public class DamageCondition : CastingCondition
{
    [InfoBox("@ToString()")]
    [Tooltip("If Ticked, will check if you have done this damage on a unique target instead of in total.")]
    public bool UniqueTarget;
    public EConditionComparator Compared;
    public int Value;

    public override bool Validated(SpellResult Result)
    {
        int sum = 0;
        foreach (var item in Result.DamagesDealt)
        {
            if (UniqueTarget)
            {
                if (Compared.Compare(item.Value, Value))
                {
                    return true;
                }
            }
            else
            {
                sum += item.Value;
                if (Compared.Compare(sum, Value))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public override string SimpleToString()
    {
        var res = $"we dealt {Compared} {Value} damage";
        if (UniqueTarget)
        {
            res += " to a unique target";
        }
        return res;
    }
}
