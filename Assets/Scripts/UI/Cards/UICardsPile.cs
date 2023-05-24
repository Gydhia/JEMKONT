using DownBelow.Managers;
using DownBelow.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UICardsPile : MonoBehaviour
{
    public PileType PileType;
    public int MaxStackedCards = -1;
    public List<DraggableCard> Cards;

    public TextMeshProUGUI CardsNumber;

    private void Update()
    {
        if(this.CardsNumber != null)
        {
            this.CardsNumber.text = this.Cards.Count.ToString();
        }
    }

    public void ShufflePile(ref List<DraggableCard> cards)
    {
        cards.Shuffle();
    }
}
