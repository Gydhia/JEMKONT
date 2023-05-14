using DownBelow.GridSystem;
using DownBelow.Spells;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;


[ShowOdinSerializedPropertiesInInspector, NodeWidth(250)]
public class U_Shape_Node : Node
{
    [Output(ShowBackingValue.Never, dynamicPortList = false, connectionType = ConnectionType.Multiple)]
    public bool[,] ShapeMatrix;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(ShapeMatrix))
        {
            return this.SpellShapeMatrix;
        }
        return base.GetValue(port);
    }

    [TableMatrix(DrawElementMethod = "_processDrawShape", SquareCells = true, ResizableColumns = false, HorizontalTitle = nameof(SpellShapeMatrix)), OdinSerialize]
    [OnValueChanged("_updateShape")]
    public bool[,] SpellShapeMatrix = new bool[5, 5];


    public static Color AnchorColor = new(.584313725f, .741176471f, 1f, 1f);
    public static Color SelectedColor = new(.874509804f, 1f, .847058824f, 1f);
    public static Color NotSelectedColor = new(0f, 0f, 0f, 0.5f);


    [Button("Rotate Shape 90°")]
    public void RotateSpellShape() { this.SpellShapeMatrix = GridUtility.RotateSpellMatrix(this.SpellShapeMatrix, 90); }

    [Button]
    public void RegenerateShape() { SpellShapeMatrix = new bool[5, 5]; ShapePosition = new Vector2(2, 2); }

    [SerializeField, HideInInspector]
    public Vector2 ShapePosition = new Vector2(2, 2);

    [HideInInspector]
    public Vector2 RotatedShapePosition;

#if UNITY_EDITOR
    private void _updateShape() => this._updateMatrixShape(ref SpellShapeMatrix, ref ShapePosition);

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

    private bool _processDrawShape(Rect rect, bool value, int x, int y)
    {
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            value = !value;
            GUI.changed = true;
            Event.current.Use();
        }
        return _drawCell(rect, value, x, y, SpellShapeMatrix.GetLength(0), SpellShapeMatrix.GetLength(1), ShapePosition);
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
        SpellShapeMatrix ??= new bool[5, 5];
    }

#endif
}