using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Spells;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TargetConditions/TargetTypeCondition")]
public class TargetTypeCondition : TargettingCondition
{
    [InfoBox("@ToString()")]
    public ETargetType TargetType;
    public override bool Validated(Cell cell)
    {
        return TargetType switch
        {
            ETargetType.Self => cell.EntityIn == CombatManager.CurrentPlayingEntity,
            ETargetType.AllAllies => cell.EntityIn.IsAlly,
            ETargetType.AllEnemies => !cell.EntityIn.IsAlly,
            ETargetType.Enemy => !cell.EntityIn.IsAlly,
            ETargetType.Ally => cell.EntityIn.IsAlly,
            ETargetType.Empty => cell.Datas.state.HasFlag(CellState.Walkable),
            ETargetType.NCEs => cell.AttachedNCE != null,
            ETargetType.CharacterEntities => cell.EntityIn != null,
            ETargetType.Entities => cell.EntityIn != null || cell.AttachedNCE != null,
            ETargetType.All => true,
            _ => false,
        };
    }
    public override string SimpleToString()
    {
        return "the target corresponds to the targetting type (NCEs for an nce, ally/allAllies for allies (same thing))";
    }
}
