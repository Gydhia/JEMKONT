using DownBelow.Entity;
using DownBelow.Events;
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
    public class TeleportationEventData : EventData<TeleportationEventData>
    {
        public CharacterEntity EntityThatTeleported;
        public CharacterEntity EntityTheyTriedTeleportingTo; 
        public Cell StartCell; 
        public Cell EndCell;

        public TeleportationEventData(CharacterEntity EntityThatTeleported, CharacterEntity EntityTheyTriedTeleportingTo, Cell StartCell, Cell EndCell)
        {
            this.EntityThatTeleported = EntityThatTeleported;
            this.EntityTheyTriedTeleportingTo = EntityTheyTriedTeleportingTo;
            this.StartCell = StartCell;
            this.EndCell = EndCell;
        }
    }
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
                    Cell oldCell = TargetEntities[0].EntityCell;
                    cellToTP = TargetEntities[0].SmartTeleport(cellToTP, Result);
                    RefEntity.FireOnTeleportation(new(TargetEntities[0], RefEntity,oldCell,cellToTP));
                } else
                {
                    Cell oldCell = RefEntity.EntityCell;
                    cellToTP = RefEntity.SmartTeleport(cellToTP, Result);
                    RefEntity.FireOnTeleportation(new(RefEntity,TargetCell.EntityIn,oldCell,cellToTP));
                }
                TargetedCells.Clear();
                TargetedCells.Add(LocalData.TeleportType == ETeleportType.PullTo ? cellToTP : TargetCell);
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

        }
    }
}
