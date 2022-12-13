using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UICardSection : MonoBehaviour
    {
        public GameObject HandPile;

        public Image DrawPile;
        public TextMeshProUGUI DrawNumber;
        public int CardsInDraw;
        public Image DiscardPile;
        public TextMeshProUGUI DiscardNumber;
        public int CardsInDiscard;

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
    }

}
