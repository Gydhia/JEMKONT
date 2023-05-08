using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Spells;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
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

        NCEs = 1 << 4,

        CharacterEntities = Enemy | Ally,

        Entities = Enemy | Ally | NCEs,
        All = Enemy | Ally | Empty
    }

    public static class TargetTypeHelper
    {
        public static bool ValidateTarget(this TargetType value, Cell cell)
        {
            return value switch
            {
                TargetType.Self => cell.EntityIn == GameManager.Instance.SelfPlayer,
                TargetType.Enemy => cell.EntityIn != null && cell.EntityIn is EnemyEntity,
                TargetType.Ally => cell.EntityIn != null && cell.EntityIn is PlayerBehavior,
                TargetType.Empty => cell.Datas.state == CellState.Walkable,
                TargetType.CharacterEntities => cell.Datas.state == CellState.EntityIn && cell.EntityIn is CharacterEntity,
                TargetType.Entities => cell.Datas.state == CellState.EntityIn,
                TargetType.NCEs => cell.hasNCE,
                TargetType.All => cell.Datas.state != CellState.Blocked,
                _ => true,
            };
        }
    }

    public class SpellData
    {
        public static Color AnchorColor = new(.584313725f, .741176471f, 1f, 1f);
        public static Color SelectedColor = new(.874509804f, 1f, .847058824f, 1f);
        public static Color NotSelectedColor = new(0f, 0f, 0f, 0.5f);

        public void Refresh()
        {
            if (this.TargetType == TargetType.Self)
                this.CastingMatrix = new bool[1, 1] { { true } };

            this.RotatedShapeMatrix = this.SpellShapeMatrix;
            this.RotatedShapePosition = this.ShapePosition;
        }

        // TODO : define it  according to casting matrix and target type later ?
        public bool RequiresTargetting = true;

        [Button("Rotate Shape 90°"), FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/Rotation", Width = 0.5f, Order = -1, MaxWidth = 200)]
        public void RotateSpellShape() { this.SpellShapeMatrix = GridUtility.RotateSpellMatrix(this.SpellShapeMatrix, 90); }

        [Button, FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/Buttons", Width = 0.5f, Order = 0, MaxWidth = 200)]
        public void RegenerateShape() { SpellShapeMatrix = new bool[5, 5]; ShapePosition = new Vector2(2, 2); }

        [Button, FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/Buttons", Width = 0.5f, Order = 0, MaxWidth = 200)]
        public void RegenerateCasting() { CastingMatrix = new bool[5, 5]; CasterPosition = new Vector2(2, 2); }

        [TableMatrix(DrawElementMethod = "_processDrawSpellShape", SquareCells = true, ResizableColumns = false, HorizontalTitle = nameof(SpellShapeMatrix)), OdinSerialize]
        [FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/Grids", Width = 0.5f, Order = 1, MaxWidth = 200)]
        [OnValueChanged("_updateSpellShape")]
        public bool[,] SpellShapeMatrix = new bool[5, 5];
        [SerializeField]
        public Vector2 ShapePosition = new Vector2(2, 2);

        // Used in run-time to store the rotated shape from the base shape
        [HideInInspector]
        public bool[,] RotatedShapeMatrix;
        [HideInInspector]
        public Vector2 RotatedShapePosition;

        [TableMatrix(DrawElementMethod = "_processDrawSpellCasting", SquareCells = true, ResizableColumns = false, HorizontalTitle = nameof(CastingMatrix)), OdinSerialize]
        [FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/Grids", Width = 0.5f, Order = 1, MaxWidth = 200)]
        [OnValueChanged("_updateCastingShape")]
        [HideIf("TargetType", TargetType.Self)]
        public bool[,] CastingMatrix;
        [SerializeField]
        public Vector2 CasterPosition = new Vector2(2, 2);

        [ShowIf("@this.SpellShapeMatrix != null")]
        [FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/RotationValue", Width = 0.5f, Order = 2, MaxWidth = 100)]
        public bool RotateShapeWithCast = false;

        [FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/TargetType", Order = 3)]
        public TargetType TargetType;


#if UNITY_EDITOR
        private void _updateSpellShape() => this._updateMatrixShape(ref SpellShapeMatrix, ref ShapePosition);
        private void _updateCastingShape() => this._updateMatrixShape(ref CastingMatrix, ref CasterPosition);

        private void _updateMatrixShape(ref bool[,] array, ref Vector2 position)
        {
            int cols = array.GetLength(0);
            int rows = array.GetLength(1);

            bool addedEdge = false;
            for (int i = 0;i < rows;i++)
            {
                // Top edge
                if (array[0, i])
                {
                    array = ArrayHelper.InsertEmptyRow(array, 0);
                    position.x++;
                    addedEdge = true;
                    break;
                }
                // Bottom edge
                else if (array[cols - 1, i])
                {
                    array = ArrayHelper.ResizeArrayTwo(array, array.GetLength(0) + 1, array.GetLength(1), false);
                    addedEdge = true;
                    break;
                }
            }

            if (!addedEdge && rows > 5)
            {
                bool anyTop = false;
                bool anyBot = false;
                for (int i = 0;i < cols;i++)
                {
                    if (array[i, 1]) anyTop = true;
                    if (array[i, rows - 2]) anyBot = true;
                }
                if (!anyBot) { array = ArrayHelper.RemoveColumn(array, rows - 1); rows--; }
                if (!anyTop) { array = ArrayHelper.RemoveColumn(array, 0); position.y--; rows--; }
            }

            addedEdge = false;
            for (int i = 1;i < cols - 1;i++)
            {
                // Left edge
                if (array[i, 0])
                {
                    array = ArrayHelper.InsertEmptyColumn(array, 0);
                    position.y++;
                    addedEdge = true;
                    break;
                }
                // Right edge
                if (array[i, rows - 1])
                {
                    array = ArrayHelper.ResizeArrayTwo(array, array.GetLength(0), array.GetLength(1) + 1, false);
                    addedEdge = true;
                    break;
                }
            }

            if (!addedEdge && cols > 5)
            {
                bool anyLeft = false;
                bool anyRight = false;
                for (int i = 0;i < rows;i++)
                {
                    if (array[1, i]) anyLeft = true;
                    if (array[cols - 2, i]) anyRight = true;
                }
                if (!anyRight) { array = ArrayHelper.RemoveRow(array, cols - 1); }
                if (!anyLeft) { array = ArrayHelper.RemoveRow(array, 0); position.x--; }
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
            return _drawCell(rect, value, x, y, SpellShapeMatrix.GetLength(0), SpellShapeMatrix.GetLength(1), ShapePosition);
        }
        private bool _processDrawSpellCasting(Rect rect, bool value, int x, int y)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                GUI.changed = true;
                Event.current.Use();
            }
            return _drawCell(rect, value, x, y, CastingMatrix.GetLength(0), CastingMatrix.GetLength(1), CasterPosition);
        }
        private static bool _drawCell(Rect rect, bool value, int x, int y, int width, int height, Vector2 basePos)
        {
            Color color = value ? SelectedColor : NotSelectedColor;
            if (x == basePos.x && y == basePos.y)
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
            } else
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
    }
}