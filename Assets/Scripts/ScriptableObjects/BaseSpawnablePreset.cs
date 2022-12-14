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
    public Guid UID;
    [OnValueChanged("_updateUID")]
    public string UName;

    public CellState AffectingState;

    //public string prefab;

    //[PropertyOrder(3)]
    //[ShowInInspector, OdinSerialize]
    //[PropertyTooltip("Path to the prefab asset")]
    //[AssetSelector(Paths = "Assets/Resources/Prefabs")]
    //[AssetsOnly]
    //[OnValueChanged("_onPrefabinternalChanged")]
    //[InlineEditor(InlineEditorModes.LargePreview)]
    //private GameObject _prefab;

    private void _updateUID()
    {
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(UName));
            this.UID = new Guid(hash);
        }
    }

    //private void _onPrefabinternalChanged()
    //{
    //    if (this._prefab != null)
    //    {
    //        this.prefab = AssetDatabase.GetAssetPath(this._prefab);
    //        if (this.prefab != null)
    //        {
    //            this.prefab = this.prefab.Replace(".prefab", "").ToLower();
    //            this.prefab = this.prefab.Replace("assets/resources/", "");
    //        }
    //    }
    //    else
    //    {
    //        this.prefab = "";
    //    }
    //    EditorUtility.SetDirty(this);
    //}

    public virtual void Init(Cell attachedCell)
    {
        Debug.LogError("Init hasn't been overrided");
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
