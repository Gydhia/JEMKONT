using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace DownBelow.Spells
{
    public class Spell_ApplyStatFromNCE : Spell<SpellData_Stats>
    {
        public Spell_ApplyStatFromNCE(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }
        public override void ExecuteAction()
        {
            base.ExecuteAction();
            GetTargets(TargetCell);
            foreach (var cell in TargetedCells)
            {
                NetworkManager.Instance.EntityAskToBuffAction(new Spell_Stats(new SpellData_Stats(LocalData.IsNegativeEffect, LocalData.Statistic, LocalData.StatAmount), RefEntity, cell, null, null));
            }
            List<Cell> targetCells = null;
            if (CombatManager.Instance.NCEs != null && CombatManager.Instance.NCEs.Count > 0)
            {
                targetCells = CombatManager.Instance.NCEs.Select(x => x.AttachedCell).ToList();
            }
            if (targetCells == null)
            {
                return;
            }
            foreach (var item in targetCells)
            {
                
            }
            EndAction();
        }

    }

}
