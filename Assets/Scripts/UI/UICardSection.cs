using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DownBelow.Mechanics;

namespace DownBelow.UI
{
    public class UICardSection : MonoBehaviour
    {

        public GameObject CardsHolder;

        public Image DrawPile;
        public TextMeshProUGUI DrawNumber;
        public int CardsInDraw;
        public Image DiscardPile;
        public TextMeshProUGUI DiscardNumber;
        public int CardsInDiscard;

        private RectTransform _cardsHolderRectTransform;

        private void Start()
        {
            _cardsHolderRectTransform = CardsHolder.GetComponent<RectTransform>();
        }

        public void AddDiscardCard(int number)
        {
            this.CardsInDiscard += number;
            this.DiscardNumber.text = this.CardsInDiscard.ToString();
        }
        public void AddDrawCard(int number)
        {
            this.CardsInDraw += number;
            this.DrawNumber.text = this.CardsInDraw.ToString();
        }



        public void UpdateLayoutGroup()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_cardsHolderRectTransform);
        }

    }

}
