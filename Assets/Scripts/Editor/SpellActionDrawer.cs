using DownBelow.Entity;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpellActionDrawer : OdinValueDrawer<SpellAction>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        Rect rect = EditorGUILayout.GetControlRect();
        if (label != null)
        {
            rect = EditorGUI.PrefixLabel(rect, label);
        }
        SpellAction value = this.ValueEntry.SmartValue;
        GUIHelper.PushLabelWidth(20);
        GUIHelper.PopLabelWidth();

        this.ValueEntry.SmartValue = value;
    }
}