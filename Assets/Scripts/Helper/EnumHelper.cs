using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnumHelper
{
    /// <summary>
    /// D�sol� kiki j'ai eu la flemme de cr�er une classe juste pour �a....................
    /// En �change je t'ai fait une m�thode qui return un random d'un tableau 2D..........
    /// </summary>
    /// <param name="entityStat"></param>
    /// <returns>The minimum value of the statistic.</returns>
    public static int MinValue(this EntityStatistics entityStat) => entityStat switch
    {
        EntityStatistics.None => 0,
        EntityStatistics.Health => 1,
        EntityStatistics.Mana => 0,
        EntityStatistics.MaxMana => 1,
        EntityStatistics.Speed => 0,
        EntityStatistics.Strength => 0,
        EntityStatistics.Defense => 0,
        EntityStatistics.Range => 0,
        _ => 0,
    };
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
