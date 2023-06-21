using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Spells;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "TargetConditions/TargetTypeCondition")]
public class TargetTypeCondition : TargettingCondition
{
    [InfoBox("@ToString()")]
    public ETargetType TargetType;
    public override bool Validated(Cell cell)
    {
        bool validated = TargetType.ValidateTarget(cell);
        return validated;
    }
    public override string SimpleToString()
    {
        return "the target corresponds to the targetting type (NCEs for an nce, ally/allAllies for allies (same thing))";
    }
}
