using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.Managers;
using DownBelow.Mechanics;
using DownBelow.UI;
using EODE.Wonderland;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName ="NewDeckPreset.asset",menuName ="DownBelow/Cards/DeckPreset")]
public class DeckPreset : SerializedScriptableObject
{
    [ReadOnly]
    public Guid UID;
    [OnValueChanged("_updateUID")]
    public string Name;

    public EClass Class;

    public Deck Deck;
    public EntityStats Statistics;

    public Deck Copy() {
        return new Deck(Deck);
    }

    public PlayerBehavior LinkedPlayer;

    public UICardsHolder RefCardsHolder;

    public void SetupForCombat(UICardsHolder Holder)
    {
        this.RefCardsHolder = Holder;
        this.RefCardsHolder.LinkedPlayer = this.LinkedPlayer;
        this.ClearAllPiles();

        CombatManager.Instance.OnTurnStarted += _subscribeEntityEvents;
        LinkedPlayer.OnTurnEnded += SelfDrawCard;

        List<DraggableCard> createdCards = new List<DraggableCard>();
        foreach (var card in this.Deck.Cards)
        {
            createdCards.Add(this.CreateCard(card));
        }
        this.RefCardsHolder.AddRangeToPile(PileType.Draw, createdCards, true);

        this.SelfDrawCard(null);
    }

    public void EndForCombat()
    {
        this.ClearAllPiles();

        GameManager.SelfPlayer.OnTurnEnded -= SelfDrawCard;
    }

    public void SelfDrawCard(GameEventData Data)
    {
        this._unsubscribeEntityEvents();

        for (int i = 0; i < SettingsManager.Instance.CombatPreset.CardsToDrawAtStart; i++)
        {
            DrawCard();
        }
    }

    public void TryDiscardCard(CardEventData Data)
    {
        if (!Data.Played)
            return;

        this.RefCardsHolder.MoveCard(PileType.Hand, PileType.Discard, Data.DraggableCard, false);
    }

    public void CreateAndDrawSpecificCard(ScriptableCard card)
    {
        var createdCard = CreateCard(card);
        this.RefCardsHolder.Piles[PileType.Draw].Cards.Insert(0, createdCard);
        this.DrawCard(RefCardsHolder.GetCardIndex(PileType.Draw, createdCard));
    }

    public void DrawCard(ScriptableCard card, PileType pileType)
    {
        var refCard = this.GetSpecificCard(card, pileType);
        if (refCard != null)
        {
            this.DrawCard(this.RefCardsHolder.GetCardIndex(pileType, refCard));
        }
    }
    public void DrawCard(int specificIndex = 0)
    {
        this.RefCardsHolder.CheckMainPilesState();

        this.RefCardsHolder.MoveCard(PileType.Draw, PileType.Hand, specificIndex);
    }

    public DraggableCard GetSpecificCard(ScriptableCard card, PileType pileType)
    {
        DraggableCard refCard = null;

        if (pileType != PileType.All)
        {
            refCard = this.RefCardsHolder.Piles[pileType].Cards.SingleOrDefault(c => c.CardReference == card);
        }
        else
        {
            foreach (PileType pile in Enum.GetValues(typeof(PileType)))
            {
                if (pile == PileType.All) { break; }
                refCard = this.GetSpecificCard(card, pile);
                if (refCard != null) { break; }
            }
        }

        return refCard;
    }
    public DraggableCard CreateCard(ScriptableCard fromCard) 
    {
        var card = Instantiate(CardsManager.Instance.CardPrefab, this.RefCardsHolder.Piles[PileType.Draw].CardsHolder);
        card.name += fromCard.Title;
        card.Init(fromCard, this.RefCardsHolder.Piles[PileType.Draw]);

        return card;
    }
    
    public void ClearAllPiles()
    {
        foreach (var pile in this.RefCardsHolder.Piles)
        {
            foreach (var card in pile.Value.Cards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            pile.Value.Cards.Clear();
        }
    }

    private void _subscribeEntityEvents(EntityEventData Data)
    {
        if(Data.Entity == this.LinkedPlayer)
        {
            CombatManager.Instance.OnCardEndUse += TryDiscardCard;
        }
    }

    private void _unsubscribeEntityEvents()
    {
        CombatManager.Instance.OnCardEndUse -= TryDiscardCard;
    }

    private void _updateUID()
    {
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(this.Name));
            this.UID = new Guid(hash);
        }
    }
}
