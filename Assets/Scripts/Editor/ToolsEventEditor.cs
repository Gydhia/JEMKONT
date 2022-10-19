using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(ToolsEvents))]
public class ToolsEventsEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		EditorGUILayout.HelpBox("Ce scriptable contient (et on peux ajouter) des m�thodes qui vont passer par l'impl�mentation singleton, pour apeller certaines m�thodes du joueur par exemple, ou d'une cellule (genre BreakTree(), etc).\nEn gros, ce scriptable ne sert qu'aux callbacks d'UnityActions ou des trucs du genre.",MessageType.Info);
	}
}

