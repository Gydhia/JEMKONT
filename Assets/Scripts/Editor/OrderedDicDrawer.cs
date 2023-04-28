using DownBelow.Managers;
using mattmc3.dotmore.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderedDicDrawer : OdinValueDrawer<OrderedDictionary<GridPosition, BaseSpawnablePreset>>
{
    bool delete;
    int index;
    Object obj;

    protected override void DrawPropertyLayout(GUIContent label)
    {

        for (int i = 0; i < this.ValueEntry.SmartValue.Count; i++)
        {
            GUILayout.BeginHorizontal();
            SirenixEditorFields.IntField(i, GUILayout.Width(60));
            obj = SirenixEditorFields.UnityObjectField(this.ValueEntry.SmartValue[i], typeof(BaseSpawnablePreset), false);
            delete = GUILayout.Button("x", GUILayout.Width(30));
            GUILayout.EndHorizontal();

            if (obj != this.ValueEntry.SmartValue[i])
                this.ValueEntry.SmartValue[i] = obj as BaseSpawnablePreset;
            if (delete)
                this.ValueEntry.SmartValue.RemoveAt(i);
        }
    }
}
