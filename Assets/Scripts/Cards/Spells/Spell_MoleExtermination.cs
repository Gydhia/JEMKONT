using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Spells
{

    public class Spell_MoleExtermination : Spell<SpellData_Summon>
    {
        public Spell_MoleExtermination(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();
            GetTargets(TargetCell);
            foreach (Cell targetCell in TargetedCells)
            {
                Managers.CombatManager.Instance.StartCoroutine(SummonNCE(targetCell, LocalData, RefEntity));
            }
            EndAction();
        }

        public static IEnumerator SummonNCE(Cell cell, SpellData_Summon summondata, CharacterEntity RefEntity)
        {
            summondata.NCEPreset.InitNCE(cell, RefEntity);
            yield return null;
        }

    }
}

