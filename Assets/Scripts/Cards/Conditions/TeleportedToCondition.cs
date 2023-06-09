using DownBelow.GridSystem;
using DownBelow.Spells;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "TargetConditions/TeleportedToCondition")]
public class TeleportedToCondition : ConditionBase
{
    public ETargetType TypeOfTargetOnTeleported;

    public override string SimpleToString()
    {
        return $"we teleported on an \"{TypeOfTargetOnTeleported}\"";
    }

    public override bool Validated(SpellResult Result, Cell cell)
    {
        if (!Result.Teleported)
        {
            return false;
        }
        return TypeOfTargetOnTeleported switch
        {
            ETargetType.Self => Result.TeleportedTo.Any(x => x == Result.Caster),
            ETargetType.AllAllies => Result.TeleportedTo.Any(x => x.IsAlly),
            ETargetType.AllEnemies => Result.TeleportedTo.Any(x => !x.IsAlly),
            ETargetType.Enemy => Result.TeleportedTo.Any(x => !x.IsAlly),
            ETargetType.Ally => Result.TeleportedTo.Any(x => x.IsAlly),
            ETargetType.Empty => true,
            ETargetType.NCEs => false,//We can't for now.............
            ETargetType.CharacterEntities => Result.TeleportedTo.Count > 0,
            ETargetType.Entities => Result.TeleportedTo.Count > 0,
            ETargetType.All => true,
            _ => false,
        };
    }

}
