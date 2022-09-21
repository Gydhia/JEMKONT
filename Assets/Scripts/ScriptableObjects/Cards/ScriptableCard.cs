using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public enum EChipType { Melee, Ranged, Special }
[CreateAssetMenu(menuName ="Chip")]
public class ScriptableCard : ScriptableObject {
    public string Title;
    public int Cost;
    [TextArea]public string Description;
    public Sprite IllustrationImage;
}
public class ChipsComparer : IComparer<ScriptableCard> {
    public int Compare(ScriptableCard x, ScriptableCard y) {
        return x.Title.CompareTo(y.Title);
    }
}
