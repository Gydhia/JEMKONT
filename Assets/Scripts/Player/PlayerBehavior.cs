using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Inventory;
using DownBelow.Managers;
using DownBelow.Spells;
using DownBelow.UI.Inventory;
using EODE.Wonderland;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace DownBelow.Entity
{
    public class PlayerBehavior : CharacterEntity
    {
        #region EVENTS

        public event GatheringEventData.Event OnGatheringStarted;
        public event GatheringEventData.Event OnGatheringCanceled;
        public event GatheringEventData.Event OnGatheringEnded;

        public event CardEventData.Event OnCardPlayed;

        public void FireGatheringStarted(InteractableResource resource)
        {
            this.OnGatheringStarted?.Invoke(new(resource));
        }

        public void FireGatheringCanceled(InteractableResource resource = null)
        {
            this.OnGatheringCanceled?.Invoke(new(resource));
        }

        public void FireGatheringEnded(InteractableResource resource = null)
        {
            this.OnGatheringEnded?.Invoke(new(resource));
        }

        #endregion

        public BaseStorage PlayerInventory;

        private DateTime _lastTimeAsked = DateTime.Now;
        private string _nextGrid = string.Empty;

        public Interactable NextInteract = null;

        public MeshRenderer PlayerBody;
        public PhotonView PlayerView;

        public List<Cell> NextPath { get; private set; }
        public bool CanEnterGrid => true;

        public ToolItem ActiveTool;
        public BaseStorage PlayerSpecialSlot;
        public ItemPreset CurrentSelectedItem;
        public bool IsAutoAttacking = false;
        public int inventorySlotSelected = 0;

        private bool scrollBusy;
        private PlaceableItem lastPlaceable;
        [HideInInspector] public int theList= 0;
        public Deck Deck
        {
            get => testingDeck.Deck;
            set => testingDeck.Deck = value;
        }

        // TEMPORARY
        public DeckPreset testingDeck;

        public override int Mana
        {
            get => Mathf.Min(Statistics[EntityStatistics.Mana] + NumberOfTurnsPlayed,
                Statistics[EntityStatistics.MaxMana]);
        }

        #region cards constants

        public const int MAXCARDSINHAND = 7;
        public const int CARDSTOSTARTTURNWITH = 3;
        public const int MAXMANA = 6;
        public const int MAXMANAHARDCAP = 7;

        #endregion

        public override void Init(EntityStats stats, Cell refCell, WorldGrid refGrid, int order = 0)
        {
            base.Init(stats, refCell, refGrid, order);

            refGrid.GridEntities.Add(this);
            this.PlayerInventory = new BaseStorage();
            this.PlayerInventory.Init(
                SettingsManager.Instance.GameUIPreset.SlotsByPlayer[Photon.Pun.PhotonNetwork.PlayerList.Length - 1]);
            this.PlayerSpecialSlot = new BaseStorage();
            this.PlayerSpecialSlot.Init(1);

            PlayerInputs.player_scroll.performed += this._scroll;
        }

        public override void FireEnteredCell(Cell cell)
        {
            /*if (cell.ItemContained != null && cell.ItemContained.ItemPreset != null)
            {
                cell.TryPickUpItem(this);
            }
            //*/
            base.FireEnteredCell(cell);
        }

        private void _scroll(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => this.Scroll(ctx.ReadValue<float>());
        void Scroll(float value)
        {
            if (scrollBusy) return;
            scrollBusy = true;
            int newSlot = inventorySlotSelected;
            if (value >= 110)
            {
                //If we're scrolling up,
                if (inventorySlotSelected + 1 >= PlayerInventory.MaxSlots)
                {
                    //If by incrementing our selectedslot we would go over the limit; do a loop
                    newSlot = 0;
                } else
                {
                    //Increment simply
                    newSlot++;
                }
                switchSlots(inventorySlotSelected, newSlot);
            } else if (value <= -110)
            {

                if (inventorySlotSelected - 1 < 0)
                {
                    //if by decrementing we would go below 0;
                    newSlot = PlayerInventory.MaxSlots;
                } else
                {
                    //decrement
                    newSlot--;
                }

                switchSlots(inventorySlotSelected, newSlot);
            } else
            {
                //NoScrollin
            }
            scrollBusy = false;
            processEndScroll();
        }

        void switchSlots(int old, int newSlot)
        {
            UIManager.Instance.SwitchSelectedSlot(old, newSlot);
            if (newSlot == 0)
            {
                CurrentSelectedItem = ActiveTool;

                //ActiveSlot
            } else
            {
                CurrentSelectedItem = PlayerInventory.StorageItems[newSlot - 1].ItemPreset;
                //Inventory
            }
            inventorySlotSelected = newSlot;
        }

        void processEndScroll()
        {
            if(CurrentSelectedItem != null)
            {
                if (CurrentSelectedItem is PlaceableItem placeable)
                {
                    lastPlaceable = placeable;
                    InputManager.Instance.OnNewCellHovered += lastPlaceable.Previsualize;
                    InputManager.Instance.OnCellRightClickDown += lastPlaceable.Place;
                } else
                {
                    if(lastPlaceable!= null)
                    {
                        InputManager.Instance.OnNewCellHovered -= lastPlaceable.Previsualize;
                        InputManager.Instance.OnCellRightClickDown -= lastPlaceable.Place;
                        lastPlaceable = null;
                    }
                }
            } else
            {
                if(lastPlaceable!= null)
                {
                    InputManager.Instance.OnNewCellHovered -= lastPlaceable.Previsualize;
                    InputManager.Instance.OnCellRightClickDown -= lastPlaceable.Place;
                    lastPlaceable = null;
                }
            }
        }

        public void SetActiveTool(ToolItem activeTool)
        {
            activeTool.ActualPlayer = this;
            this.ActiveTool = activeTool;
            this.RefStats = ToolsManager.Instance.ToolStats[activeTool.Class];
        }

        public override void StartTurn()
        {
            base.StartTurn();
        }

        public override void EndTurn()
        {
            base.EndTurn();
        }

        #region MOVEMENTS

        //    // TODO: parse these values in a different way later
        //    if (this.CurrentGrid.IsCombatGrid) {
        //        GridManager.Instance.ShowPossibleCombatMovements(this);
        //    }
        //    if (this.NextPath != null) {
        //        this.MoveWithPath(this.NextPath, _nextGrid);
        //        this.NextPath = null;
        //    } else if (this._nextGrid != string.Empty) {
        //        // TODO: This means we shouldn't ask network to change grid, it's made up when moving
        //        if (Photon.Pun.PhotonNetwork.LocalPlayer.UserId == GameManager.Instance.SelfPlayer.PlayerID)
        //            NetworkManager.Instance.PlayerAsksToEnterGrid(this, this.CurrentGrid, this._nextGrid);
        //        this.NextCell = null;
        //        this._nextGrid = string.Empty;
        //    } else if (this.NextInteract != null) {
        //        NetworkManager.Instance.PlayerAsksToInteract(NextInteract.RefCell);
        //        this.NextInteract = null;
        //    }
        //}

        public void EnterNewGrid(CombatGrid grid)
        {
            this.healthText.gameObject.SetActive(true);
            Cell closestCell =
                GridUtility.GetClosestAvailableCombatCell(this.CurrentGrid, grid, this.EntityCell.PositionInGrid);

            while (closestCell.Datas.state != CellState.Walkable)
            {
                List<Cell> neighbours = GridManager.Instance.GetCombatNeighbours(closestCell, grid);
                closestCell = neighbours.First(cell => cell.Datas.state == CellState.Walkable);
                if (closestCell == null)
                    closestCell = neighbours[0];
            }
            theList = 0; //:)
            this.FireExitedCell();

            this.CurrentGrid = grid;

            this.FireEnteredCell(closestCell);

            this.transform.position = closestCell.WorldPosition;
        }

        public bool RespectedDelayToAsk()
        {
            return (System.DateTime.Now - this._lastTimeAsked).Seconds >=
                   SettingsManager.Instance.InputPreset.PathRequestDelay;
        }

        #endregion

        #region INTERACTIONS

        public void TakeResources(ItemPreset resource, int quantity)
        {
            this.PlayerInventory.TryAddItem(resource, quantity);
        }

        #endregion
    }
}