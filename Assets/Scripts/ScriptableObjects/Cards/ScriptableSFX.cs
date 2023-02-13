using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Mechanics {
    public enum ESFXTravelType {
        Instantaneous,
        ProjectileToEnemy,
        //We can imagine others:
        ProjectileFromEnemy,
        ProjectileBackAndForth,

    }
    [CreateAssetMenu(menuName = "SpellSFX")]
    public class ScriptableSFX : ScriptableObject {
        public GameObject Prefab;
        [Tooltip("This also defines when the spells are going to be applied.")]
        public ESFXTravelType TravelType;
        //Sounds too, one day,maybe...
        [EnableIf("@TravelType != ESFXTravelType.Instantaneous")]
        public float TravelTime = 0.35f;
    }
}