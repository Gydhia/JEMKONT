using DownBelow.Mechanics;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ECollection { Common, Miner, Herbalist, Farmer, Fisherman }
[CreateAssetMenu(menuName = "DownBelow/Presets/CardCollection    ")]
public class ScriptableCardCollection : SerializedScriptableObject {

    public Dictionary<ECollection, ScriptableCard[]> Collections;
}
