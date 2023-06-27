using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.UI.Inventory;
using EasyTransition;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.Entity
{
    public class PlayerBehavior : CharacterEntity
    {
        #region EVENTS
        public event GatheringEventData.Event OnGatheringStarted;
        public event GatheringEventData.Event OnGatheringEnded;

        public event CardEventData.Event OnCardPlayed;


        public void FireGatheringStarted(InteractableResource resource)
        {
            this.OnGatheringStarted?.Invoke(new GatheringEventData(resource));
        }

        public void FireGatheringEnded(InteractableResource resource)
        {
            this.OnGatheringEnded?.Invoke(new GatheringEventData(resource));
        }
        #endregion
        /// <summary>
        /// The owner of this potential FakePlayer. Used in combat
        /// </summary>
        public PlayerBehavior Owner;
        public bool IsFake = false;
        public int Index = -1;

        public BaseStorage PlayerInventory;

        private DateTime _lastTimeAsked = DateTime.Now;

        public Interactable NextInteract = null;

        public Transform ToolHolder;
        public PhotonView PlayerView;


        public ToolItem ActiveTool => ActiveTools.Count > 0 ? ActiveTools[0] : null;
        public ToolItem CombatTool => CombatTools[0];
        /// <summary>
        /// Each possessed tools that differs according to number of players
        /// </summary>
        public List<ToolItem> ActiveTools = new List<ToolItem>();
        /// <summary>
        /// Each tools set for combat. If in combat alone in a 2 players room, there will be 4 tools.
        /// </summary>
        public List<ToolItem> CombatTools = new List<ToolItem>();

        public BaseStorage PlayerSpecialSlots;
        public bool IsAutoAttacking = false;
        public Outlining.Outline ToolOutline;
        public Outlining.Outline PlayerOutline;

        public void ShowOutline(bool show)
        {
            if (show)
            {
                this.ToolOutline.enabled = true;
                InputManager.Instance.OnNewCellHovered += this.OutlineChange;
            }
            else
            {
                this.ToolOutline.enabled = false;
                InputManager.Instance.OnNewCellHovered -= this.OutlineChange;
            }
        }

        private void OutlineChange(CellEventData Data)
        {
            if (Data.Cell == EntityCell)
            {
                ToolOutline.enabled = true;
                ToolOutline.OutlineColor = SettingsManager.Instance.GameUIPreset.ToolHoverColor;
            }
            else
            {
                ToolOutline.OutlineColor = SettingsManager.Instance.GameUIPreset.ToolAttackColor;
            }
        }

        [HideInInspector] public int theList = 0;
        public DeckPreset Deck
        {
            get
            {
                return (this.CombatTool == null || CombatTool.DeckPreset == null) ?
                    null :
                    CombatTool.DeckPreset;
            }
        }


        public override int Mana
        {
            get => Mathf.Min(Statistics[EntityStatistics.Mana] + NumberOfTurnsPlayed,
                Statistics[EntityStatistics.Mana]) + Buff(EntityStatistics.Mana);
        }

        public bool CanGatherThisResource(EClass resourceClass)
        {
            return this.ActiveTools.Count > 0 && this.ActiveTools.Any(a => a.Class == resourceClass);
        }

        #region cards constants

        public const int MAXCARDSINHAND = 7;
        public const int CARDSTOSTARTTURNWITH = 3;
        public const int MAXMANA = 6;
        public const int MAXMANAHARDCAP = 7;

        #endregion

        /// <summary>
        /// Used by fake players
        /// </summary>
        /// <param name="refCell"></param>
        /// <param name="refGrid"></param>
        public void Init(Cell refCell, WorldGrid refGrid, ToolItem refItem, PlayerBehavior owner)
        {
            this.IsFake = true;
            this.Owner = owner;

            this.SetActiveTool(refItem);
            this.CombatTools.Add(refItem);
            this.ReinitializeAllStats();

            this.name = "FakePlayer - " + refItem.Class.ToString();
            this.UID = owner.UID + refItem.Class.ToString();

            this.Init(refCell, refGrid);
        }

        public override void Init(Cell refCell, WorldGrid refGrid, int order = 0)
        {
            base.Init(refCell, refGrid, order);

            if (this.RefStats == null)
            {
                this.SetStatistics(SettingsManager.Instance.CombatPreset.EmptyStatistics, false);
            }

            refGrid.GridEntities.Add(this);

            if (this.IsFake)
            {
                this.FireEntityInited();
                return;
            }

            int playersNb = PhotonNetwork.PlayerList.Length;

            this.SetCharacterVisuals(null);

            this.PlayerInventory = new BaseStorage();
            this.PlayerInventory.Init(
                SettingsManager.Instance.GameUIPreset.SlotsByPlayer[playersNb - 1]);

            int toolSlots = 4;
            if (playersNb == 2) toolSlots = 2;
            if (playersNb == 3) toolSlots = PhotonNetwork.IsMasterClient ? 2 : 1;
            if (playersNb == 4) toolSlots = 1;

            this.PlayerSpecialSlots = new BaseStorage();
            this.PlayerSpecialSlots.Init(toolSlots);

            this.FireEntityInited();
        }

        public override void FireEnteredCell(Cell cell)
        {
            base.FireEnteredCell(cell);
        }

        public void SetActiveTool(ToolItem activeTool)
        {
            activeTool.ActualPlayer = this;
            activeTool.DeckPreset.LinkedPlayer = this;

            // Only set the player stats from one tool, the first one picked up
            if (this.ActiveTool == null)
            {
                this.ActiveTools.Add(activeTool);
                this.SetStatistics(activeTool.DeckPreset.Statistics);
                this.SetCharacterVisuals(activeTool);

                this.ToolOutline = this.ToolHolder.GetComponentInChildren<Outlining.Outline>();
                this.ToolOutline.enabled = false;
                this.ToolOutline.OutlineColor = SettingsManager.Instance.GameUIPreset.ToolAttackColor;
            }
            else
            {
                this.ActiveTools.Add(activeTool);
            }
        }

        public void RemoveActiveTool(ToolItem removedTool)
        {
            removedTool.ActualPlayer = null;
            removedTool.DeckPreset.LinkedPlayer = null;

            bool isCurrentTool = removedTool == this.ActiveTool;

            this.ActiveTools.Remove(removedTool);

            if (isCurrentTool || this.ActiveTool == null)
            {
                this.SetStatistics(this.ActiveTool ? this.ActiveTool.DeckPreset.Statistics : SettingsManager.Instance.CombatPreset.EmptyStatistics);
                this.SetCharacterVisuals(this.ActiveTool);
            }
        }

        public void SetCharacterVisuals(ToolItem toolRef)
        {
            foreach (Transform child in this.ToolHolder)
            {
                Destroy(child.gameObject);
            }

            if (toolRef != null)
            {
                ToolOnGround tool = Instantiate(toolRef.DroppedItemPrefab, this.ToolHolder).GetComponent<ToolOnGround>();
                tool.Init(false);

                var outline = tool.GetComponentInChildren<Outlining.Outline>();
                outline.enabled = false;
            }

            // Skin
            this.Renderer.materials[0].SetTexture("_BaseMap", toolRef ? toolRef.CharacterTexture : SettingsManager.Instance.CombatPreset.WhiteCharacter);
            this.Renderer.materials[0].mainTexture = toolRef ? toolRef.CharacterTexture : SettingsManager.Instance.CombatPreset.WhiteCharacter;
            // Hairs
            this.Renderer.materials[1].SetColor("_BaseColor", toolRef ? toolRef.ToolRefColor : SettingsManager.Instance.CombatPreset.WhiteColor);
        }

        public override void StartTurn()
        {
            ShowOutline(true);
            base.StartTurn();
        }

        public override void EndTurn()
        {
            ShowOutline(false);
            base.EndTurn();
        }

        public override string ToString()
        {
            var res = base.ToString() + "\n";
            res += $"Class: {ActiveTool?.Class}";
            return res;
        }
        #region MOVEMENTS

        public void EnterNewGrid(WorldGrid grid)
        {
            // TODO : Killian (it's me Killian) plug this somewhere else
            theList = 0; //:)
            this.CurrentGrid = grid;
        }

        public bool RespectedDelayToAsk()
        {
            return (System.DateTime.Now - this._lastTimeAsked).Seconds >=
                   SettingsManager.Instance.InputPreset.PathRequestDelay;
        }

        #endregion

        #region ATTACKS

        /// <summary>
        /// Tries to attack the given cell.
        /// </summary>
        /// <param name="cellToAttack">The cell to attack.</param>
        public void AutoAttack(Cell cellToAttack)
        {
            this.CanAutoAttack = false;

            //Normally already verified. Just in case
            //Calculate straight path, see if obstacle.  
            var path = GridManager.Instance.FindPath(this, cellToAttack.PositionInGrid, true);

            var notwalkable = path.Find(x => x.Datas.state != CellState.Walkable);
            bool attacked = false;
            if (notwalkable != null)
            {
                switch (notwalkable.Datas.state)
                {
                    case CellState.EntityIn:
                        NetworkManager.Instance.EntityAskToBuffAction(new AttackingAction(this, notwalkable));
                        attacked = true;
                        break;
                }
            }
            else
            {
                NetworkManager.Instance.EntityAskToBuffAction(new AttackingAction(this, cellToAttack));
                attacked = true;
            }

            if (attacked)
            {
                this.ToolOutline.enabled = false;
            }
        }
        #endregion

        #region INTERACTIONS

        public void TeleportToGrid(string gridName)
        {
            this.StartCoroutine(this._playTeleport(gridName));
        }

        private IEnumerator _playTeleport(string gridName)
        {
            this.CanMove = false;

            var tSett = SettingsManager.Instance.BaseTransitionSettings;

            TransitionManager.Instance().Transition(tSett, 0f);
            yield return new WaitForSeconds(tSett.transitionTime / 2f);

            this._teleportToGrid(gridName);
        }

        private void _teleportToGrid(string gridName)
        {
            if (GridManager.Instance.WorldGrids.TryGetValue(gridName, out WorldGrid grid))
            {
                var gridAction = new EnterGridAction(GameManager.RealSelfPlayer, grid.Cells[0, 0]);
                gridAction.Init(gridName);

                NetworkManager.Instance.EntityAskToBuffAction(gridAction);
            }
            else {
                Debug.LogError($"COULD NOT FIND GRID {gridName}.");
            }

            GameManager.RealSelfPlayer.CanMove = true;
        }

		#endregion

		public override void Die() {
            ShowOutline(false);
			base.Die();
		}
	}
}