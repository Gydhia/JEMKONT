using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Mechanics;
using Newtonsoft.Json;
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

        protected Spell(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData)
            : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }
    }

    public abstract class Spell : EntityAction
    {
        public Spell(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData)
           : base(RefEntity, TargetCell)
        {
            this.Data = CopyData;
            this.Data.Refresh();
            this.ParentSpell = ParentSpell;
            this.ConditionData = ConditionData;
        }

        public SpellData Data;
        [HideInInspector]
        public SpellHeader SpellHeader;

        #region PLAYABLE
        [HideInInspector, JsonIgnore]
        public List<NonCharacterEntity> NCEHits;
        [HideInInspector, JsonIgnore]
        public List<Cell> TargetedCells;
        [HideInInspector, JsonIgnore]
        public List<CharacterEntity> TargetEntities;
        [HideInInspector, JsonIgnore]
        public Spell ParentSpell;
        [HideInInspector, JsonIgnore]
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
            this.RefEntity.ApplyStat(EntityStatistics.Mana, -CardsManager.Instance.ScriptableCards[SpellHeader.RefCard].Cost);

            if (!this.ValidateConditions())
            {
                EndAction();
                return;
            } else
            {
                this.TargetEntities = this.GetTargets(this.TargetCell);

                this.Result = new SpellResult();
                this.Result.Setup(this.TargetEntities, this);
                if (Data.ProjectileSFX != null)
                {
                    await SFXManager.Instance.DOSFX(new RuntimeSFXData(Data.ProjectileSFX, RefEntity, TargetCell, this));
                }
                if (Data.CellSFX != null && TargetedCells != null && TargetedCells.Count != 0)
                {
                    for (int i = 0;i < TargetedCells.Count;i++)
                    {
                        var targetedCell = this.TargetedCells[i];
                        if (i != TargetedCells.Count)
                            //Not awaiting since we want to do it all. Suggestion could be to wait 0.05s to have some kind of wave effect.
                            SFXManager.Instance.DOSFX(new(Data.CellSFX, RefEntity, targetedCell, this));
                        else
                            await SFXManager.Instance.DOSFX(new(Data.CellSFX, RefEntity, targetedCell, this));
                    }
                }
            }
        }

        public override void EndAction()
        {
            Result.Unsubribirse();
            base.EndAction();
        }

        public List<CharacterEntity> GetTargets(Cell cellTarget)
        {
            if (Data.SpellResultTargeting)
            {
                TargetEntities.AddRange(GetSpellFromIndex(Data.SpellResultIndex).TargetEntities);
            }
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
            } else if (this.ConditionData != null)
            {
                return this.ConditionData.GetValidatedTargets();
            } else
            {
                return new List<CharacterEntity> { this.ParentSpell == null ? this.RefEntity : this.ParentSpell.RefEntity };
            }
        }
        /// <summary>
        /// Recursive method going into the parent spell to get the index.
        /// DO NOT USE THE PARAMETER, it is meant to be used only with the recursive.
        /// </summary>
        /// <param name="start">The recursive parameter, DO NOT USE</param>
        /// <returns></returns>
        int GetSpellIndex(int start = 0)
        {
            int res = start;
            if (ParentSpell != null)
            {
                res = ParentSpell.GetSpellIndex(start + 1);
            }
            return res;
        }

        Spell GetSpellFromIndex(int index)
        {
            int thisIndex = GetSpellIndex();
            if (index > thisIndex)
            {
                return null;
            } else if (index == thisIndex)
            {
                return this;
            } else
            {
                if(ParentSpell.GetSpellIndex() == index)
                {
                    Debug.Log($"Fetched the result of the spell {this}!");
                    return ParentSpell;
                } else
                {
                    return ParentSpell.GetSpellFromIndex(index);
                }
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