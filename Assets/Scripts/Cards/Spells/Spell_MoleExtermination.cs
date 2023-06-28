using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
namespace DownBelow.Spells
{
    public class SpellData_MoleExtermination : SpellData_Summon
    {
        public SpellData_MoleExtermination(NCEPreset NCEPreset) : base(NCEPreset)
        {
        }
        [HideInInspector] public MoleHole otherHole;
        [HideInInspector] public MoleHole Hole;
    }
    public class Spell_MoleExtermination : Spell<SpellData_Summon>
    {
        public Spell_MoleExtermination(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, TargettingCondition targCond, CastingCondition castCond) : base(CopyData, RefEntity, TargetCell, ParentSpell, targCond,castCond)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();
            SetTargets(TargetCell);
            var targets = TargetedCells;
            if (targets != null)
            {
                foreach (var target in targets)
                {
                    Managers.CombatManager.Instance.StartCoroutine(Spell_SummonNCE.SummonNCE(target, LocalData, RefEntity));
                }
                for (int i = 0;i < targets.Count;i++)
                {
                    Cell target = targets[i];
                    if(target.AttachedNCE is MoleHole hole)
                    {
                        int index = i == targets.Count - 1 ? 0 : i + 1;
                        hole.otherHole = targets[index].AttachedNCE as MoleHole;
                    }
                }
            }
        }

    }
}
