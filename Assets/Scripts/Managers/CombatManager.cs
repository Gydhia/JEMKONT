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

namespace DownBelow.Managers
{
    public class CombatManager : _baseManager<CombatManager>
    {
        #region EVENTS
        public event GridEventData.Event OnCombatStarted;
        public event GridEventData.Event OnCombatEnded;
        public event EntityEventData.Event OnTurnStarted;
        public event EntityEventData.Event OnTurnEnded;

        public event CardEventData.Event OnCardBeginUse;
        public event CardEventData.Event OnCardEndUse;

        public event SpellTargetEventData.Event OnSpellBeginTargetting;
        public event SpellTargetEventData.Event OnSpellEndTargetting;

        public void FireCombatStarted(WorldGrid Grid) =>
            this.OnCombatStarted?.Invoke(new GridEventData(Grid));

        public void FireCombatEnded(WorldGrid Grid) =>
            this.OnCombatEnded?.Invoke(new GridEventData(Grid));

        public void FireTurnStarted(CharacterEntity Entity) =>
            this.OnTurnStarted?.Invoke(new EntityEventData(Entity));

        public void FireTurnEnded(CharacterEntity Entity) =>
            this.OnTurnEnded?.Invoke(new EntityEventData(Entity));

        public void FireCardBeginUse(
            ScriptableCard Card,
            Spell[] GeneratedSpells = null,
            Cell Cell = null,
            bool Played = false
        ) => this.OnCardBeginUse?.Invoke(new CardEventData(Card, GeneratedSpells, Cell, Played));

        public void FireCardEndUse(
            ScriptableCard Card,
            Spell[] GeneratedSpells = null,
            Cell Cell = null,
            bool Played = false
        ) => this.OnCardEndUse?.Invoke(new CardEventData(Card, GeneratedSpells, Cell, Played));

        public void FireSpellBeginTargetting(Spell TargetSpell, Cell Cell) =>
            this.OnSpellBeginTargetting?.Invoke(new SpellTargetEventData(TargetSpell, Cell));

        public void FireSpellEndTargetting(Spell TargetSpell, Cell Cell) =>
            this.OnSpellEndTargetting?.Invoke(new SpellTargetEventData(TargetSpell, Cell));
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

        public int TurnNumber;
        #endregion

        private void Start()
        {
            GameManager.Instance.OnEnteredGrid += this.WelcomePlayerInCombat;

            this.OnCardBeginUse += this._beginUseSpell;
        }

        public void WelcomePlayerInCombat(EntityEventData Data)
        {
            Data.Entity.ReinitializeAllStats();
            //TODO: Verify this is well understood.
            DrawPile = ((PlayerBehavior)Data.Entity).Deck;
            DrawPile.ShuffleDeck();

            if (BattleGoing)
            {
                UIManager.Instance.StartCombatButton.gameObject.SetActive(false);
            }
        }

        public void StartCombat(CombatGrid startingGrid)
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
            this.CurrentPlayingEntity = this.PlayingEntities[
                this.TurnNumber % this.PlayingEntities.Count
            ];
            this.FireTurnStarted(this.CurrentPlayingEntity);

            for (int i = 0; i < SettingsManager.Instance.CombatPreset.CardsToDrawAtStart; i++)
                DrawCard();
        }

        public void EndCombat()
        {
            this.FireCombatEnded(this.CurrentPlayingGrid.ParentGrid);
        }

        public void ProcessStartTurn(string entityID)
        {
            this.CurrentPlayingEntity = this.PlayingEntities.Where(e => e.UID == entityID).FirstOrDefault();

            Debug.Log("Started turn for entity : " + this.CurrentPlayingEntity.name);
            this.CurrentPlayingEntity.StartTurn();

            if (this.TurnNumber >= 0)
                UIManager.Instance.TurnSection.ChangeSelectedEntity(
                    this.TurnNumber % this.PlayingEntities.Count
                );

            if (this.CurrentPlayingEntity is PlayerBehavior)
                this._turnCoroutine = StartCoroutine(this._startTurnTimer());
        }

        public void ProcessEndTurn(string entityID)
        {
            this.CurrentPlayingEntity.EndTurn();

            // We draw cards at end of turn
            if (GameManager.Instance.SelfPlayer == this.CurrentPlayingEntity)
            {
                for (int i = 0; i < SettingsManager.Instance.CombatPreset.CardsToDrawAtTurn; i++)
                    DrawCard();
            }

            // Reset the time slider
            if (this._turnCoroutine != null)
            {
                StopCoroutine(this._turnCoroutine);
                this._turnCoroutine = null;
            }

            UIManager.Instance.TurnSection.TimeSlider.fillAmount = 0f;

            // Increment the turns to pre-select next entity
            this.TurnNumber++;
            this.CurrentPlayingEntity = this.PlayingEntities[
                this.TurnNumber % this.PlayingEntities.Count
            ];

            this.FireTurnStarted(this.CurrentPlayingEntity);
        }

