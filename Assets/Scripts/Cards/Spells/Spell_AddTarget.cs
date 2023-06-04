using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
{
    public class SpellData_AddSpecificTarget : SpellData
    {
        public enum ESpecificTargettingType { GetClosest, }
        public ESpecificTargettingType SpecificTargetType;
    }
    /// <summary>
    /// Does nothing more than storing cells in result
    /// </summary>
    public class Spell_AddTarget : Spell<SpellData>
    {
        public Spell_AddTarget(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();
            GetTargets(TargetCell);
            if (LocalData is SpellData_AddSpecificTarget Specific)
            {
                switch (Specific.SpecificTargetType)
                {
                    case SpellData_AddSpecificTarget.ESpecificTargettingType.GetClosest:
                        int min = int.MaxValue;
                        CharacterEntity minEntity = null;
                        foreach (var item in TargetEntities)
                        {
                            int newMin = Mathf.Min(GridManager.Instance.FindPath(RefEntity, item.EntityCell.PositionInGrid, true).Count, min);
                            if (newMin != min)
                            {
                                minEntity = item;
                            }
                            min = newMin;
                        }
                        TargetEntities.Clear();
                        TargetEntities.Add(minEntity);
                        break;
                }
            }
            Result.TargetedCells.AddRange(TargetedCells);

            EndAction();
        }
    }

}
