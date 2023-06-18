using DownBelow.GridSystem;
using DownBelow.Spells;
using DownBelow.Spells.Alterations;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "TargetConditions/TargetAlteredCondition")]
public class TargetAlteredCondition : TargettingCondition
{
    [InfoBox("@ToString()")]
    public Alteration Alteration;

    public override bool Validated(Cell cell)
    {
        if (cell != null && cell.EntityIn != null)
        {
            return cell.EntityIn.Alterations.Any(x => x.GetType() == Alteration.GetType());
        }
        return false; //? i guess?
    }

    public override string SimpleToString()
    {
        if (Alteration != null)
        {
            return $"the targets have the {Alteration.GetType()} alteration.\n(sidenote: Only the type of the alteration is taken into account, the other values arent!)";

        } else
        {
            return "Alteration is null!";
        }
    }
}
