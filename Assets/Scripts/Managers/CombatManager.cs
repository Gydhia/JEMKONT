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
using DownBelow.UI;

namespace DownBelow.Managers {
    public class CombatManager : _baseManager<CombatManager> {
        #region EVENTS
        public event GridEventData.Event OnCombatStarted;
        public event GridEventData.Event OnCombatEnded;
        public event EntityEventData.Event OnTurnStarted;
        public event EntityEventData.Event OnTurnEnded;

        public event CardEventData.Event OnCardBeginUse;
        public event CardEventData.Event OnCardEndUse;

        public void FireCombatStarted(WorldGrid Grid)
        {
            this.OnCombatStarted?.Invoke(new GridEventData(Grid));
        }
        public void FireCombatEnded(WorldGrid Grid)
        {
            this.OnCombatEnded?.Invoke(new GridEventData(Grid));
        }

        public void FireTurnStarted(CharacterEntity Entity) 
        {
            this.OnTurnStarted?.Invoke(new EntityEventData(Entity));
        }
        public void FireTurnEnded(CharacterEntity Entity) 
        {
            this.OnTurnEnded?.Invoke(new EntityEventData(Entity));
        }

        public void FireCardBeginUse(ScriptableCard Card, Spell[] GeneratedSpells = null, Cell Cell = null, bool Played = false) 
        {
            this.OnCardBeginUse?.Invoke(new CardEventData(Card, GeneratedSpells, Cell, Played));
        }
        public void FireCardEndUse(ScriptableCard Card, Spell[] GeneratedSpells = null, Cell Cell = null, bool Played = false) 
        {
            this.OnCardEndUse?.Invoke(new CardEventData(Card, GeneratedSpells, Cell, Played));
        }

        #endregion

        public bool BattleGoing;
        private Spell[] _currentSpells;

        #region Run-time
        private Coroutine _turnCoroutine;

        public CharacterEntity CurrentPlayingEntity;
        public CombatGrid CurrentPlayingGrid;
        public List<CharacterEntity> PlayingEntities;

        public List<DraggableCard> DiscardPile;
        public Deck DrawPile;
        public List<DraggableCard> HandPile;

        public GameObject CardPrefab;
        public List<SpellAction> PossibleAutoAttacks;

        public int TurnNumber;
        #endregion

        private void Start()
        {
            GameManager.Instance.OnEnteredGrid += this.WelcomePlayerInCombat;

            this.OnCardBeginUse += this._beginUseSpell;
        }

        public void WelcomePlayerInCombat(EntityEventData Data) {
            Data.Entity.ReinitializeAllStats();
            //TODO: Verify this is well understood.
            DrawPile = ((PlayerBehavior)Data.Entity).Deck;
            DrawPile.ShuffleDeck();

            if (BattleGoing) {
                UIManager.Instance.StartCombatButton.gameObject.SetActive(false);
            }
        }

        public void StartCombat(CombatGrid startingGrid) {
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

            for (int i = 0;i < SettingsManager.Instance.CombatPreset.CardsToDrawAtStart;i++)
                DrawCard();
        }

        public void EndCombat()
        {
            this.FireCombatEnded(this.CurrentPlayingGrid.ParentGrid);
        }

        public void ProcessStartTurn(string entityID) {
            this.CurrentPlayingEntity.StartTurn();

            if (this.TurnNumber >= 0)
                UIManager.Instance.TurnSection.ChangeSelectedEntity(this.TurnNumber % this.PlayingEntities.Count);
                
            if(this.CurrentPlayingEntity is PlayerBehavior)
                this._turnCoroutine = StartCoroutine(this._startTurnTimer());
        }

        public void ProcessEndTurn(string entityID) {
            this.CurrentPlayingEntity.EndTurn();

            // We draw cards at end of turn
            if (GameManager.Instance.SelfPlayer == this.CurrentPlayingEntity) {
                for (int i = 0;i < SettingsManager.Instance.CombatPreset.CardsToDrawAtTurn;i++)
                    DrawCard();
            }

            // Reset the time slider
            if (this._turnCoroutine != null) {
                StopCoroutine(this._turnCoroutine);
                this._turnCoroutine = null;
            }

            UIManager.Instance.TurnSection.TimeSlider.value = 0f;

            // Increment the turns to pre-select next entity
            this.TurnNumber++;
            this.CurrentPlayingEntity = this.PlayingEntities[this.TurnNumber % this.PlayingEntities.Count];

            this.FireTurnStarted(this.CurrentPlayingEntity);
        }

