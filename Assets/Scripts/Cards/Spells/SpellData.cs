using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Mechanics;
using DownBelow.Spells;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEditor;
using UnityEngine;

namespace DownBelow.Spells
{
    [System.Flags]
    public enum ETargetType
    {
        None = 0,
        Self = 1 << 0,

        Enemy = 1 << 1,
        Ally = 1 << 2,

        Empty = 1 << 4,

        NCEs = 1 << 8,

        CharacterEntities = Enemy | Ally,

        Entities = Enemy | Ally | NCEs,
        All = Enemy | Ally | Empty
    }

    public static class TargetTypeHelper
    {
        public static bool ValidateTarget(this ETargetType value, Cell cell)
        {
            bool validated = true;
            if (value.HasFlag(ETargetType.None))
            {
                validated = false; //en mode MALVEILLANCE MAAAAAAAAAAX
            }
            if (value.HasFlag(ETargetType.Ally))
            {
                validated |= cell.EntityIn != null && cell.EntityIn.IsAlly;
            }
            if (value.HasFlag(ETargetType.Self))
            {
                validated |= cell.EntityIn != null && cell.EntityIn == CombatManager.CurrentPlayingEntity;
            }
            if (value.HasFlag(ETargetType.Enemy))
            {
                validated |= cell.EntityIn != null && !cell.EntityIn.IsAlly;
            }
            if (value.HasFlag(ETargetType.NCEs))
            {
                validated |= cell.AttachedNCE != null;
            }
            if (value.HasFlag(ETargetType.Entities))
            {
                validated |= cell.EntityIn != null || cell.AttachedNCE != null;
            }
            if (value.HasFlag(ETargetType.CharacterEntities))
            {
                validated |= cell.EntityIn != null;
            }
            if (value.HasFlag(ETargetType.All))
            {
                //Validated doesn't change!
            }
            if (value.HasFlag(ETargetType.Empty))
            {
                validated |= (cell.EntityIn == null && cell.AttachedNCE == null && cell.Datas.state.HasFlag(CellState.Walkable));
            }
            return validated;
        }
    }

    public class SpellData
    {
        public static Color AnchorColor = new(.584313725f, .741176471f, 1f, 1f);
        public static Color SelectedColor = new(.874509804f, 1f, .847058824f, 1f);
        public static Color NotSelectedColor = new(0f, 0f, 0f, 0.5f);

        public void Refresh()
        {
            if (this.TargetType == ETargetType.Self)
            {
                this.CastingMatrix = new bool[1, 1] { { true } };
                this.CasterPosition = Vector2.zero;
            }

            this.RotatedShapeMatrix = this.SpellShapeMatrix;
            this.RotatedShapePosition = this.ShapePosition;
        }
        public bool CanRetargetAlreadyTargettedCells = false;
        // TODO : define it  according to casting matrix and target type later ?
        [DisableIf(nameof(SpellResultTargeting)), InfoBox("@TargetTypeInfo()")]
        public bool RequiresTargetting = true;

        [Button("Rotate Shape 90°"), FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/Rotation", Width = 0.5f, Order = -1, MaxWidth = 200)]
        public void RotateSpellShape() { this.SpellShapeMatrix = GridUtility.RotateSpellMatrix(this.SpellShapeMatrix, 90); }

        [HideIf("@TargetType == ETargetType.Self || !RequiresTargetting")]
        [Button("Generate Classic Casting"), FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/Rotation", Width = 0.5f, Order = 0, MaxWidth = 200)]
        public void GenerateClassicCasting()
        {
            this.CastingMatrix = new bool[7, 7];
            for (int i = 0; i < 7 * 7; i++)
            {
                CastingMatrix[i % 7, i / 7] = true;
            }
            CastingMatrix[3, 3] = false;
            CasterPosition = new Vector2(3, 3);
        }
        [Button, FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/Buttons", Width = 0.5f, Order = 0, MaxWidth = 200)]
        public void RegenerateShape() { SpellShapeMatrix = new bool[5, 5]; ShapePosition = new Vector2(2, 2); }

        [HideIf("@TargetType == ETargetType.Self || !RequiresTargetting")]
        [Button, FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/Buttons", Width = 0.5f, Order = 0, MaxWidth = 200)]
        public void RegenerateCasting() { CastingMatrix = new bool[5, 5]; CasterPosition = new Vector2(2, 2); }

        private string TargetTypeInfo()
        {
            if (SpellResultTargeting)
            {
                return $"This spell will take the same target that the spell n°{SpellResultIndex.ToString()} on this card.";
            }
            else if (RequiresTargetting)
            {
                return $"This spell will be castable on {TargetType.ToString()} cells.";
            }
            else
            {
                return $"This spell will target ALL OF THE {TargetType.ToString()} on the grid.";
            }
        }
        [DisableIf("@this.RequiresTargetting")]
        [HorizontalGroup("SpellResult", Width = 0.5f, Order = 0)]
        public bool SpellResultTargeting;
        [InfoBox("If this is on, the targets of the result of the spell you want can be added to the targets of this spell, i.e: Damage, then break the defense of the same ones.\\/")]
        [Min(0), ShowIf(nameof(SpellResultTargeting)), HorizontalGroup("SpellResult", Width = 0.5f, Order = 0)]
        public int SpellResultIndex;

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
        [HideIf("@TargetType == ETargetType.Self || !RequiresTargetting")]
        public bool[,] CastingMatrix;
        [SerializeField, HideIf("@TargetType == ETargetType.Self || !RequiresTargetting")]
        public Vector2 CasterPosition = new Vector2(2, 2);

        [ShowIf("@this.SpellShapeMatrix != null")]
        [FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/RotationValue", Width = 0.5f, Order = 2, MaxWidth = 100)]
        public bool RotateShapeWithCast = false;

        [HideIf("@this.SpellResultTargeting")]
        [FoldoutGroup("Spell Targeting"), HorizontalGroup("Spell Targeting/TargetType", Order = 3)]
        public ETargetType TargetType;

        public ScriptableSFX ProjectileSFX;
        public ScriptableSFX CellSFX;

#if UNITY_EDITOR
        private void _updateSpellShape() => this._updateMatrixShape(ref SpellShapeMatrix, ref ShapePosition);
        private void _updateCastingShape() => this._updateMatrixShape(ref CastingMatrix, ref CasterPosition);

        private void _updateMatrixShape(ref bool[,] array, ref Vector2 position)
        {
            int cols = array.GetLength(0);
            int rows = array.GetLength(1);

            bool addedEdge = false;
            for (int i = 0; i < rows; i++)
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
                for (int i = 0; i < cols; i++)
                {
                    if (array[i, 1]) anyTop = true;
                    if (array[i, rows - 2]) anyBot = true;
                }
                if (!anyBot) { array = ArrayHelper.RemoveColumn(array, rows - 1); rows--; }
                if (!anyTop) { array = ArrayHelper.RemoveColumn(array, 0); position.y--; rows--; }
            }

            addedEdge = false;
            for (int i = 1; i < cols - 1; i++)
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
                for (int i = 0; i < rows; i++)
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
    }
}