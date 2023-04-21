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

        protected Spell(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell) : base(CopyData, RefEntity, TargetCell, ParentSpell)
        {
        }
    }

    public abstract class Spell : EntityAction
    {
        public Spell(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell)
           : base(RefEntity, TargetCell)
        {
            this.Data = CopyData;
            this.Data.RotatedShapeMatrix = this.Data.SpellShapeMatrix;
            this.Data.RotatedShapePosition = this.Data.ShapePosition;
            this.ParentSpell = ParentSpell;
        }

        public SpellData Data;

        #region PLAYABLE
        [HideInInspector]
        public List<Cell> TargetCells;
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

        public override async void ExecuteAction()
        {
            if (!this.ValidateConditions())
            {
                EndAction();
                return;
            }
            if (Data.ProjectileSFX != null)
            {
                await SFXManager.Instance.DOSFX(Data.ProjectileSFX, RefEntity, TargetCell, this);
            }
            if (Data.CellSFX != null && TargetCells != null && TargetCells.Count != 0)
            {
                for (int i = 0;i < TargetCells.Count;i++)
                {
                    Cell cell = this.TargetCells[i];
                    if (cell != this.TargetCells[^1])
                        SFXManager.Instance.DOSFX(Data.CellSFX, RefEntity, TargetCell, this);
                    else
                        await SFXManager.Instance.DOSFX(Data.CellSFX, RefEntity, TargetCell, this);
                    //Not awaiting since we want to do it all
                }
            }
        }

        public List<CharacterEntity> GetTargets(Cell cellTarget)
        {
            return GridUtility
                .TransposeShapeToCells(ref Data.SpellShapeMatrix, cellTarget, Data.ShapePosition)
                .Where(cell => cell.EntityIn != null)
                .Select(cell => cell.EntityIn)
                .ToList();
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