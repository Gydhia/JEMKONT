using DownBelow.Entity;
using DownBelow.GridSystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
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
    public class Spell_Push : Spell
    {
        [FoldoutGroup("PUSH Spell Datas")]
        public PushType PushType;
        [FoldoutGroup("PUSH Spell Datas")]
        public int PushAmount;
        [FoldoutGroup("PUSH Spell Datas")]
        public int PushDamages = 3;
        public Spell_Push(CharacterEntity RefEntity, Cell TargetCell, SpellAction ActionData, SpellCondition ConditionData = null) 
            : base(RefEntity, TargetCell, ActionData, ConditionData)
        {
        }

        Dictionary<CharacterEntity, List<Cell>> bn;

        public override void ExecuteAction()
        {
            this.TryToPush();

            base.ExecuteAction();
        }

        public void TryToPush()
        {
            var pushedEntities = this.GetTargets(RefEntity, base.TargetCell);

            int newX, offsetX;
            int newY, offsetY;

            int gridWidth = TargetCell.RefGrid.GridWidth;
            int gridHeight = TargetCell.RefGrid.GridHeight;

            foreach (var entity in pushedEntities)
            {
                newX = entity.EntityCell.PositionInGrid.longitude;
                newY = entity.EntityCell.PositionInGrid.latitude;

                offsetX = newX - TargetCell.PositionInGrid.longitude;
                offsetY = newY - TargetCell.PositionInGrid.latitude;

                switch (this.PushType)
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
                if(newCell.Datas.state == CellState.Walkable)
                {
                    // Temporary for tests
                    entity.EntityCell.EntityIn = null;
                    entity.EntityCell.Datas.state = CellState.Walkable;
                    entity.EntityCell = newCell;
                    newCell.EntityIn = entity; 
                    newCell.Datas.state = CellState.EntityIn;

                    entity.transform.position = newCell.transform.position;
                }
                else
                {
                    entity.ApplyStat(EntityStatistics.Health, -PushDamages, true);
                }
            }
        }

    }
}