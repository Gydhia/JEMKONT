using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(ToolsEvents))]
public class ToolsEventsEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		EditorGUILayout.HelpBox("Ce scriptable contient (et on peux ajouter) des méthodes qui vont passer par l'implémentation singleton, pour apeller certaines méthodes du joueur par exemple, ou d'une cellule (genre BreakTree(), etc).\nEn gros, ce scriptable ne sert qu'aux callbacks d'UnityActions ou des trucs du genre.",MessageType.Info);
	}
}

