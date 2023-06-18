using DownBelow.GridSystem;
using DownBelow.Spells;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "TargetConditions/TargetHasFullHPCondition")]
public class TargetHasFullHPCondition : TargettingCondition
{
    [InfoBox("Returns true if the target has full HP.")]
    public override bool Validated(Cell cell)
    {
        if (cell != null && cell.EntityIn != null)
        {
            return cell.EntityIn.Health >= int.MaxValue;
        }
        return false; //? i guess?
    }
    public override string SimpleToString()
    {
        return "the target has full HP";
    }
}