        private void _setupEnemyEntities()
        {
            foreach (
                CharacterEntity enemy in this.CurrentPlayingGrid.GridEntities.Where(e => !e.IsAlly)
            )
            {
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

            // We use the constructor since it's how EntityActions work, and only give de spellData to it with main datas
            object[] fullDatas = new object[5];
            for (int i = 0; i < this._currentSpells.Length; i++)
            {
                Type type = data.Card.Spells[i].GetType();

                fullDatas[0] = data.Card.Spells[i].Data;
                fullDatas[1] = this.CurrentPlayingEntity;
                fullDatas[3] = i > 0 ? this._currentSpells[i - 1] : null;
                fullDatas[4] = data.Card.Spells[i].ConditionData;

                this._currentSpells[i] = Activator.CreateInstance(type, fullDatas) as Spell;
            }

            DraggableCard.SelectedCard.CardReference.CurrentSpellTargetting = 0;

            this.FireSpellBeginTargetting(this._currentSpells[0], data.Cell);

            InputManager.Instance.OnCellRightClickDown += _abortUsedSpell;
            InputManager.Instance.OnCellClickedUp += _processSpellClick;
        }

        private void _abortUsedSpell(CellEventData Data)
        {
            this.FireCardEndUse(
                DraggableCard.SelectedCard.CardReference,
                this._currentSpells,
                null,
                false
            );

            this.FireSpellEndTargetting(
                this._currentSpells[
                    DraggableCard.SelectedCard.CardReference.CurrentSpellTargetting
                ],
                Data.Cell
            );

            this._currentSpells = null;

            DraggableCard.SelectedCard.DiscardToPile();

            InputManager.Instance.OnCellRightClickDown -= _abortUsedSpell;
            InputManager.Instance.OnCellClickedUp -= _processSpellClick;
        }

        public bool IsCellCastable(Cell cell, Spell spell)
        {
            return cell != null
                && spell.Data.TargetType.ValidateTarget(cell)
                && (
                    spell.Data.CastingMatrix == null
                    || GridUtility.IsCellWithinPlayerRange(
                        ref spell.Data.CastingMatrix,
                        this.CurrentPlayingEntity.EntityCell.PositionInGrid,
                        cell.PositionInGrid,
                        spell.Data.CasterPosition
                    )
                );
        }

        private void _processSpellClick(CellEventData Data)
        {
            // No clicked cell -> no need to do anything
            if (Data.Cell == null)
                return;

            ScriptableCard currentCard = DraggableCard.SelectedCard.CardReference;
            Spell currentSpell = this._currentSpells[currentCard.CurrentSpellTargetting];

            // If the selected cell isn't of wanted type or isn't within range, same as before
            if (!this.IsCellCastable(Data.Cell, currentSpell))
                return;

            currentSpell.TargetCell = Data.Cell;
            this.FireSpellEndTargetting(currentSpell, Data.Cell);

            // Means that there are no more targetting spells in the array, so we finished
            if (currentCard.GetNextTargettingSpellIndex() == -1)
            {
                this.FireCardEndUse(currentCard, this._currentSpells, Data.Cell, true);

                // TODO: make it go to discard pile instead
                DraggableCard.SelectedCard.DiscardToPile();

                InputManager.Instance.OnCellRightClickDown -= _abortUsedSpell;
                InputManager.Instance.OnCellClickedUp -= _processSpellClick;
            }
            else
            {
                this.FireSpellBeginTargetting(
                    this._currentSpells[
                        DraggableCard.SelectedCard.CardReference.CurrentSpellTargetting
                    ],
                    Data.Cell
                );
            }
        }

        public void DiscardCard(DraggableCard card)
        {
            UIManager.Instance.CardSection.AddDiscardCard(1);

            this.HandPile.Remove(card);
            this.DiscardPile.Add(card);
        }

        public void DrawCard()
        {
            if (this.DrawPile.Count > 0)
            {
                this.HandPile.Add(
                    Instantiate(CardPrefab, UIManager.Instance.CardSection.DrawPile.transform)
                        .GetComponent<DraggableCard>()
                );
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

        private IEnumerator _startTurnTimer()
        {
            float time = SettingsManager.Instance.CombatPreset.TurnTime;
            float timePassed = 0f;

            UIManager.Instance.TurnSection.TimeSlider.fillAmount = 0f;
            //UIManager.Instance.TurnSection.TimeSlider.maxValue = time;

            while (timePassed <= time)
            {
                yield return new WaitForSeconds(Time.deltaTime);
                timePassed += Time.deltaTime;

                float timeToShowOnSlider = timePassed / time;
                
                UIManager.Instance.TurnSection.TimeSlider.fillAmount = timeToShowOnSlider;
            }

            this.FireTurnEnded(this.CurrentPlayingEntity);
        }

        private void _defineEntitiesTurn()
        {
            List<CharacterEntity> enemies = this.CurrentPlayingGrid.GridEntities
                .Where(x => !x.IsAlly)
                .ToList();
            List<CharacterEntity> players = this.CurrentPlayingGrid.GridEntities
                .Where(x => x.IsAlly)
                .ToList();

            for (int i = 0; i < players.Count; i++)
                players[i].TurnOrder = i;
            for (int i = 0; i < enemies.Count; i++)
                enemies[i].TurnOrder = i;

            this.PlayingEntities = new List<CharacterEntity>();

            // We check both for the tests, if we have more allies than ennemies or inverse
            if (enemies.Count >= players.Count)
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    this.PlayingEntities.Add(enemies[i]);
                    if (i < players.Count)
                        this.PlayingEntities.Add(players[i]);
                }
            }
            else
            {
                for (int i = 0; i < players.Count; i++)
                {
                    this.PlayingEntities.Add(players[i]);
                    if (i < enemies.Count)
                        this.PlayingEntities.Add(enemies[i]);
                }
            }
        }
    }
}
