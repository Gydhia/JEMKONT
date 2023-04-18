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
    [System.Flags]
    public enum TargetType
    {
        Self = 0,

        Enemy = 1 << 0,
        Ally = 1 << 1,

        Empty = 1 << 2,

        Entities = Enemy | Ally,
        All = Enemy | Ally | Empty
    }

    public static class TargetTypeHelper
    {
        public static bool ValidateTarget(this TargetType value, Cell cell)
        {
            switch (value)
            {
                case TargetType.Self: return cell.EntityIn == GameManager.Instance.SelfPlayer;
                case TargetType.Enemy: return cell.EntityIn != null && cell.EntityIn is EnemyEntity;
                case TargetType.Ally: return cell.EntityIn != null && cell.EntityIn is PlayerBehavior;
                case TargetType.Empty: return cell.Datas.state == CellState.Walkable;
                case TargetType.Entities: return cell.Datas.state == CellState.EntityIn;
                case TargetType.All: return cell.Datas.state != CellState.Blocked;
            }

            return true;
        }
    }

    public class Spell : EntityAction
    {
        public Spell(CharacterEntity RefEntity, Cell TargetCell, SpellAction ActionData, SpellCondition ConditionData = null)
           : base(RefEntity, TargetCell)
        {
            this.ActionData = ActionData;
            this.ConditionData = ConditionData;
        }

        #region DATA_FAT_INSPECTOR

        public static Color AnchorColor = new(.584313725f, .741176471f, 1f, 1f);
        public static Color SelectedColor = new(.874509804f, 1f, .847058824f, 1f);
        public static Color NotSelectedColor = new(0f, 0f, 0f, 0.5f);

        // TODO : define it  according to casting matrix and target type later ?
        public bool RequiresTargetting = true;

        [Button("Rotate Shape 90°"), FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/Rotation", Width = 0.5f, Order = -1, MaxWidth = 200)]
        public void RotateSpellShape() { this.SpellShapeMatrix = GridUtility.RotateSpellMatrix(this.SpellShapeMatrix, 90); }

        [Button, FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/Buttons", Width = 0.5f, Order = 0, MaxWidth = 200)]
        public void RegenerateShape() { SpellShapeMatrix = new bool[5, 5]; }

        [Button, FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/Buttons", Width = 0.5f, Order = 0, MaxWidth = 200)]
        public void RegenerateCasting() { CastingMatrix = new bool[5, 5]; }

        [TableMatrix(DrawElementMethod = "_processDrawSpellShape", SquareCells = true, ResizableColumns = false, HorizontalTitle = nameof(SpellShapeMatrix)), OdinSerialize]
        [FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/Grids", Width = 0.5f, Order = 1, MaxWidth = 200)]
        [OnValueChanged("_updateSpellShape")]
        public bool[,] SpellShapeMatrix = new bool[5, 5];
        protected Vector2 SpellPosition = new Vector2(2, 2);

        [TableMatrix(DrawElementMethod = "_processDrawSpellCasting", SquareCells = true, ResizableColumns = false, HorizontalTitle = nameof(CastingMatrix)), OdinSerialize]
        [FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/Grids", Width = 0.5f, Order = 1, MaxWidth = 200)]
        [OnValueChanged("_updateCastingShape")]
        public bool[,] CastingMatrix;
        protected Vector2 CasterPosition = new Vector2(2, 2);

        [ShowIf("@this.SpellShapeMatrix != null")]
        [FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/RotationValue", Width = 0.5f, Order = 2, MaxWidth = 100)]
        public bool RotateShapeWithCast = false;

        [FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/TargetType", Order = 3)]
        public TargetType TargetType;


#if UNITY_EDITOR
        private void _updateSpellShape() => this._updateMatrixShape(ref SpellShapeMatrix);
        private void _updateCastingShape() => this._updateMatrixShape(ref CastingMatrix);

        private void _updateMatrixShape(ref bool[,] array)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);

            for (int i = 0; i < cols; i++)
            {
                // Top edge
                if (array[0, i])
                {
                    SpellShapeMatrix = ArrayHelper.InsertEmptyRow(SpellShapeMatrix, 0);
                    break;
                }
                // Bottom edge
                else if (array[rows - 1, i])
                {
                    SpellShapeMatrix = ArrayHelper.ResizeArrayTwo(SpellShapeMatrix, SpellShapeMatrix.GetLength(0) + 1, SpellShapeMatrix.GetLength(1), false);
                    break;
                }
            }

            for (int i = 1; i < rows - 1; i++)
            {
                // Left edge
                if (array[i, 0])
                {
                    SpellShapeMatrix = ArrayHelper.InsertEmptyColumn(array, 0);
                    break;
                }
                // Right edge
                if (array[i, cols - 1])
                {
                    SpellShapeMatrix = ArrayHelper.ResizeArrayTwo(SpellShapeMatrix, SpellShapeMatrix.GetLength(0), SpellShapeMatrix.GetLength(1) + 1, false);
                    break;
                }
            }
        }

        private bool _processDrawSpellShape(Rect rect, bool value, int x, int y)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                GUI.changed = true;
                Event.current.Use();
            }
            return _drawCell(rect, value, x, y, SpellShapeMatrix.GetLength(0), SpellShapeMatrix.GetLength(1));
        }
        private bool _processDrawSpellCasting(Rect rect, bool value, int x, int y)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                GUI.changed = true;
                Event.current.Use();
            }
            return _drawCell(rect, value, x, y, CastingMatrix.GetLength(0), CastingMatrix.GetLength(1));
        }
        private static bool _drawCell(Rect rect, bool value, int x, int y, int width, int height)
        {
            Color color = value ? SelectedColor : NotSelectedColor;
            if (x == width/ 2  && y == height / 2 )
            {
                EditorGUI.DrawRect(
                    rect.Padding(1),
                    AnchorColor
                );
                EditorGUI.DrawRect(
                    new Rect(rect)
                    {
                        size = rect.size / 2,
                        x = rect.x + (rect.size.x / 4),
                        y = rect.y + (rect.size.y / 4)
                    }, color);
                //Achor cell
            }
            else
            {
                EditorGUI.DrawRect(
                    rect.Padding(1),
                    color
                );
            }
            return value;
        }

        [OnInspectorInit]
        private void InitData()
        {
            CastingMatrix ??= new bool[5, 5];
            SpellShapeMatrix ??= new bool[5, 5];
        }

#endif
        #endregion

        #region PLAYABLE
        [HideInInspector]
        public List<Cell> TargetCells;
        [HideInInspector]
        public Spell ParentSpell;
        public SpellCondition ConditionData;
        public SpellAction ActionData;

        // The instantiated spell
        [HideInInspector]
        public SpellAction CurrentAction;
        [HideInInspector]
        public CharacterEntity Caster;

        public void Init(Spell parentSpell)
        {
            this.ParentSpell = parentSpell;
        }

        public override void ExecuteAction()
        {
            Debug.Log("Executed spell to " + this.TargetCell.ToString());
            EndAction();
        }

        public List<CharacterEntity> GetTargets(CharacterEntity caster, GridSystem.Cell cellTarget)
        {
            return GridUtility
                .TransposeShapeToCell(ref this.SpellShapeMatrix, cellTarget)
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