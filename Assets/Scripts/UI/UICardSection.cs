using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DownBelow.Mechanics;
using DownBelow.Managers;
using Utility.SLayout;
using DownBelow.Events;

namespace DownBelow.UI
{
    public class UICardSection : MonoBehaviour
    {
        public List<UICardsHolder> CardsHolders;

        public SHorizontalLayoutGroup _cardsLayoutGroup;

        public void Init()
        {
            GameManager.Instance.OnSelfPlayerSwitched += _updateVisibleCards;

            foreach (var holder in this.CardsHolders)
            {
                holder.CanvasGroup.alpha = 0;
                holder.CanvasGroup.blocksRaycasts = false;
                holder.CanvasGroup.interactable = false;
            }
        }

        private void _updateVisibleCards(EntityEventData Data)
        {
            this.CardsHolders[Data.OldIndex].CanvasGroup.alpha = 0;
            this.CardsHolders[Data.OldIndex].CanvasGroup.blocksRaycasts = false;
            this.CardsHolders[Data.OldIndex].CanvasGroup.interactable = false;

            this.CardsHolders[Data.NewIndex].CanvasGroup.alpha = 1;
            this.CardsHolders[Data.NewIndex].CanvasGroup.blocksRaycasts = true;
            this.CardsHolders[Data.NewIndex].CanvasGroup.interactable = true;
        }

        public void UpdateLayoutGroup()
        {
            _cardsLayoutGroup.enabled = false;
            _cardsLayoutGroup.enabled = true;
        }
    }
}
