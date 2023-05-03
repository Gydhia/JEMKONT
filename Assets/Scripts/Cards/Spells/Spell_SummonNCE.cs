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
        [FoldoutGroup("SUMMON Spell Datas"), Tooltip("For how long the NCE is going to be up. Negative value for infinity.")]
        public int Uptime;
        [FoldoutGroup("SUMMON Spell Datas"), Tooltip("(optional) Wall that's going to be placed on every cell\n(not rotated at all... Think about what prefab you put in there.)")]
        public NonCharacterEntity NCEPrefab;

    }

    public class Spell_SummonNCE : Spell<SpellData_Summon>
    {
        public Spell_SummonNCE(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();
            foreach(Cell targetCell in TargetedCells)
            {
                Managers.CombatManager.Instance.StartCoroutine(this.SummonNCE(targetCell));
            }
            base.EndAction();
        }

        public IEnumerator SummonNCE(Cell cell)
        {
            NonCharacterEntity NCEInstance = GameObject.Instantiate(LocalData.NCEPrefab, cell.transform);
            cell.ChangeCellState(CellState.EntityIn,true);
            NCEInstance.Init(cell,LocalData.Uptime,RefEntity);
            yield return null;
        }

    }

}
