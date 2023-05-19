using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DownBelow.Mechanics;
using DownBelow.Managers;
using Utility.SLayout;

namespace DownBelow.UI
{
    public class UICardSection : MonoBehaviour
    {
        public GameObject CardsHolder;
        public GameObject DiscardHolder;
        public GameObject DrawHolder;

        public Image DrawPile;
        public TextMeshProUGUI DrawNumber;
        public Image DiscardPile;
        public TextMeshProUGUI DiscardNumber;

        public SHorizontalLayoutGroup _cardsLayoutGroup;

        private RectTransform _cardsHolderRectTransform;

        private void Start()
        {
            _cardsHolderRectTransform = CardsHolder.GetComponent<RectTransform>();
        }

        private void Update()
        {
            // No needs to handle this another way for now. Suck but good
            this.DiscardNumber.text = CardsManager.Instance.DiscardPile.Count.ToString();
            this.DrawNumber.text = CardsManager.Instance.DrawPile.Count.ToString();
        }

        public void UpdateLayoutGroup()
        {
            _cardsLayoutGroup.enabled = false;
            _cardsLayoutGroup.enabled = true;
        }


    }
}
