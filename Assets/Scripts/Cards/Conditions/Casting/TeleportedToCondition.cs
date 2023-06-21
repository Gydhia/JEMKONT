using DownBelow.Spells;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "CastingConditions/TeleportedToCondition")]
public class TeleportedToCondition : CastingCondition
{
    public ETargetType TypeOfTargetOnTeleported;

    public override string SimpleToString()
    {
        return $"we teleported on an \"{TypeOfTargetOnTeleported}\"";
    }

    public override bool Validated(SpellResult Result)
    {
        if (!Result.Teleported)
        {
            return false;
        }
        bool validated = true;
        if (TypeOfTargetOnTeleported.HasFlag(ETargetType.None))
        {
            validated = false; //en mode MALVEILLANCE MAAAAAAAAAAX
        }
        if (TypeOfTargetOnTeleported.HasFlag(ETargetType.Ally))
        {
            validated = Result.TeleportedTo.Any(x => x.IsAlly);
        }
        if (TypeOfTargetOnTeleported.HasFlag(ETargetType.Self))
        {
            validated = Result.TeleportedTo.Any(x => x == Result.Caster);
        }
        if (TypeOfTargetOnTeleported.HasFlag(ETargetType.Enemy))
        {
            validated = Result.TeleportedTo.Any(x => !x.IsAlly);
        }
        if (TypeOfTargetOnTeleported.HasFlag(ETargetType.NCEs))
        {
            validated = false;
        }
        if (TypeOfTargetOnTeleported.HasFlag(ETargetType.Entities))
        {
            validated = Result.TeleportedTo.Count > 0;
        }
        if (TypeOfTargetOnTeleported.HasFlag(ETargetType.CharacterEntities))
        {
            validated = Result.TeleportedTo.Count > 0;
        }
        if (TypeOfTargetOnTeleported.HasFlag(ETargetType.All))
        {
            //Validated doesn't change!
        }
        if (TypeOfTargetOnTeleported.HasFlag(ETargetType.Empty))
        {
            validated = true;
        }
        return validated;
    }
}
