using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Inventory;
using DownBelow.Managers;
using DownBelow.Spells;
using DownBelow.UI.Inventory;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        /// <summary>
        /// The owner of this potential FakePlayer. Used in combat
        /// </summary>
        public PlayerBehavior Owner;
        public bool IsFake = false;

        public BaseStorage PlayerInventory;

        private DateTime _lastTimeAsked = DateTime.Now;

        public Interactable NextInteract = null;

        public MeshRenderer PlayerBody;
        public PhotonView PlayerView;


        public ToolItem ActiveTool;
        /// <summary>
        /// Each possessed tools. If alone in combat, there will be 4. Out of combat, depends of the max players
        /// </summary>
        public List<ToolItem> ActiveTools;

        public BaseStorage PlayerSpecialSlots;
        public ItemPreset CurrentSelectedItem;
        public bool IsAutoAttacking = false;
        public int inventorySlotSelected = 0;

        private bool scrollBusy;
        private PlaceableItem lastPlaceable;
        [HideInInspector] public int theList= 0;
        public DeckPreset Deck
        {
            get 
            {
                return (this.ActiveTool == null || ActiveTool.DeckPreset == null) ?
                    null :
                    ActiveTool.DeckPreset;
            }
        }

       
        public override int Mana
        {
            get => Mathf.Min(Statistics[EntityStatistics.Mana] + NumberOfTurnsPlayed,
                Statistics[EntityStatistics.MaxMana]);
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

        public override void Init(Cell refCell, WorldGrid refGrid, int order = 0, bool isFake = false)
        {
            base.Init(refCell, refGrid, order);

            refGrid.GridEntities.Add(this);

            this.IsFake = isFake;

            if (isFake) {
                this.FireEntityInited();
                return; 
            }

            int playersNb = PhotonNetwork.PlayerList.Length;

            this.PlayerInventory = new BaseStorage();
            this.PlayerInventory.Init(
                SettingsManager.Instance.GameUIPreset.SlotsByPlayer[playersNb - 1]);

            int toolSlots = 4;
            if (playersNb == 2) toolSlots = 2;
            if (playersNb == 3) toolSlots = PhotonNetwork.IsMasterClient ? 2 : 1;
            if (playersNb == 4) toolSlots = 1;

            this.PlayerSpecialSlots = new BaseStorage();
            this.PlayerSpecialSlots.Init(toolSlots);

            PlayerInputs.player_scroll.performed += this._scroll;

            this.FireEntityInited();
        }

        public override void FireEnteredCell(Cell cell)
        {
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
                }
                else
                {
                    if(lastPlaceable!= null)
                    {
                        InputManager.Instance.OnNewCellHovered -= lastPlaceable.Previsualize;
                        InputManager.Instance.OnCellRightClickDown -= lastPlaceable.Place;
                        lastPlaceable = null;
                    }
                }
            }
            else
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
            activeTool.DeckPreset.LinkedPlayer = this;
            
            // Only set the player stats from one tool, the first one picked up
            if (this.ActiveTool == null)
            {
                this.ActiveTool = activeTool;
                this.SetStatistics(activeTool.DeckPreset.Statistics);
            }

            this.ActiveTools.Add(activeTool);
        }

        public void RemoveActiveTool(ToolItem removedTool)
        {
            removedTool.ActualPlayer = null;
            removedTool.DeckPreset.LinkedPlayer = null;
            this.ActiveTools.Remove(removedTool);

            this.ActiveTool = this.ActiveTools.Count > 0 ?
                this.ActiveTools[0] : 
                null;
            
            if(this.ActiveTool != null)
                this.SetStatistics(this.ActiveTool.DeckPreset.Statistics);
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

        #region INTERACTIONS

        public void TakeResources(ItemPreset resource, int quantity)
        {
            this.PlayerInventory.TryAddItem(resource, quantity);
        }

        #endregion
    }
}