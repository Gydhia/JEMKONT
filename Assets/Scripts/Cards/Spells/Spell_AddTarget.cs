using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;

namespace DownBelow.Spells
{
    public class SpellData_AddSpecificTarget : SpellData
    {
        public enum ESpecificTargettingType { GetClosest, }
        public ESpecificTargettingType SpecificTargetType;
    }
    public class SpellData_CombineWithSpellResult : SpellData
    {
        [Min(0), ShowIf(nameof(DoCombine))]
        [BoxGroup("Combine Result")]
        public int SpellResultIndexToCombine;
        [BoxGroup("Combine Result")]
        public bool DoCombine;
    }
    /// <summary>
    /// Does nothing more than storing cells in result
    /// </summary>
    public class Spell_AddTarget : Spell<SpellData>
    {
        public Spell_AddTarget(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, TargettingCondition targCond, CastingCondition castCond) : base(CopyData, RefEntity, TargetCell, ParentSpell, targCond,castCond)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();
            if (LocalData is SpellData_AddSpecificTarget Specific) {
                switch (Specific.SpecificTargetType) {
                    case SpellData_AddSpecificTarget.ESpecificTargettingType.GetClosest:
                        int min = int.MaxValue;
                        CharacterEntity minEntity = null;
                        var targets = TargetEntities.FindAll(x => x != RefEntity);
                        foreach (var item in targets) {
                            int newMin = Mathf.Min(item.EntityCell.DistanceWith(RefEntity.EntityCell), min);
                            if (newMin != min) {
                                minEntity = item;
                            }
                            min = newMin;
                        }
                        TargetEntities.Clear();
                        TargetedCells.Clear();
                        TargetEntities.Add(minEntity);
                        TargetedCells.AddRange(TargetEntities.Select(x => x.EntityCell));
                        break;
                }
            } else if (LocalData is SpellData_CombineWithSpellResult combine && combine.DoCombine)
            {
                var spell = GetSpellFromIndex(combine.SpellResultIndexToCombine);
                TargetedCells.AddRange(spell.Result.TargetedCells);
                TargetEntities.AddRange(spell.TargetEntities);
            }
            Result.TargetedCells.AddRange(TargetedCells);
        }
    }

}
