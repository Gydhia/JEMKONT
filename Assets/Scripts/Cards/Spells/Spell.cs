using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Mechanics;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DownBelow.Spells
{
    public abstract class Spell<T> : Spell where T : SpellData
    {
        protected T LocalData => this.Data as T;

        protected Spell(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) 
            : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }
    }

    public abstract class Spell : EntityAction
    {
        public Spell(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost)
           : base(RefEntity, TargetCell)
        {
            this.Data = CopyData;
            this.Data.Refresh();
            this.ParentSpell = ParentSpell;
            this.ConditionData = ConditionData;
            this.Cost = Cost;
        }

        public int Cost;
        public SpellData Data;

        #region PLAYABLE
        [HideInInspector]
        public List<NonCharacterEntity> NCEHits;
        [HideInInspector]
        public List<Cell> TargetedCells;
        [HideInInspector]
        public List<CharacterEntity> TargetEntities;
        [HideInInspector]
        public Spell ParentSpell;
        [HideInInspector]
        public SpellResult Result;

        public SpellCondition ConditionData;

        public bool ValidateConditions()
        {

            if (ParentSpell == null || ConditionData == null)
                return true;

            return this.ConditionData.Check(ParentSpell.Result);
        }

        public override void ExecuteAction()
        {
            this.RefEntity.ApplyStat(EntityStatistics.Mana, -this.Cost);

            if (!this.ValidateConditions())
            {
                EndAction();
                return;
            }
            else
            {
                this.TargetEntities = this.GetTargets(this.TargetCell);

                this.Result = new SpellResult();
                this.Result.Setup(this.TargetEntities, this);
            }
        }

        public List<CharacterEntity> GetTargets(Cell cellTarget)
        {
            if (this.Data.RequiresTargetting)
            {
                TargetedCells = GridUtility.TransposeShapeToCells(ref Data.RotatedShapeMatrix, cellTarget, Data.RotatedShapePosition);
                NCEHits = TargetedCells                   
                    .Where(cell => cell.AttachedNCE != null)
                    .Select(cell => cell.AttachedNCE)
                    .ToList();
                return TargetedCells                   
                    .Where(cell => cell.EntityIn != null)
                    .Select(cell => cell.EntityIn)
                    .ToList();
            }
            else if(this.ConditionData != null)
            {
                return this.ConditionData.GetValidatedTargets();
            }
            else
            {
                return new List<CharacterEntity> { this.ParentSpell == null ? this.RefEntity : this.ParentSpell.RefEntity };
            }
        }

        public override object[] GetDatas()
        {
            // temporary
            return new object[0];
        }

        public override void SetDatas(object[] Datas)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}