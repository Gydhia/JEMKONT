using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Spells
{
    public enum ETeleportType { GoTo, PullTo };

    public class SpellData_Teleport : SpellData
    {
        [InfoBox("Pull : teleport the target to YOU.\nGoTo: You get teleported TO the target.")]
        public ETeleportType TeleportType;
    }

    public class Spell_Teleport : Spell<SpellData_Teleport>
    {
        public Spell_Teleport(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();
            GetTargets(TargetCell);
            if (TargetEntities.Count == 1)
            {
                var cellToTP = LocalData.TeleportType == ETeleportType.PullTo ? RefEntity.EntityCell : TargetCell;
                if (LocalData.TeleportType == ETeleportType.PullTo)
                {
                    TargetEntities[0].SmartTeleport(cellToTP, Result);
                } else
                {
                    RefEntity.SmartTeleport(cellToTP, Result);
                }
            } else if (LocalData.TeleportType == ETeleportType.PullTo)
            {
                string debug = "NO ENTITIES";
                if (TargetEntities.Count > 0)
                {
                    debug = "";
                    foreach (var item in TargetEntities)
                    {
                        debug += item.ToString();
                    }
                }
                Debug.LogWarning($"TargetEntities.Count != 1 in the teleport spell. This is not an issue, but it's not normal. Entities: {debug}");
            }
            EndAction();
        }
    }
}
