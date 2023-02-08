using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Mechanics {
    public enum ESFXTravelType {
        Instantaneous,
        ProjectileToEnemy,
        //We can imagine others:
        //ProjectileFromEnemy
    }
    [CreateAssetMenu(menuName = "SpellSFX")]
    public class ScriptableSFX : ScriptableObject {
        public GameObject Prefab;
        [Tooltip("This also defines when the spells are going to be applied.")] public ESFXTravelType TravelType;
        //Sounds too, one day...
    }
}
