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
using UnityEngine.InputSystem;

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

        public void FireCardBeginUse(
            ScriptableCard Card,
            DraggableCard DraggedCard = null,
            SpellHeader GeneratedHeader = null,
            Cell Cell = null,
            bool Played = false
        ) => this.OnCardBeginUse?.Invoke(new CardEventData(Card, DraggedCard, GeneratedHeader, Cell, Played));

        public void FireCardEndUse(
            ScriptableCard Card,
            DraggableCard DraggedCard = null,
            SpellHeader GeneratedHeader = null,
            Cell Cell = null,
            bool Played = false
        ) => this.OnCardEndUse?.Invoke(new CardEventData(Card, DraggedCard, GeneratedHeader, Cell, Played));

        public void FireSpellBeginTargetting(Spell TargetSpell, Cell Cell) =>
            this.OnSpellBeginTargetting?.Invoke(new SpellTargetEventData(TargetSpell, Cell));

        public void FireSpellEndTargetting(Spell TargetSpell, Cell Cell) =>
            this.OnSpellEndTargetting?.Invoke(new SpellTargetEventData(TargetSpell, Cell));
        #endregion

        public bool BattleGoing;
        private SpellHeader _currentSpellHeader;
        private Spell _currentSpell;

        #region Run-time
        private Coroutine _turnCoroutine;

        public static CharacterEntity CurrentPlayingEntity;
        public static CombatGrid CurrentPlayingGrid;
        public List<CharacterEntity> PlayingEntities;

        public List<PlayerBehavior> PlayersInGrid = new List<PlayerBehavior>();

        /// <summary>
        /// Both used in the setup phase and the playing phase. 
        /// Before combat, this is only used as a placeholder
        /// </summary>
        public List<PlayerBehavior> FakePlayers;
        private int _currentPlayerIndex;

        public int TurnNumber;

        public List<NonCharacterEntity> NCEs;

        #endregion

        private void Start()
        {
            GameManager.Instance.OnEnteredGrid += this.WelcomePlayerInCombat;

            this.OnCardBeginUse += this._beginUseSpell;
        }

        /// <summary>
        /// To welcome any player entering a combat grid.
        /// </summary>
        /// <param name="Data"></param>
        public void WelcomePlayerInCombat(EntityEventData Data)
        {
            if (!(Data.Entity.CurrentGrid is CombatGrid))
                return;

            CombatGrid currentGrid = Data.Entity.CurrentGrid as CombatGrid;

            PlayerBehavior player = Data.Entity as PlayerBehavior;

            this.PlayersInGrid.Add(player);

            // If FakePlayers isn't populated (=init), do it
            if(this.FakePlayers == null || this.FakePlayers.Count == 0)
            {
                foreach (var deck in CardsManager.Instance.DeckPresets.Values)
                {
                    if(deck == player.Deck) { continue; }

                    var fakePlayer = Instantiate(GameManager.Instance.PlayerPrefab, GameManager.Instance.gameObject.transform);

                    Cell placementCell = currentGrid.PlacementCells.First(c => c.Datas.state != CellState.EntityIn);

                    fakePlayer.Init(placementCell, currentGrid, 0, true);

                    var refTool = ToolsManager.Instance.ToolPresets.Values.Single(t => t.DeckPreset == deck);
                    fakePlayer.SetActiveTool(refTool);

                    fakePlayer.ReinitializeAllStats();

                    this.FakePlayers.Add(fakePlayer);
                }
            }

            // Replace the fake on by the real player
            var fakeToReplace = this.FakePlayers.SingleOrDefault(f => f.Deck == player.Deck);

            if(fakeToReplace != null)
            {
                fakeToReplace.FireExitedCell();
                this.FakePlayers.Remove(fakeToReplace);

                Destroy(fakeToReplace);
            }

            Cell playerCell = currentGrid.PlacementCells.First(c => c.Datas.state != CellState.EntityIn);
            Data.Entity.FireExitedCell();
            Data.Entity.FireEnteredCell(playerCell);
            Data.Entity.transform.position = playerCell.WorldPosition;

            Data.Entity.ReinitializeAllStats();

            if(GameManager.Instance.SelfPlayer == Data.Entity)
            {
                PoolManager.Instance.CellIndicatorPool.DisplayPathIndicators(currentGrid.PlacementCells);
            }
        }

        public void StartCombat(CombatGrid startingGrid)
        {
            if (CurrentPlayingGrid != null && CurrentPlayingGrid.HasStarted)
                return;

            this.BattleGoing = true;
            CurrentPlayingGrid = startingGrid;

            this._setupEnemyEntities();

            UIManager.Instance.PlayerInfos.Init();

            this.TurnNumber = -1;
            CurrentPlayingGrid.HasStarted = true;

            this._defineEntitiesTurn();

            this.FireCombatStarted(CurrentPlayingGrid);

            StartCoroutine(this._startCombatDelay(2f));
        }

        private IEnumerator _startCombatDelay(float time)
        {
            yield return new WaitForSeconds(time);

            NetworkManager.Instance.StartEntityTurn();
        }


        private void _switchToFirstPlayer(InputAction.CallbackContext ctx) => this._switchSelectedPlayer(0);
        private void _switchToSecondPlayer(InputAction.CallbackContext ctx) => this._switchSelectedPlayer(1);
        private void _switchToThirdPlayer(InputAction.CallbackContext ctx) => this._switchSelectedPlayer(2);
        private void _switchToFourthPlayer(InputAction.CallbackContext ctx) => this._switchSelectedPlayer(3);

        private void _switchSelectedPlayer(int index)
        {
            this._currentPlayerIndex = index;
        }

        public void ProcessStartTurn()
        {
            this.TurnNumber++;
            CurrentPlayingEntity = this.PlayingEntities[
                this.TurnNumber % this.PlayingEntities.Count
            ];

            if (this.TurnNumber >= 0)
                UIManager.Instance.TurnSection.ChangeSelectedEntity(
                    this.TurnNumber % this.PlayingEntities.Count
                );

            if (CurrentPlayingEntity is PlayerBehavior && Photon.Pun.PhotonNetwork.IsMasterClient)
                this._turnCoroutine = StartCoroutine(this._startTurnTimer());

            this.OnTurnStarted?.Invoke(new EntityEventData(CurrentPlayingEntity));
        }


        public void ProcessEndTurn()
        {
            CurrentPlayingEntity.EndTurn();

            // Reset the time slider
            if (this._turnCoroutine != null)
            {
                StopCoroutine(this._turnCoroutine);
                this._turnCoroutine = null;
            }

            UIManager.Instance.TurnSection.TimeSlider.fillAmount = 0f;

            this.OnTurnEnded?.Invoke(new EntityEventData(CurrentPlayingEntity));
        }

        public void EndCombat()
        {
            this.FireCombatEnded(CurrentPlayingGrid.ParentGrid);
        }

        private void _setupEnemyEntities()
        {
            foreach (
                CharacterEntity enemy in CurrentPlayingGrid.GridEntities.Where(e => !e.IsAlly)
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
            // Create the spell header, used to store only usefull datas (=avoid duplications)
            this._currentSpellHeader = new SpellHeader(data.Card.UID, data.Card.Spells.Length, CurrentPlayingEntity.UID);
            this._currentSpell = data.Card.Spells.First(s => s.Data.RequiresTargetting);

            foreach (var spell in data.Card.Spells)
            {
                spell.Data.Refresh();
            }

            DraggableCard.SelectedCard.CardReference.CurrentSpellTargetting = 0;

            this.FireSpellBeginTargetting(this._currentSpell, data.Cell);

            InputManager.Instance.OnCellRightClickDown += _abortUsedSpell;
            InputManager.Instance.OnCellClickedUp += _processSpellClick;
        }

        private void _abortUsedSpell(CellEventData Data)
        {
            this.FireCardEndUse(
                DraggableCard.SelectedCard.CardReference,
                DraggableCard.SelectedCard,
                this._currentSpellHeader,
                null,
                false
            );

            this.FireSpellEndTargetting(
                this._currentSpell,
                Data.Cell
            );

            this._currentSpell = null;

            DraggableCard.SelectedCard.DiscardToHand();

            InputManager.Instance.OnCellRightClickDown -= _abortUsedSpell;
            InputManager.Instance.OnCellClickedUp -= _processSpellClick;
        }

        public static bool IsCellCastable(Cell cell, Spell spell)
        {
            return cell != null
                && spell.Data.TargetType.ValidateTarget(cell)
                && (
                    spell.Data.CastingMatrix == null
                    || GridUtility.IsCellWithinPlayerRange(
                        ref spell.Data.CastingMatrix,
                        CurrentPlayingEntity.EntityCell.PositionInGrid,
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

            // If the selected cell isn't of wanted type or isn't within range, same as before
            if (!IsCellCastable(Data.Cell, this._currentSpell))
                return;

            this._currentSpellHeader.TargetedCells[currentCard.CurrentSpellTargetting] = Data.Cell.PositionInGrid;
            this.FireSpellEndTargetting(this._currentSpell, Data.Cell);

            // Means that there are no more targetting spells in the array, so we finished
            if (currentCard.GetNextTargettingSpellIndex() == -1)
            {
                this.FireCardEndUse(currentCard, DraggableCard.SelectedCard, this._currentSpellHeader, Data.Cell, true);

                InputManager.Instance.OnCellRightClickDown -= _abortUsedSpell;
                InputManager.Instance.OnCellClickedUp -= _processSpellClick;
            }
            else
            {
                ScriptableCard card = DraggableCard.SelectedCard.CardReference;

                this.FireSpellBeginTargetting(
                    card.Spells[card.CurrentSpellTargetting],
                    Data.Cell
                );
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

            // End of allowed time
            if(CurrentPlayingEntity == GameManager.Instance.SelfPlayer)
            {
                NetworkManager.Instance.EntityAskToBuffAction(
                    new EndTurnAction(CurrentPlayingEntity, CurrentPlayingEntity.EntityCell)
                );
            }

            this._turnCoroutine = null;
        }

        private void _defineEntitiesTurn()
        {
            List<CharacterEntity> enemies = CurrentPlayingGrid.GridEntities
                .Where(x => !x.IsAlly)
                .ToList();
            List<CharacterEntity> players = CurrentPlayingGrid.GridEntities
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
