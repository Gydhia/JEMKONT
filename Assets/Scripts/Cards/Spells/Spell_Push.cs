    using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Spells
{
    public enum PushType
    {
        Left,
        Right,
        Top,
        Bottom,

        Inside,
        Outside
    }

    public class SpellData_Push : SpellData
    {
        [FoldoutGroup("PUSH Spell Datas")]
        public PushType PushType;
        [FoldoutGroup("PUSH Spell Datas")]
        public float PushDelay = 0.2f;
        [FoldoutGroup("PUSH Spell Datas")]
        public int PushAmount;
        [FoldoutGroup("PUSH Spell Datas")]
        public int PushDamages = 3;
        [FoldoutGroup("PUSH Spell Datas")]
        [Range(0.1f, 2f)]
        public float PushDamagesMultiplier = 0.5f;

        public SpellData_Push(PushType pushType, int pushAmount)
        {
            PushType = pushType;
            PushAmount = pushAmount;
        }
    }
    public class Spell_Push : Spell<SpellData_Push>
    {
        public Spell_Push(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, TargettingCondition targCond, CastingCondition castCond)
            : base(CopyData, RefEntity, TargetCell, ParentSpell, targCond, castCond)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();

            Managers.CombatManager.Instance.StartCoroutine(this.TryToPush());
        }

        public IEnumerator TryToPush()
        {
            var pushedEntities = this.GetTargets(this.TargetCell);

            int newX, offsetX;
            int newY, offsetY;

            int gridWidth = TargetCell.RefGrid.GridWidth;
            int gridHeight = TargetCell.RefGrid.GridHeight;

            foreach (var entity in pushedEntities)
            {
                bool blockedOnce = false;

                for (int i = 0; i < LocalData.PushAmount; i++)
                {
                    newX = entity.EntityCell.PositionInGrid.longitude;
                    newY = entity.EntityCell.PositionInGrid.latitude;

                    offsetX = newX - TargetCell.PositionInGrid.longitude;
                    offsetY = newY - TargetCell.PositionInGrid.latitude;

                    switch (LocalData.PushType)
                    {
                        case PushType.Left:
                            newX = Mathf.Max(0, entity.EntityCell.PositionInGrid.longitude - 1);
                            break;
                        case PushType.Right:
                            newX = Mathf.Min(gridWidth - 1, entity.EntityCell.PositionInGrid.longitude + 1);
                            break;
                        case PushType.Top:
                            newY = Mathf.Max(0, entity.EntityCell.PositionInGrid.latitude - 1);
                            break;
                        case PushType.Bottom:
                            newY = Mathf.Min(gridHeight - 1, entity.EntityCell.PositionInGrid.latitude + 1);
                            break;
                        case PushType.Inside:
                            if (offsetX < 0)
                                newX = Mathf.Max(0, entity.EntityCell.PositionInGrid.longitude + 1);
                            else if (offsetX > 0)
                                newX = Mathf.Min(gridWidth - 1, entity.EntityCell.PositionInGrid.longitude - 1);
                            if (offsetY < 0)
                                newY = Mathf.Max(0, entity.EntityCell.PositionInGrid.latitude + 1);
                            else if (offsetY > 0)
                                newY = Mathf.Min(gridHeight - 1, entity.EntityCell.PositionInGrid.latitude - 1);
                            break;
                        case PushType.Outside:
                            if (offsetX < 0)
                                newX = Mathf.Max(0, entity.EntityCell.PositionInGrid.longitude - 1);
                            else if (offsetX > 0)
                                newX = Mathf.Min(gridWidth - 1, entity.EntityCell.PositionInGrid.longitude + 1);
                            if (offsetY < 0)
                                newY = Mathf.Max(0, entity.EntityCell.PositionInGrid.latitude - 1);
                            else if (offsetY > 0)
                                newY = Mathf.Min(gridHeight - 1, entity.EntityCell.PositionInGrid.latitude + 1);
                            break;
                    }

                    Cell newCell = TargetCell.RefGrid.Cells[newY, newX];

                    // Means that we're pushing the entity to a free cell, it won't take any damage
                    if (newCell.Datas.state.HasFlag(CellState.NonWalkable))
                    {
                        if (!blockedOnce)
                        {
                            entity.ApplyStat(EntityStatistics.Health, (int)(-(LocalData.PushDamages * (LocalData.PushAmount * LocalData.PushDamagesMultiplier))));
                            blockedOnce = true;

                            yield return new WaitForSeconds(LocalData.PushDelay);
                        }
                    }
                    else 
                    {
                        entity.Teleport(newCell);

                        yield return new WaitForSeconds(LocalData.PushDelay);
                    }
                }
                entity.FireEntityPushed(new SpellEventData(RefEntity, LocalData.PushAmount));

            }
        }
    }
}