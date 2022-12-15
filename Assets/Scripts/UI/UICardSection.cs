using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Jemkont.Mechanics;
namespace Jemkont.UI
{
    public class UICardSection : MonoBehaviour
    {

        [SerializeField] private RectTransform _cardsHolder;
        [SerializeField] private GameObject _spellCardPrefab;

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

        public void AddNewCardInHand(ScriptableCard scriptableCard)
        {
            CardComponent card = Instantiate(_spellCardPrefab, _cardsHolder).GetComponent<CardComponent>();
            card.Init(scriptableCard);
        }
    }

}
