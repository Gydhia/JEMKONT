using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Managers {
    public class DeckbuildingSystem : _baseManager<DeckbuildingSystem> {
        [BoxGroup("Prefabs")]
        public GameObject BigCardPrefab;
        [BoxGroup("Prefabs")]
        public GameObject LittleCardPrefab;
    }
}
