using DownBelow.Spells;
using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using DownBelow.Mechanics;

namespace DownBelow.Managers {
    public class CombatManager : _baseManager<CombatManager> 
    {
        #region EVENTS
        public event GridEventData.Event OnCombatStarted;
        public event EntityEventData.Event OnTurnStarted;
        public event EntityEventData.Event OnTurnEnded;

        public event CardEventData.Event OnCardBeginDrag;
        public event CardEventData.Event OnCardEndDrag;
       
        public void FireCombatStarted(WorldGrid Grid) 
        {
            this.OnCombatStarted?.Invoke(new GridEventData(Grid));
        }
        public void FireTurnStarted(CharacterEntity Entity)
        {
            this.OnTurnStarted?.Invoke(new EntityEventData(Entity));
        }
        public void FireTurnEnded(CharacterEntity Entity)
        {
            this.OnTurnEnded?.Invoke(new EntityEventData(Entity));
        }

        public void FireCardBeginDrag(ScriptableCard Card, Cell Cell = null, bool Played = false)
        {
            this.OnCardBeginDrag?.Invoke(new CardEventData(Card, Cell, Played));
        }
        public void FireCardEndDrag(ScriptableCard Card, Cell Cell = null, bool Played = false)
        {
            this.OnCardEndDrag?.Invoke(new CardEventData(Card, Cell, Played));
        }

        #endregion

        public bool BattleGoing;
        private Spell[] _currentSpells;

        #region Run-time
        private Coroutine _turnCoroutine;

        public CharacterEntity CurrentPlayingEntity;
        public CombatGrid CurrentPlayingGrid;
        public List<CharacterEntity> PlayingEntities;

        public ScriptableCard CurrentCard;
        public List<CardComponent> DiscardPile;
        public Deck DrawPile;
        public List<CardComponent> HandPile;

        public GameObject CardPrefab;
        public List<SpellAction> PossibleAutoAttacks;

        public int TurnNumber;
        #endregion

        private void Start() 
        {
            GameManager.Instance.OnEnteredGrid += this.WelcomePlayerInCombat;

            this.OnCardBeginDrag += _cardDrag;
            InputManager.Instance.OnCellClickedUp += _cardEndDrag;
            InputManager.Instance.OnCellRightClick += _processCellClickUp;
        }

        public void ExecuteSpells(Cell target, ScriptableCard spell) 
        {
            this._currentSpells = spell.Spells;
            StartCoroutine(this._waitForSpell(target));
        }

        public void WelcomePlayerInCombat(EntityEventData Data) 
        {
            Data.Entity.ReinitializeAllStats();
            //TODO: Verify this is well understood.
            DrawPile = ((PlayerBehavior)Data.Entity).Deck;
            DrawPile.ShuffleDeck();

            if (BattleGoing) {
                UIManager.Instance.StartCombatButton.gameObject.SetActive(false);
            }
        }

        public async void StartCombat(CombatGrid startingGrid) 
        {
            if (this.CurrentPlayingGrid != null && this.CurrentPlayingGrid.HasStarted)
                return;

            this.BattleGoing = true;
            this.CurrentPlayingGrid = startingGrid;

            this._setupEnemyEntities();

            // Think about enabling/initing this UI only when in combat
            UIManager.Instance.PlayerInfos.Init();

            this.TurnNumber = -1;
            this.CurrentPlayingGrid.HasStarted = true;

            this._defineEntitiesTurn();

            UIManager.Instance.TurnSection.Init(this.PlayingEntities);
            UIManager.Instance.StartCombatButton.gameObject.SetActive(false);

            this.FireCombatStarted(this.CurrentPlayingGrid);
            this.TurnNumber++;
            this.CurrentPlayingEntity = this.PlayingEntities[this.TurnNumber % this.PlayingEntities.Count];
            this.FireTurnStarted(this.CurrentPlayingEntity);

            for (int i = 0;i < SettingsManager.Instance.CombatPreset.CardsToDraw; i++) 
                await DrawCard();
        }

        public void ProcessStartTurn(string entityID)
        {
            this.CurrentPlayingEntity.StartTurn();

            if (this.TurnNumber >= 0)
                UIManager.Instance.TurnSection.ChangeSelectedEntity(this.TurnNumber % this.PlayingEntities.Count);
            // TODO : remove this when we'll no longer need to test enemies
            //if (this.CurrentPlayingEntity.IsAlly)
            if(this.CurrentPlayingEntity is PlayerBehavior)
                this._turnCoroutine = StartCoroutine(this._startTurnTimer());
        }

        public async void ProcessEndTurn(string entityID)
        {
            this.CurrentPlayingEntity.EndTurn();

            // We draw cards at end of turn
            if (GameManager.Instance.SelfPlayer == this.CurrentPlayingEntity)
            {
                for (int i = 0; i < SettingsManager.Instance.CombatPreset.CardsToDraw; i++)
                    await DrawCard();
            }

            // Reset the time slider
            if (this._turnCoroutine != null)
            {
                StopCoroutine(this._turnCoroutine);
                this._turnCoroutine = null;
            }

            UIManager.Instance.TurnSection.TimeSlider.value = 0f;

            // Increment the turns to pre-select next entity
            this.TurnNumber++;
            this.CurrentPlayingEntity = this.PlayingEntities[this.TurnNumber % this.PlayingEntities.Count];

            this.FireTurnStarted(this.CurrentPlayingEntity);
        }

