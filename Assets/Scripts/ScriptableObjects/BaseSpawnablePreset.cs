using DownBelow.GridSystem;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BaseSpawnablePreset : SerializedScriptableObject, IEquatable<BaseSpawnablePreset>, IEqualityComparer<BaseSpawnablePreset>, IComparable<BaseSpawnablePreset>
{
    [ReadOnly]
    [FoldoutGroup("BASE")]
    public Guid UID;
    [OnValueChanged("_updateUID")]
    [FoldoutGroup("BASE")]
    public string UName;

    [FoldoutGroup("BASE")]
    public CellState AffectingState;
    [FoldoutGroup("BASE"), Tooltip("If this spawnable isn't made to be saved, but to remain the same as when it started ; Tools or Spawn")]
    public bool OverrideSave = false;
    private void _updateUID()
    {
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(UName));
            this.UID = new Guid(hash);
        }
    }

    public virtual void Init(Cell attachedCell)
    {
        attachedCell.Datas.state = this.AffectingState;
    }

    public bool Equals(BaseSpawnablePreset other)
    {
        return other == null ? false : this.UID == other.UID;
    }
    public bool Equals(BaseSpawnablePreset x, BaseSpawnablePreset y)
    {
        return (x == null || y == null) ? false : x.UID == y.UID;
    }

    public int CompareTo(BaseSpawnablePreset other)
    {
        if (other == null)
            return this.UID.CompareTo(null);

        return this.UID.CompareTo(other.UID);
    }

    public int GetHashCode(BaseSpawnablePreset obj)
    {
        return obj.UName.GetHashCode();
    }
}
