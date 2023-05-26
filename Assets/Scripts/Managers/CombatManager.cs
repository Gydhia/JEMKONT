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
using UnityEngine.Rendering;
using System.Runtime.Remoting.Messaging;

namespace DownBelow.Managers
{
    public class CombatManager : _baseManager<CombatManager>
    {
        #region EVENTS
        public event GridEventData.Event OnCombatStarted;
        public event GridEventData.Event OnCombatEnded;
        public event EntityEventData.Event OnTurnStarted;
        public event EntityEventData.Event OnTurnEnded;
        public event EntityEventData.Event OnEntityDeath;

        public event CardEventData.Event OnCardBeginUse;
        public event CardEventData.Event OnCardEndUse;

        public event SpellTargetEventData.Event OnSpellBeginTargetting;
        public event SpellTargetEventData.Event OnSpellEndTargetting;

        public void FireCombatStarted(WorldGrid Grid)
        {
            PlayerInputs.player_select_1.canceled += this._switchToFirstPlayer;
            PlayerInputs.player_select_2.canceled += this._switchToSecondPlayer;
            PlayerInputs.player_select_3.canceled += this._switchToThirdPlayer;
            PlayerInputs.player_select_4.canceled += this._switchToFourthPlayer;
            PlayerInputs.player_reselect.canceled += this._switchToSelfPlayer;

            this.OnCombatStarted?.Invoke(new GridEventData(Grid));
        }

        public void FireCombatEnded(WorldGrid Grid, bool AllyVictory)
        {
            CurrentPlayingGrid.HasStarted = false;

            PlayerInputs.player_select_1.canceled -= this._switchToFirstPlayer;
            PlayerInputs.player_select_2.canceled -= this._switchToSecondPlayer;
            PlayerInputs.player_select_3.canceled -= this._switchToThirdPlayer;
            PlayerInputs.player_select_4.canceled -= this._switchToFourthPlayer;
            PlayerInputs.player_reselect.canceled -= this._switchToSelfPlayer;

            GameManager.Instance.FireSelfPlayerSwitched(null, this._playerIndex, 0);

            this.OnCombatEnded?.Invoke(new GridEventData(Grid, AllyVictory));
        }

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

        public void FireEntityDeath(CharacterEntity Entity) => this.OnEntityDeath?.Invoke(new EntityEventData(Entity));
        #endregion

        public bool BattleGoing;
        private SpellHeader _currentSpellHeader;
        private Spell _currentSpell;

        public int TurnNumber;

        #region Run-time
        private Coroutine _turnCoroutine;

        public static CharacterEntity CurrentPlayingEntity;
        public static CombatGrid CurrentPlayingGrid;
        public List<CharacterEntity> PlayingEntities;
        public List<CharacterEntity> DeadEntities;

        public List<PlayerBehavior> PlayersInGrid = new List<PlayerBehavior>();

        public List<NonCharacterEntity> NCEs;
        /// <summary>
        /// Both used in the setup phase and the playing phase. 
        /// Before combat, this is only used as a placeholder
        /// </summary>
        public List<PlayerBehavior> FakePlayers;
        private int _playerIndex = -1;

        public bool IsPlayerOrOwned(CharacterEntity entity)
        {
            if (entity is PlayerBehavior player)
            {
                return GameManager.RealSelfPlayer == player ||
                    (player.IsFake && player.Owner == GameManager.RealSelfPlayer);
            }
            else
            {
                return false;
            }
        }
        #endregion

        public void Init()
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

            // Take the first tool 
            List<DeckPreset> realAlliesDeck = new List<DeckPreset>();
            List<DeckPreset> fakeAlliesDeck = new List<DeckPreset>();
            foreach (var netPlayer in GameManager.Instance.Players.Values)
            {
                if(netPlayer != player && netPlayer.CurrentGrid is CombatGrid)
                {
                    realAlliesDeck.Add(((ToolItem)netPlayer.PlayerSpecialSlots.StorageItems[0].ItemPreset).DeckPreset);
                    for (int i = 1; i < netPlayer.PlayerSpecialSlots.StorageItems.Length; i++)
                    {
                        fakeAlliesDeck.Add(((ToolItem)netPlayer.PlayerSpecialSlots.StorageItems[i].ItemPreset).DeckPreset);
                    }
                }
            }

            var selfTool = ((ToolItem)player.PlayerSpecialSlots.StorageItems[0].ItemPreset);
            player.SetActiveTool(selfTool);

            var selfFakeDecks = player.PlayerSpecialSlots.StorageItems.Select(s => (s.ItemPreset as ToolItem).DeckPreset).Where(t => t != selfTool.DeckPreset);

