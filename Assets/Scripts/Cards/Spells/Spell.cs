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
            Debug.LogWarning($"Executing spell {SettingsManager.Instance.ScriptableCards[this.SpellHeader.RefCard].Title}");
            int cost = SettingsManager.Instance.ScriptableCards[SpellHeader.RefCard].Cost;
            if (this.ParentSpell == null && this.RefEntity.Mana - cost > 0)
            {
                this.RefEntity.ApplyStat(EntityStatistics.Mana, -SettingsManager.Instance.ScriptableCards[SpellHeader.RefCard].Cost);
            }

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
            Result?.Unsubscribe();
            base.EndAction();
        }

        public List<CharacterEntity> GetTargets(Cell cellTarget)
        {
            if (Data.SpellResultTargeting)
            {
                var spell = GetSpellFromIndex(Data.SpellResultIndex);
                TargetEntities = spell.TargetEntities;
                TargetedCells = spell.TargetedCells;
                return TargetEntities;
            } else if (this.Data.RequiresTargetting)
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
                List<Cell> TargetCellsToTranspose = null;
                switch (this.Data.TargetType)
                {
                    case ETargetType.Ally:
                    case ETargetType.AllAllies:
                        TargetCellsToTranspose.AddRange(CombatManager.Instance.PlayingEntities.FindAll(x => x.IsAlly).Select(x => x.EntityCell).ToList());
                        break;
                    case ETargetType.Self:
                        TargetCellsToTranspose.Add(this.ParentSpell == null ? this.RefEntity.EntityCell : this.ParentSpell.RefEntity.EntityCell);
                        break;
                    case ETargetType.Enemy:
                    case ETargetType.AllEnemies:
                        TargetCellsToTranspose.AddRange(CombatManager.Instance.PlayingEntities.FindAll(x => !x.IsAlly).Select(x => x.EntityCell).ToList());
                        break;
                    case ETargetType.Empty:
                        //(en vrai jpense jfais tout péter dans le doute c'est bien)
                        throw new Exception($"The spell {this.GetType()} of the card {SettingsManager.Instance.ScriptableCards[this.SpellHeader.RefCard].name} is set to a targetting type of 'Empty' but has no targetting. This is not allowed.");
                        //Lisez et comprenez cette ligne avant de me pinger, pégus.
                    case ETargetType.NCEs:
                        TargetCellsToTranspose.AddRange(CombatManager.Instance.NCEs.Select(x => x.AttachedCell));
                        break;
                    case ETargetType.CharacterEntities:
                        TargetCellsToTranspose.AddRange(CombatManager.Instance.PlayingEntities.Select(x=>x.EntityCell));
                        break; 

                    case ETargetType.Entities:
                        TargetCellsToTranspose.AddRange(CombatManager.Instance.NCEs.Select(x => x.AttachedCell).ToList());
                        TargetCellsToTranspose.AddRange(CombatManager.Instance.PlayingEntities.Select(x => x.EntityCell).ToList());
                        break;
                    case ETargetType.All:
                        var cells = CombatManager.Instance.PlayingEntities[0].CurrentGrid.Cells;
                        for (int col = 0;col < cells.GetLength(0);col++)
                        {
                            for (int row = 0;row < cells.GetLength(1);row++)
                            {
                                TargetCellsToTranspose.Add(cells[col, row]);
                            }
                        }
                        break;
                    default:
                        TargetCellsToTranspose.Add(this.ParentSpell == null ? this.RefEntity.EntityCell : this.ParentSpell.RefEntity.EntityCell);
                        break;
                }
                foreach (var item in TargetCellsToTranspose)
                {
                    TargetedCells.AddRange(GridUtility.TransposeShapeToCells(ref Data.RotatedShapeMatrix, item, Data.RotatedShapePosition));
                }
                TargetEntities.AddRange(TargetedCells.FindAll(x => x.EntityIn != null).Select(x => x.EntityIn));
                return TargetEntities;
            }
        }

        Spell GetSpellFromIndex(int index)
        {
            return SettingsManager.Instance.ScriptableCards[this.SpellHeader.RefCard].Spells[index];
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