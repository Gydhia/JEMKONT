using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Spells
{


    public class SpellData_Wall : SpellData
    {
        [FoldoutGroup("WALL Spell Datas"), Tooltip("For how long the wall is going to be up.")]
        public int Uptime;
        [FoldoutGroup("WALL Spell Datas"), Tooltip("(optional) Wall that's going to be placed on every cell\n(not rotated at all... Think about what prefab you put in there.)")]
        public GameObject WallPrefab;

    }
    public class Spell_Wall : Spell<SpellData_Wall>
    {
        public Spell_Wall(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();
            TargetCell.ChangeCellState(CellState.Blocked);
            Managers.CombatManager.Instance.StartCoroutine(this.WallComingUp());
        }

        public IEnumerator WallComingUp()
        {
            var go = Object.Instantiate(LocalData.WallPrefab);
            TempObject tempobj;
            if(!go.TryGetComponent(out tempobj))
            {
                tempobj = go.AddComponent<TempObject>();
            }
            tempobj.Init(TargetCell,LocalData.Uptime,RefEntity);
            //Coroutine if we want to make the wall come out of the ground
            yield return null;
        }
    }
}
