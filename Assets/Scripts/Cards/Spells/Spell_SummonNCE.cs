using DownBelow.Entity;
using DownBelow.GridSystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
{
    public class SpellData_Summon : SpellData
    {
        [FoldoutGroup("SUMMON Spell Datas"), Tooltip("(optional) Wall that's going to be placed on every cell\n(not rotated at all... Think about what prefab you put in there.)")]
        public NCEPreset NCEPreset;

        public SpellData_Summon(NCEPreset NCEPreset)
        {
            this.NCEPreset = NCEPreset;
        }
    }

    public class Spell_SummonNCE : Spell<SpellData_Summon>
    {
        public Spell_SummonNCE(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
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
            if(summondata.NCEPreset.entityToSummon is MoleHole hole)
            {
            }
            yield return null;
        }

    }

}
