using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "EditorGridData", menuName = "DownBelow/Editor/EditorGridData", order = 0)]
public class GridDataScriptableObject : SerializedBigDataScriptableObject<EditorGridData>
{
}