            // If FakePlayers isn't populated (=init), that's a local action
            if (this.FakePlayers == null || this.FakePlayers.Count == 0)
            {
                foreach (var fakeDeck in selfFakeDecks.Union(fakeAlliesDeck))
                {
                    var fakePlayer = Instantiate(GameManager.Instance.PlayerPrefab, GameManager.Instance.gameObject.transform);

                    Cell placementCell = currentGrid.PlacementCells.First(c => c.Datas.state != CellState.EntityIn);

                    fakePlayer.Init(placementCell, currentGrid, 0, true);

                    var refTool = CardsManager.Instance.ToolPresets.Values.Single(t => t.DeckPreset == fakeDeck);
                    fakePlayer.SetActiveTool(refTool);

                    fakePlayer.ReinitializeAllStats();

                    fakePlayer.name = "FakePlayer - " + refTool.DeckPreset.Class.ToString();
                    fakePlayer.UID = fakePlayer.name;

                    this.FakePlayers.Add(fakePlayer);
                }
            }
            // If already populated, remove the corresponding 
            else
            {
                // Replace the fake on by the real player
                var fakeToReplace = this.FakePlayers.FirstOrDefault(f => f.Deck == player.Deck);

                if (fakeToReplace != null)
                {
                    fakeToReplace.FireExitedCell();
                    this.FakePlayers.Remove(fakeToReplace);

                    Destroy(fakeToReplace.gameObject);
                }
            }

            Cell playerCell = currentGrid.PlacementCells.First(c => c.Datas.state != CellState.EntityIn);
            player.FireExitedCell();
            player.FireEnteredCell(playerCell);
            player.transform.position = playerCell.WorldPosition;

            player.ReinitializeAllStats();

            if(GameManager.SelfPlayer == Data.Entity)
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
            this._setOwnedFakes();
            this._subcribeToEntitiesDeath();

            this._switchToFirstPlayer(new InputAction.CallbackContext());

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
        private void _switchToSelfPlayer(InputAction.CallbackContext ctx) 
        { 
            if(CurrentPlayingEntity is PlayerBehavior player && IsPlayerOrOwned(player))
            {
                this._switchSelectedPlayer(player);
            }
        }
        private void _switchSelectedPlayer(PlayerBehavior player)
        {
            this._switchSelectedPlayer(player.IsFake ? this.FakePlayers.IndexOf(player) + 1 : 0);
        }

        private void _switchSelectedPlayer(int index)
        {
            if(index > this.FakePlayers.Count || index == this._playerIndex) { return; }

            var player = index == 0 ? GameManager.RealSelfPlayer : this.FakePlayers[index - 1];

            if (IsPlayerOrOwned(player))
            {
                GameManager.Instance.FireSelfPlayerSwitched(
                    index == 0 ? null : this.FakePlayers[index - 1],
                    this._playerIndex < 0 ? 0 : this._playerIndex,
                    index
                );

                this._playerIndex = index;
            }
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

            if (CurrentPlayingEntity is PlayerBehavior player)
            {
                this._turnCoroutine = StartCoroutine(this._startTurnTimer());

                // Auto switch the current playing entity
                if (this.IsPlayerOrOwned(player))
                {
                    this._switchSelectedPlayer(player);
                }
            }


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

        private void _setupEnemyEntities()
        {
            foreach (CharacterEntity enemy in CurrentPlayingGrid.GridEntities.Where(e => !e.IsAlly))
            {
                enemy.ReinitializeAllStats();
                enemy.EntityCell.EntityIn = enemy;
                enemy.gameObject.SetActive(true);
            }
        }

        #region CARDS
        private void _beginUseSpell(CardEventData data)
        {
            if(data.Card.Spells == null)
            {
                Debug.LogError("Trying to use a card without Spell. Fix it in editor.");
            }

            // Create the spell header, used to store only usefull datas (=avoid duplications)
            this._currentSpellHeader = new SpellHeader(data.Card.UID, data.Card.Spells.Length, CurrentPlayingEntity.UID);
            this._currentSpell = data.Card.Spells.FirstOrDefault(s => s.Data.RequiresTargetting);

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
                this._currentSpell = currentCard.Spells[currentCard.CurrentSpellTargetting];

                this.FireSpellBeginTargetting(
                    this._currentSpell,
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
            
            // End of allowed time, only by the master client to avoid multiple buffing
            if(Photon.Pun.PhotonNetwork.IsMasterClient && this.IsPlayerOrOwned(CurrentPlayingEntity))
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

        private void _setOwnedFakes()
        {
            // We'll now only use it to storage self player fakes
            this.FakePlayers.Clear();
            foreach (PlayerBehavior fakePlayer in PlayingEntities.Where(e => e is PlayerBehavior player && player.IsFake))
            {
                // Find the owner
                var owner = GameManager.Instance.Players.Values.SingleOrDefault(p => p.CombatTools.Contains(fakePlayer.ActiveTool));
                fakePlayer.Owner = owner;
                this.FakePlayers.Add(fakePlayer);
            }
        }

        private void _subcribeToEntitiesDeath()
        {
            foreach (var entity in this.PlayingEntities)
            {
                entity.OnDeath += _makeEntityDie;
            }
        }

        private void _makeEntityDie(EntityEventData Data)
        {
            this.PlayingEntities.Remove(Data.Entity);
            this.DeadEntities.Add(Data.Entity);

            Data.Entity.Die();

            this.FireEntityDeath(Data.Entity);

            // all Allies dead
            if(PlayingEntities.Count(p => p.IsAlly) == 0)
            {
                this.FireCombatEnded(CurrentPlayingGrid, false);
            }
            // all Enemies dead
            else if(PlayingEntities.Count(p => !p.IsAlly) == 0)
            {
                this.FireCombatEnded(CurrentPlayingGrid, true);
            }
        }
    }
}
