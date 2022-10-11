
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Jemkont.Entity;

[CreateAssetMenu(menuName = "Entity/EntitySpawn")]
public class EntitySpawn : SerializedScriptableObject
{
    [ReadOnly]
    public Guid UID;

    [OnValueChanged("_updateUID")]
    public string UName;

    private void _updateUID()
    {
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(UName));
            this.UID = new Guid(hash);
        }
    }

    public CharacterEntity Entity;
}
