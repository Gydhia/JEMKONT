using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Interactable", menuName = "DownBelow/ScriptableObject/Interactable", order = 2)]
public class InteractablePreset : SerializedScriptableObject
{
    [ReadOnly]
    public Guid UID;
    [OnValueChanged("_updateUID")]
    public string UName;

    public Outline ObjectPrefab;

    private void _updateUID()
    {
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(UName));
            this.UID = new Guid(hash);
        }
    }
}