        private void _setupEnemyEntities() {
            foreach (CharacterEntity enemy in this.CurrentPlayingGrid.GridEntities.Where(e => !e.IsAlly)) {
                enemy.ReinitializeAllStats();
                enemy.EntityCell.EntityIn = enemy;
                enemy.gameObject.SetActive(true);
            }
        }

        #region CARDS
        private void _beginUseSpell(CardEventData data)
        {
            // Copy it to avoid erasing datas
            this._currentSpells = new Spell[data.Card.Spells.Length];
            data.Card.Spells.CopyTo(this._currentSpells, 0);

            DraggableCard.SelectedCard.CardReference.CurrentSpellTargetting = 0;

            InputManager.Instance.OnCellRightClick += _abortUsedSpell;
            InputManager.Instance.OnCellClickedUp += _processSpellClick;
        }

        private void _abortUsedSpell(CellEventData Data)
        {
            this.FireCardEndUse(DraggableCard.SelectedCard.CardReference, this._currentSpells, null, false);

            this._currentSpells = null;
            DraggableCard.SelectedCard.DiscardToPile();

            InputManager.Instance.OnCellRightClick -= _abortUsedSpell;
            InputManager.Instance.OnCellClickedUp -= _processSpellClick;
        }

        private void _processSpellClick(CellEventData Data)
        {
            // No clicked cell -> no need to do anything
            if (Data.Cell == null)
                return;

            ScriptableCard currentCard = DraggableCard.SelectedCard.CardReference;
            Spell currentSpell = this._currentSpells[currentCard.CurrentSpellTargetting];

            // If the selected cell isn't of wanted type or isn't within range, same as before
            if (!currentSpell.TargetType.ValidateTarget(Data.Cell) ||
                (currentSpell.CastingMatrix != null &&
                 !GridUtility.IsCellWithinPlayerRange(
                     ref currentSpell.CastingMatrix,
                     this.CurrentPlayingEntity.EntityCell.PositionInGrid,
                     Data.Cell.PositionInGrid
                  )))
            {
                return;
            }

            currentSpell.TargetCell = Data.Cell;

            // Means that there are no more targetting spells in the array, so we finished
            if(currentCard.GetNextTargettingSpellIndex() == -1)
            {
                this.FireCardEndUse(currentCard, this._currentSpells, Data.Cell, true);

                // TODO: make it go to discard pile instead
                DraggableCard.SelectedCard.DiscardToPile();

                InputManager.Instance.OnCellRightClick -= _abortUsedSpell;
                InputManager.Instance.OnCellClickedUp -= _processSpellClick;
            }
        }

        public void DiscardCard(DraggableCard card) {
            UIManager.Instance.CardSection.AddDiscardCard(1);

            this.HandPile.Remove(card);
            this.DiscardPile.Add(card);
        }

        public void DrawCard() 
        {
            if (this.DrawPile.Count > 0) 
            {
                this.HandPile.Add(Instantiate(CardPrefab, UIManager.Instance.CardSection.DrawPile.transform).GetComponent<DraggableCard>());
                this.HandPile[^1].Init(DrawPile.DrawCard());

                this.HandPile[^1].DrawFromPile();

                if (HandPile.Count > 7) 
                {
                    this.HandPile[^1].DiscardToPile();
                    this.HandPile[^1].Burn();
                    this.HandPile.Remove(this.HandPile[^1]);
                }
            }
        }
        #endregion

        private IEnumerator _startTurnTimer() {
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

        private void _defineEntitiesTurn() {
            List<CharacterEntity> enemies = this.CurrentPlayingGrid.GridEntities.Where(x => !x.IsAlly).ToList();
            List<CharacterEntity> players = this.CurrentPlayingGrid.GridEntities.Where(x => x.IsAlly).ToList();

            for (int i = 0;i < players.Count;i++)
                players[i].TurnOrder = i;
            for (int i = 0;i < enemies.Count;i++)
                enemies[i].TurnOrder = i;

            this.PlayingEntities = new List<CharacterEntity>();

            // We check both for the tests, if we have more allies than ennemies or inverse
            if (enemies.Count >= players.Count) {
                for (int i = 0;i < enemies.Count;i++) {
                    this.PlayingEntities.Add(enemies[i]);
                    if (i < players.Count)
                        this.PlayingEntities.Add(players[i]);
                }
            } else {
                for (int i = 0;i < players.Count;i++) {
                    this.PlayingEntities.Add(players[i]);
                    if (i < enemies.Count)
                        this.PlayingEntities.Add(enemies[i]);
                }
            }
        }
    }
}