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
using System.Threading.Tasks;
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

        public ScriptableCard RefCard => SettingsManager.Instance.ScriptableCards[SpellHeader.RefCard];

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
        public ConditionBase TargettingCondition;

        public bool ValidateConditions()
        {
            if (ParentSpell == null || ConditionData == null)
                return true;

            return this.ConditionData.Check(ParentSpell.Result);
        }

        public override async void ExecuteAction()
        {
            Debug.LogWarning($"Executing spell {RefCard.Title}");
            int cost = RefCard.Cost;
            if (this.ParentSpell == null)
            {
                this.RefEntity.ApplyStat(EntityStatistics.Mana, -cost);
            }

            if (!this.ValidateConditions())
            {
                EndAction();
                return;
            } else
            {
                this.TargetEntities = this.GetTargets(this.TargetCell);

                this.Result = new SpellResult();
                this.Result.Setup(this.TargetEntities, this);
                await DoSpellBehavior();
                EndAction();
            }
        }

        public virtual async Task DoSpellBehavior()
        {
            if (Data.ProjectileSFX != null)
            {
                if (Data.RequiresTargetting)
                {
                    await SFXManager.Instance.DOSFX(new RuntimeSFXData(Data.ProjectileSFX, RefEntity, TargetCell, this));
                }
                else if(Data.SpellResultTargeting){
                    await SFXManager.Instance.DOSFX(new RuntimeSFXData(Data.ProjectileSFX, RefEntity, GetSpellFromIndex(Data.SpellResultIndex).TargetCell, this));
                } else
                {
                    for (int i = 0;i < TargetEntities.Count;i++)
                    {
                        CharacterEntity item = TargetEntities[i];
                        if(i == TargetEntities.Count - 1)
                        {
                            await SFXManager.Instance.DOSFX(new RuntimeSFXData(Data.ProjectileSFX, RefEntity, item.EntityCell, this));
                        } else
                        {
                            SFXManager.Instance.DOSFX(new RuntimeSFXData(Data.ProjectileSFX, RefEntity, item.EntityCell, this));
                        }
                    }
                }
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

        public override void EndAction()
        {
            if (Result != null && Result.SetUp)
            {
                Result?.Unsubscribe();
            }
            base.EndAction();
        }

        public List<CharacterEntity> GetTargets(Cell cellTarget)
        {
            if (Data.SpellResultTargeting)
            {
                var spell = GetSpellFromIndex(Data.SpellResultIndex);
                TargetedCells = spell.TargetedCells;
            } else if (this.Data.RequiresTargetting)
            {
                TargetedCells = GridUtility.TransposeShapeToCells(ref Data.RotatedShapeMatrix, cellTarget, Data.RotatedShapePosition);
                NCEHits = TargetedCells
                    .FindAll(cell => cell.AttachedNCE != null)
                    .Select(cell => cell.AttachedNCE)
                    .ToList();
            } else
            {
                TargetedCells = new();
                List<Cell> TargetCellsToTranspose = new List<Cell>();
                if (this.Data.TargetType.HasFlag(ETargetType.Ally) || this.Data.TargetType.HasFlag(ETargetType.AllAllies))
                {
                    TargetCellsToTranspose.AddRange(CombatManager.Instance.PlayingEntities.FindAll(x => x.IsAlly).Select(x => x.EntityCell).ToList());
                }
                if (this.Data.TargetType.HasFlag(ETargetType.Self))
                {
                    TargetCellsToTranspose.Add(this.ParentSpell == null ? this.RefEntity.EntityCell : this.ParentSpell.RefEntity.EntityCell);
                }
                if (this.Data.TargetType.HasFlag(ETargetType.Enemy) || this.Data.TargetType.HasFlag(ETargetType.AllEnemies))
                {
                    TargetCellsToTranspose.AddRange(CombatManager.Instance.PlayingEntities.FindAll(x => !x.IsAlly).Select(x => x.EntityCell).ToList());
                }
                if (this.Data.TargetType.HasFlag(ETargetType.NCEs))
                {
                    TargetCellsToTranspose.AddRange(CombatManager.Instance.NCEs.Select(x => x.AttachedCell));
                }
                if (this.Data.TargetType.HasFlag(ETargetType.Entities))
                {
                    TargetCellsToTranspose.AddRange(CombatManager.Instance.NCEs.Select(x => x.AttachedCell).ToList());
                    TargetCellsToTranspose.AddRange(CombatManager.Instance.PlayingEntities.Select(x => x.EntityCell).ToList());
                }
                if (this.Data.TargetType.HasFlag(ETargetType.CharacterEntities))
                {
                    TargetCellsToTranspose.AddRange(CombatManager.Instance.PlayingEntities.Select(x => x.EntityCell));
                }
                if (this.Data.TargetType.HasFlag(ETargetType.All))
                {
                    var cells = CombatManager.Instance.PlayingEntities[0].CurrentGrid.Cells;
                    for (int col = 0;col < cells.GetLength(0);col++)
                    {
                        for (int row = 0;row < cells.GetLength(1);row++)
                        {
                            TargetCellsToTranspose.Add(cells[col, row]);
                        }
                    }
                }
                if (this.Data.TargetType.HasFlag(ETargetType.Empty))
                {
                    //(en vrai jpense jfais tout péter dans le doute c'est bien)
                    throw new Exception($"The spell {this.GetType()} of the card {RefCard.name} is set to a targetting type of 'Empty' but has no targetting. This is not allowed.");
                    //Lisez et comprenez cette ligne avant de me pinger, pégus.
                }
                //Removing duplicates
                TargetCellsToTranspose = TargetCellsToTranspose.GroupBy(x => x.PositionInGrid).Select(y => y.First()).ToList();
                foreach (Cell item in TargetCellsToTranspose)
                {
                    TargetedCells.AddRange(GridUtility.TransposeShapeToCells(ref Data.RotatedShapeMatrix, item, Data.RotatedShapePosition));
                }
            }
            if (this.TargettingCondition != null)
            {
                var realTargeted = new List<Cell>();
                realTargeted.AddRange(TargetedCells);
                TargetedCells.Clear();
                foreach (Cell cell in realTargeted)
                {
                    if (TargettingCondition.Validated(ParentSpell.Result, cell))
                    {
                        TargetedCells.Add(cell);
                    }
                }
            }
            TargetEntities = new();
            TargetEntities.AddRange(TargetedCells.FindAll(x => x.EntityIn != null).Select(x => x.EntityIn));

            Debug.Log($"{this.GetType()} spell targets : {TargetEntities.ArrayToString()}");
            return TargetEntities;
        }

        protected int Index()
        {
            return Array.IndexOf(RefCard.Spells, this);
        }

        protected Spell GetSpellFromIndex(int index)
        {
            return RefCard.Spells[index];
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