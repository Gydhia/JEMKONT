using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName ="DEV/Method Holders/ToolsEvents")]
public class ToolsEvents : ScriptableObject
{
    public void CutDownTree() {
        //chercher l'arbre s�l�ctionn�
        //
    }
    public void CutDownTree(GameObject tree) {
        //chercher l'arbre s�l�ctionn�
        //
        tree.SetActive(false);

    }
}
