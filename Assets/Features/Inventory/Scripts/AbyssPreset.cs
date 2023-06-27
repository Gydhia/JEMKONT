using DownBelow.GridSystem;
using DownBelow.Managers;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Mechanics
{
    [CreateAssetMenu(fileName = "AbyssPreset", menuName = "DownBelow/Grid/Abyss Preset", order = 2)]
    public class AbyssPreset : SerializedScriptableObject
    {
        [ReadOnly]
        public bool IsCleared = false;

        [ValueDropdown("GetSavedGrids")]
        public string TargetGrid;
        private IEnumerable<string> GetSavedGrids()
        {
            GridManager.Instance.LoadGridsFromJSON();
            return GridManager.Instance.SavedGrids.Keys;
        }

        public bool RefillResources = true;
        public int MaxResourcesUpgrade = 5;

        public List<ScriptableCard> GiftedCards = new List<ScriptableCard>();

        public void GiftCards()
        {
            foreach (var card in GiftedCards) 
            {
                var act = new PurchaseCardsAction(GameManager.RealSelfPlayer, GameManager.RealSelfPlayer.EntityCell);
                act.Init(card);
                NetworkManager.Instance.EntityAskToBuffAction(act);
            }
        }
    }
}