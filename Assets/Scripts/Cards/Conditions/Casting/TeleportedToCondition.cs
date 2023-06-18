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
        return TypeOfTargetOnTeleported switch
        {
            ETargetType.Self => Result.TeleportedTo.Any(x => x == Result.Caster),
            ETargetType.Enemy => Result.TeleportedTo.Any(x => !x.IsAlly),
            ETargetType.Ally => Result.TeleportedTo.Any(x => x.IsAlly),
            ETargetType.Empty => true,
            ETargetType.NCEs => false,//We can't for now. hasta luego.
            ETargetType.CharacterEntities => Result.TeleportedTo.Count > 0,
            ETargetType.Entities => Result.TeleportedTo.Count > 0,
            ETargetType.All => true,
            _ => false,
        };
    }
}