        private IEnumerator _waitForSpell(DownBelow.GridSystem.Cell target) 
        {
            for (int i = 0;i < this._currentSpells.Length;i++) 
            {
                bool canExecute = true;

                if (this._currentSpells[i].ConditionData != null)
                    if (i - 1 >= 0)
                        if (!this._currentSpells[i].ConditionData.Check(this._currentSpells[i - 1].CurrentAction.Result))
                            canExecute = false;

                if (canExecute) {
                    this._currentSpells[i].CurrentAction = Instantiate(this._currentSpells[i].ActionData,Vector3.zero,Quaternion.identity,CombatManager.Instance.CurrentPlayingEntity.gameObject.transform);
                    this._currentSpells[i].ExecuteSpell(CurrentPlayingEntity,target);
                    while (!this._currentSpells[i].CurrentAction.HasEnded) {
                        yield return new WaitForSeconds(Time.deltaTime);
                    }
                    CurrentPlayingEntity.UnsubToSpell(this._currentSpells[i].ActionData);
                }
            }

        }

        private void _setupEnemyEntities() 
        {
            foreach (CharacterEntity enemy in this.CurrentPlayingGrid.GridEntities.Where(e => !e.IsAlly))
            {
                enemy.ReinitializeAllStats();
                enemy.EntityCell.EntityIn = enemy;
                enemy.gameObject.SetActive(true);
            }
        }

        #region CARDS
        private void _cardDrag(CardEventData Data)
        {
            this.CurrentCard = Data.Card;
        }

        private void _cardEndDrag(CellEventData Data)
        {
            // If we aren't dragging card or outside the grid, return
            if (this.CurrentCard == null)
                return;

            // If there is no selected Cell, the card should return to hand
            if(Data.Cell == null || !Data.InCurrentGrid)
            {
                this.FireCardEndDrag(this.CurrentCard, Data.Cell, false);
                return;
            }
            
            // Lastly, cast the card's spell and say that the card has been played
            else if (this.CurrentCard != null)
                this.CurrentCard.CastSpell(Data.Cell);

            this.FireCardEndDrag(this.CurrentCard, Data.Cell, true);

            this.CurrentCard = null;
        }
        
        private void _processCellClickUp(CellEventData Data)
        {
            // If we right click while dragging a card, cancel
            if(this.CurrentCard != null)
            {
                this._cardEndDrag(Data);
            }
        }

        public void DiscardCard(CardComponent card) 
        {
            UIManager.Instance.CardSection.AddDiscardCard(1);

            this.HandPile.Remove(card);
            this.DiscardPile.Add(card);
        }

        public async Task DrawCard()
        {
            if (this.DrawPile.Count > 0) 
            {
                this.HandPile.Add(Instantiate(CardPrefab,UIManager.Instance.CardSection.DrawPile.transform).GetComponent<CardComponent>());
                this.HandPile[^1].Init(DrawPile.DrawCard());
                
                await this.HandPile[^1].DrawCardFromPile();

                if (HandPile.Count > 7) {
                    this.HandPile[^1].Burn();
                    this.HandPile.Remove(this.HandPile[^1]);
                }
            }
        }
        #endregion

        private IEnumerator _startTurnTimer()
        {
            float time = SettingsManager.Instance.CombatPreset.TurnTime;
            float timePassed = 0f;

            UIManager.Instance.TurnSection.TimeSlider.minValue = 0f;
            UIManager.Instance.TurnSection.TimeSlider.maxValue = time;

            while (timePassed <= time) {
                yield return new WaitForSeconds(Time.deltaTime);
                timePassed += Time.deltaTime;
                UIManager.Instance.TurnSection.TimeSlider.value = timePassed;
            }

            this.FireTurnEnded(this.CurrentPlayingEntity);
        }

        private void _defineEntitiesTurn() 
        {
            List<CharacterEntity> enemies = this.CurrentPlayingGrid.GridEntities.Where(x => !x.IsAlly).ToList();
            List<CharacterEntity> players = this.CurrentPlayingGrid.GridEntities.Where(x => x.IsAlly).ToList();

            for (int i = 0;i < players.Count;i++)
                players[i].TurnOrder = i;
            for (int i = 0;i < enemies.Count;i++)
                enemies[i].TurnOrder = i;

            this.PlayingEntities = new List<CharacterEntity>();
            for (int i = 0;i < enemies.Count;i++) {
                this.PlayingEntities.Add(enemies[i]);
                if (i < players.Count)
                    this.PlayingEntities.Add(players[i]);
            }
            this.PlayingEntities = this.PlayingEntities.OrderBy(x => x.Inspired).ToList();
        }
    }
}