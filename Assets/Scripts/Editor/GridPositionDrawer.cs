using DownBelow.Managers;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPositionDrawer : OdinValueDrawer<GridPosition>
{
    private string display;

    protected override void DrawPropertyLayout(GUIContent label)
    {
        display = this.ValueEntry.SmartValue.latitude + " | " + this.ValueEntry.SmartValue.longitude;
        display = SirenixEditorFields.TextField(display);
    }
}
