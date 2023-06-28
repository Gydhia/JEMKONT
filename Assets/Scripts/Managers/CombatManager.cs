using DownBelow.Spells;
using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DownBelow.Mechanics;
using DownBelow.UI;
using UnityEngine.InputSystem;
using Sirenix.Utilities;
using Photon.Realtime;
using EODE.Wonderland;

namespace DownBelow.Managers {
	public class CombatManager : _baseManager<CombatManager> {
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

		public void FireCombatStarted(WorldGrid Grid) {
			PlayerInputs.player_select_1.canceled += this._switchToFirstPlayer;
			PlayerInputs.player_select_2.canceled += this._switchToSecondPlayer;
			PlayerInputs.player_select_3.canceled += this._switchToThirdPlayer;
			PlayerInputs.player_select_4.canceled += this._switchToFourthPlayer;
			PlayerInputs.player_reselect.canceled += this._switchToSelfPlayer;


			this.OnCombatStarted?.Invoke(new GridEventData(Grid));
		}

		public void FireCombatEnded(WorldGrid Grid, bool AllyVictory) 
		{
			NetworkManager.Instance.EndOfCombat();

			BattleGoing = false;
			CurrentPlayingGrid.HasStarted = false;

			PlayerInputs.player_select_1.canceled -= this._switchToFirstPlayer;
			PlayerInputs.player_select_2.canceled -= this._switchToSecondPlayer;
			PlayerInputs.player_select_3.canceled -= this._switchToThirdPlayer;
			PlayerInputs.player_select_4.canceled -= this._switchToFourthPlayer;
			PlayerInputs.player_reselect.canceled -= this._switchToSelfPlayer;

			foreach (var fake in this.FakePlayers) {
				Destroy(fake.gameObject);
			}
			this.FakePlayers.Clear();

			if (AllyVictory) 
			{
				AkSoundEngine.PostEvent("Play_SSFX_CombatWin", AudioHolder.Instance.gameObject);

				string abyssName = (Grid as CombatGrid).ParentGrid.UName;
				var abyss = SettingsManager.Instance.AbyssesPresets.Find(x => x.name == abyssName);

				if (!abyss.IsCleared) {
					abyss.GiftCards();
					abyss.IsCleared = true;
					GameManager.MaxAbyssReached++;
				}
			}
			else
			{
				AkSoundEngine.PostEvent("Play_SSFX_CombatLose", AudioHolder.Instance.gameObject);
			}

			GameManager.SelfPlayer = GameManager.RealSelfPlayer;

			CurrentPlayingGrid.ResetGrid();

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

        public int EntityTurnRotation;
        public int TotalTurnNumber;

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
		private int _playerIndex = 0;

		public bool IsPlayerOrOwned(CharacterEntity entity) {
			if (entity is PlayerBehavior player) {
				return GameManager.RealSelfPlayer == player ||
					(player.IsFake && player.Owner == GameManager.RealSelfPlayer);
			}
			else {
				return false;
			}
		}
		#endregion

		public void Init() {
			GameManager.Instance.OnEnteredGrid += this.WelcomePlayerInCombat;
			this.OnCardBeginUse += this._beginUseSpell;
		}

		/// <summary>
		/// To welcome any player entering a combat grid.
		/// </summary>
		/// <param name="Data"></param>
		public void WelcomePlayerInCombat(EntityEventData Data) 
		{
			if (!Data.Entity.CurrentGrid.IsCombatGrid)
				return;

			CombatGrid currentGrid = Data.Entity.CurrentGrid as CombatGrid;

			PlayerBehavior player = Data.Entity as PlayerBehavior;

			this.PlayersInGrid.Add(player);

			int totalTools = CardsManager.Instance.AvailableTools.Count;

			Dictionary<PlayerBehavior, List<ToolItem>> allyTools = new Dictionary<PlayerBehavior, List<ToolItem>>();
			List<ToolItem> allTools = CardsManager.Instance.AvailableTools.ToList();
			List<ToolItem> freeTools = new List<ToolItem>();

			List<PlayerBehavior> playersToLog = new();

			int usedTools = 0;
			foreach (var netPlayer in GameManager.Instance.Players.Values) {
				netPlayer.CombatTools.Clear();

				if (netPlayer.CurrentGrid.IsCombatGrid) {
					netPlayer.CombatTools.AddRange(netPlayer.ActiveTools);
					allyTools.Add(netPlayer, new List<ToolItem>(netPlayer.ActiveTools));
					usedTools += netPlayer.ActiveTools.Count;
					playersToLog.Add(netPlayer);
				}
				else {
					freeTools.AddRange(netPlayer.ActiveTools);
				}

				foreach (var rTool in netPlayer.ActiveTools)
					allTools.Remove(rTool);
			}

			// Merge the tools on ground to the equiped tools
			freeTools.AddRange(allTools);

			if (usedTools < totalTools)
			{
				int playersInGrid = PlayersInGrid.Count;
				int playerIndex = 0;

				foreach (var tool in freeTools)
				{
					var playerToAdd = this.PlayersInGrid[playerIndex];

					playerToAdd.CombatTools.Add(tool);
					allyTools[playerToAdd].Add(tool);

					playerIndex = (playerIndex + 1 >= playersInGrid) ? 0 : playerIndex + 1;
				}
			}

			if (FakePlayers != null && this.FakePlayers.Count > 0) 
			{
				foreach (var fake in FakePlayers) 
				{
					fake.FireExitedCell();
					Destroy(fake.gameObject);
				}
				FakePlayers.Clear();
			}

			// We replaced a destroyed fake player. We need to tell the tool that we're now its owner
			player.ActiveTool.ActualPlayer = player;
			player.ActiveTool.DeckPreset.LinkedPlayer = player;

			FakePlayers = new List<PlayerBehavior>();
			foreach (var playerDecks in allyTools) {
				// Each client has one deck or more. Create fake players for each deck excepting the first one
				foreach (var tool in playerDecks.Value.Skip(1)) {
					var fakePlayer = this._createFakePlayer(currentGrid, tool, playerDecks.Key);
					this.FakePlayers.Add(fakePlayer);
					playersToLog.Add(fakePlayer);
				}
			}

			Cell playerCell = currentGrid.PlacementCells.First(c => c.Datas.state != CellState.EntityIn);
			player.FireExitedCell();
			player.FireEnteredCell(playerCell);
			player.transform.position = playerCell.WorldPosition;

			player.ReinitializeAllStats();

			if (GameManager.SelfPlayer == Data.Entity) {
				PoolManager.Instance.CellIndicatorPool.DisplayPathIndicators(currentGrid.PlacementCells);
			}
			foreach (var item in playersToLog) {
				Logpad.Log($"Print current {item.ActiveTool.Class} status", () => Debug.Log(item.ToString()));
			}

		}

		private PlayerBehavior _createFakePlayer(CombatGrid currentGrid, ToolItem toolToAssign, PlayerBehavior owner) {
			var fakePlayer = Instantiate(GameManager.Instance.PlayerPrefab, GameManager.Instance.gameObject.transform);

			Cell placementCell = currentGrid.PlacementCells.First(c => c.Datas.state != CellState.EntityIn);
			fakePlayer.Init(placementCell, currentGrid, toolToAssign, owner);

			return fakePlayer;
		}

		public void StartCombat(CombatGrid startingGrid) {
			if (CurrentPlayingGrid != null && CurrentPlayingGrid.HasStarted)
				return;

			this.BattleGoing = true;
			CurrentPlayingGrid = startingGrid;

			AkSoundEngine.PostEvent("Set_Layer_0", AudioHolder.Instance.gameObject);
			this._setupEnemyEntities();

			UIManager.Instance.PlayerInfos.Init();

            this.EntityTurnRotation = -1;
            CurrentPlayingGrid.HasStarted = true;

			this._defineEntitiesTurn();
			this._subcribeToEntitiesDeath();

			this._switchToFirstPlayer(new InputAction.CallbackContext());

			this.FireCombatStarted(CurrentPlayingGrid);

			StartCoroutine(this._startCombatDelay(2f));

			//  UIManager.Instance.CardSection.OnCharacterSwitch += _abortUsedSpell;
		}

		private IEnumerator _startCombatDelay(float time) {
			yield return new WaitForSeconds(time);

			NetworkManager.Instance.StartEntityTurn();
		}

		public void SwitchSelectedPlayer(int index) => this._switchSelectedPlayer(index);
		private void _switchToFirstPlayer(InputAction.CallbackContext ctx) => this._switchSelectedPlayer(0);
		private void _switchToSecondPlayer(InputAction.CallbackContext ctx) => this._switchSelectedPlayer(1);
		private void _switchToThirdPlayer(InputAction.CallbackContext ctx) => this._switchSelectedPlayer(2);
		private void _switchToFourthPlayer(InputAction.CallbackContext ctx) => this._switchSelectedPlayer(3);
		private void _switchToSelfPlayer(InputAction.CallbackContext ctx) 
		{
			if (CurrentPlayingEntity is PlayerBehavior player && IsPlayerOrOwned(player))
			{
				this._switchSelectedPlayer(player);
			}
		}
		private void _switchSelectedPlayer(PlayerBehavior player) 
		{
			this._switchSelectedPlayer(player.PlayerIndex);
		}

		private void _switchSelectedPlayer(int index) 
		{
			var player = this.FakePlayers.SingleOrDefault(f => f.PlayerIndex == index);
			player ??= GameManager.RealSelfPlayer.PlayerIndex == index ? GameManager.RealSelfPlayer : null;

			if (player == null)
				return;

			if (IsPlayerOrOwned(player)) 
			{
				GameManager.Instance.FireSelfPlayerSwitched(
					player,
					this._playerIndex,
					index
				);

				this._playerIndex = index;
			}
		}

		public void ProcessStartTurn() {
			if (!BattleGoing) {
				return;
			}

            this.EntityTurnRotation++;

            int entityIndex = 0;
            // Only try to get the next one if it's the the last one
            if (CurrentPlayingEntity != null && CurrentPlayingEntity.TurnOrder < this.PlayingEntities.Count)
            {
                int startIndex = this.PlayingEntities.IndexOf(CurrentPlayingEntity) + 1;
                for (int i = startIndex; i < this.PlayingEntities.Count; i++)
                {
                    if(this.PlayingEntities[i].Health > 0)
                    {
                        entityIndex = this.PlayingEntities.IndexOf(this.PlayingEntities[i]);
                        break;
                    }
                }
            }

            CurrentPlayingEntity = this.PlayingEntities[entityIndex];

            if (this.EntityTurnRotation >= 0)
            {
                UIManager.Instance.TurnSection.ChangeSelectedEntity(entityIndex);
            }
                
            if (CurrentPlayingEntity is PlayerBehavior player)
            {
                this._turnCoroutine = StartCoroutine(this._startTurnTimer());

				// Auto switch the current playing entity
				if (this.IsPlayerOrOwned(player)) {
					this._switchSelectedPlayer(player);
				}
			}

            this.OnTurnStarted?.Invoke(new EntityEventData(CurrentPlayingEntity));
        }


		public void ProcessEndTurn() {
			CurrentPlayingEntity.EndTurn();

			// Reset the time slider
			if (this._turnCoroutine != null) {
				StopCoroutine(this._turnCoroutine);
				this._turnCoroutine = null;
			}

			UIManager.Instance.TurnSection.TimeSlider.fillAmount = 0f;

			this.OnTurnEnded?.Invoke(new EntityEventData(CurrentPlayingEntity));
		}

		private void _setupEnemyEntities() {
			foreach (CharacterEntity enemy in CurrentPlayingGrid.GridEntities.Where(e => !e.IsAlly)) {
				enemy.ReinitializeAllStats();
				enemy.EntityCell.EntityIn = enemy;
				enemy.gameObject.SetActive(true);
			}
		}

		#region CARDS
		private void _beginUseSpell(CardEventData data) 
		{
			if (data.Card.Spells == null) {
				Debug.LogError("Trying to use a card without Spell. Fix it in editor.");
			}

			// Create the spell header, used to store only usefull datas (=avoid duplications)
			this._currentSpellHeader = new SpellHeader(data.Card.UID, data.Card.Spells.Length, CurrentPlayingEntity.UID);
			this._currentSpell = data.Card.Spells.FirstOrDefault(s => s.Data.RequiresTargetting);
			// If no targeting required
			bool targeting = this._currentSpell != null;
			this._currentSpell ??= data.Card.Spells[0];

			foreach (var spell in data.Card.Spells) {
				spell.Data.Refresh();
			}

			DraggableCard.SelectedCard.CardReference.CurrentSpellTargetting = 0;

			if (targeting) {
				this.FireSpellBeginTargetting(this._currentSpell, data.Cell);

				InputManager.Instance.OnCellClickedUp += _processSpellClick;

				UIManager.Instance.CardSection.OnCharacterSwitch += AbortUsedSpell;
			}
			else 
			{
				_currentSpellHeader.TargetedCells[0] = CurrentPlayingEntity.EntityCell.PositionInGrid;
				this.FireCardEndUse(data.Card, DraggableCard.SelectedCard, this._currentSpellHeader, CurrentPlayingEntity.EntityCell, true);
			}
		}

		public void AbortUsedSpell(CellEventData Data) 
		{
			if (DraggableCard.SelectedCard != null) {
				this.FireCardEndUse(
				DraggableCard.SelectedCard.CardReference,
				DraggableCard.SelectedCard,
				this._currentSpellHeader,
				null,
				false
			);

			}
			if (this._currentSpell != null && Data.Cell != null) {
				this.FireSpellEndTargetting(
								this._currentSpell,
								Data.Cell
							);
			}

			this._currentSpell = null;
			if (DraggableCard.SelectedCard != null) {
				DraggableCard.SelectedCard.DiscardToHand();
			}

			InputManager.Instance.OnCellClickedUp -= _processSpellClick;
			UIManager.Instance.CardSection.OnCharacterSwitch -= AbortUsedSpell;

		}

		public static bool IsCellInSpell(Cell cell) {
			var spell = Instance._currentSpell;

			return cell != null
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

		public static bool IsCellCastable(Cell cell, Spell spell) {
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

		private void _processSpellClick(CellEventData Data) {
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
			if (currentCard.GetNextTargettingSpellIndex() == -1) {
				this.FireCardEndUse(currentCard, DraggableCard.SelectedCard, this._currentSpellHeader, Data.Cell, true);

				InputManager.Instance.OnCellClickedUp -= _processSpellClick;
			}
			else {
				this._currentSpell = currentCard.Spells[currentCard.CurrentSpellTargetting];

				this.FireSpellBeginTargetting(
					this._currentSpell,
					Data.Cell
				);
			}
		}


		#endregion

		private IEnumerator _startTurnTimer() {
#if UNITY_EDITOR
			float time = SettingsManager.Instance.CombatPreset.EditorTurnTime;
#else
            float time = SettingsManager.Instance.CombatPreset.TurnTime;
#endif
			float timePassed = 0f;

			UIManager.Instance.TurnSection.TimeSlider.fillAmount = 0f;
			//UIManager.Instance.TurnSection.TimeSlider.maxValue = time;

			while (timePassed <= time) {
				yield return new WaitForSeconds(Time.deltaTime);
				timePassed += Time.deltaTime;

				float timeToShowOnSlider = timePassed / time;

				UIManager.Instance.TurnSection.TimeSlider.fillAmount = timeToShowOnSlider;
			}

			// End of allowed time, only by the master client to avoid multiple buffing
			if (Photon.Pun.PhotonNetwork.IsMasterClient && this.IsPlayerOrOwned(CurrentPlayingEntity)) {
				NetworkManager.Instance.EntityAskToBuffAction(
					new EndTurnAction(CurrentPlayingEntity, CurrentPlayingEntity.EntityCell)
				);
			}

			this._turnCoroutine = null;
		}

		private void _defineEntitiesTurn() {
			List<CharacterEntity> enemies = CurrentPlayingGrid.GridEntities
				.Where(x => !x.IsAlly)
				.ToList();
			List<PlayerBehavior> players = CurrentPlayingGrid.GridEntities
				.Where(x => x.IsAlly)
				.Cast<PlayerBehavior>()
				.ToList();

            this.PlayingEntities = new List<CharacterEntity>();

            int indexIncr = 0;
			int turnOrder = 0;
            // We check both for the tests, if we have more allies than ennemies or inverse
            if (enemies.Count >= players.Count)
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if(i < players.Count)
                    {
                        players[i].PlayerIndex = indexIncr++;
							
						this.PlayingEntities.Add(players[i]);

                        this.PlayingEntities[^1].TurnOrder = turnOrder++;
                    }

                    if (i < enemies.Count)
                    {
                        this.PlayingEntities.Add(enemies[i]);
                        this.PlayingEntities[^1].TurnOrder = turnOrder++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if(i < enemies.Count)
                    {
                        this.PlayingEntities.Add(enemies[i]);
                        this.PlayingEntities[^1].TurnOrder = turnOrder++;
                    }

					if (i < players.Count) {
						
						players[i].PlayerIndex = indexIncr++;

						this.PlayingEntities.Add(players[i]);
                        this.PlayingEntities[^1].TurnOrder = turnOrder++;
                    }
                }
            }
        }

		private void _subcribeToEntitiesDeath() {
			foreach (var entity in this.PlayingEntities) {
				entity.OnDeath += _makeEntityDie;
			}
		}

		private void _makeEntityDie(EntityEventData Data) {
			this.FireEntityDeath(Data.Entity);

			this.PlayingEntities.Remove(Data.Entity);
			this.DeadEntities.Add(Data.Entity);

			Data.Entity.Die();

            // all Allies dead
            if (PlayingEntities.Count(p => p.IsAlly) == 0)
            {
                this.FireCombatEnded(CurrentPlayingGrid, false);
            }
            // all Enemies dead
            else if (PlayingEntities.Count(p => !p.IsAlly) == 0)
            {
                this.FireCombatEnded(CurrentPlayingGrid, true);
            }
        }
    }
}
