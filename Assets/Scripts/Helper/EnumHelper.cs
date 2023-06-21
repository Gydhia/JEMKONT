using DownBelow.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EConditionComparator { MoreThan, LessThan, Equal }

public static class EnumHelper
{

    public static int MinValue(this EntityStatistics entityStat) => entityStat switch
    {
        EntityStatistics.None => 0,
        EntityStatistics.Health => 1,
        EntityStatistics.Mana => 0,
        EntityStatistics.Speed => 0,
        EntityStatistics.Strength => 0,
        EntityStatistics.Defense => 0,
        EntityStatistics.Range => 0,
        _ => 0,
    };

    public static ETargetType Next(this ETargetType type)
    {
        int inttype = (int)type *2;
        if (inttype <= 8)
        {
            if(inttype == 0)
            {
                inttype = 1;
            }
            if(inttype == 4) {
                return ETargetType.Empty;
            }
            type = (ETargetType)inttype;
        }
        return type;
    }

    /// <summary>
    /// Does the shit.
    /// </summary>
    /// <param name="comparator"></param>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    public static bool Compare(this EConditionComparator comparator, int value1, int value2)
    {
        return comparator switch
        {
            EConditionComparator.MoreThan => value1 > value2,
            EConditionComparator.LessThan => value1 < value2,
            EConditionComparator.Equal => value1 == value2,
            _ => false,
        };
    }
}
