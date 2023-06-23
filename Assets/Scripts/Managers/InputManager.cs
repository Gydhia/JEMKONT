using DownBelow.Events;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DownBelow.Managers
{
    public class InputManager : _baseManager<InputManager>
    {
        #region INPUT_SYSTEM

        public PlayerInput PlayerInput;
        public InputActionAsset InputActionAsset;

        #endregion

        #region EVENTS
        public event CellEventData.Event OnCellRightClickDown;
        public event GameEventData.Event OnAnyRightClickDown;

        public event CellEventData.Event OnCellClickedUp;
        public event CellEventData.Event OnCellClickedDown;

        public event CellEventData.Event OnNewCellHovered;


        public void FireRightClick()
        {
            this.OnAnyRightClickDown?.Invoke(new GameEventData());
        }
        public void FireCellRightClick(Cell Cell)
        {
            this.OnCellRightClickDown?.Invoke(new CellEventData(Cell));
        }
        public void FireCellClickedUp(Cell Cell)
        {
            this.OnCellClickedUp?.Invoke(new CellEventData(Cell));
        }
        public void FireCellClickedDown(Cell Cell)
        {
            this.OnCellClickedDown?.Invoke(new CellEventData(Cell));
        }
        public void FireNewCellHovered(Cell Cell)
        {
            this.OnNewCellHovered?.Invoke(new CellEventData(Cell));
        }
        #endregion

        public Interactable LastInteractable;
        public EventSystem GameEventSystem;

        public bool IsPressingShift = false;
        public bool IsPointingOverUI = false;

        private UI.Inventory.BaseStorage _playerInventory => GameManager.RealSelfPlayer.PlayerInventory;
        private ItemPreset _currentSelectedItem;
        private int _inventorySlotSelected = 0;
        private bool _scrollBusy;
        private PlaceableItem _lastPlaceable;

        public override void Awake()
        {
            base.Awake();

            PlayerInputs.Init(this.InputActionAsset);

            this.SubscribeToInputEvents();
        }

        public void SubscribeToInputEvents()
        {
            PlayerInputs.player_l_click.performed += this._onLeftClickDown;
            PlayerInputs.player_l_click.canceled += this._onLeftClickUp;

            PlayerInputs.player_r_click.performed += this._onRightClickDown;

            PlayerInputs.player_interact.canceled += this._onInteract;

            PlayerInputs.player_escape.canceled += this._onEscape;

            PlayerInputs.player_F4.performed += this._onF4Down;

            PlayerInputs.player_scroll.performed += this._scroll;
        }

        private void Update()
        {
            if (!GameManager.GameStarted)
                return;

            this.IsPointingOverUI = EventSystem.current.IsPointerOverGameObject() ;


            if (!ReferenceEquals(Keyboard.current, null))
                this.IsPressingShift = Keyboard.current.shiftKey.IsPressed();

            #region CELLS_RAYCAST
            if (!this.IsPointingOverUI)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                // layer 7 = Cell
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 7))
                {
                    if (hit.collider != null && hit.collider.TryGetComponent(out Cell cell))
                    {
                        // Avoid executing this code when it has already been done
                        if (cell != GridManager.Instance.LastHoveredCell)
                            this.FireNewCellHovered(cell);
                    }
                }
                else
                {
                    GridManager.Instance.LastHoveredCell = null;
                }
                #endregion
                #region INTERACTABLEs_RAYCAST
                // layer 8 = Interactable
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 8))
                {
                    if (hit.collider != null && hit.collider.TryGetComponent(out Interactable interactable))
                    {
                        if (interactable != this.LastInteractable)
                        {
                            if (LastInteractable != null)
                                this.LastInteractable.OnUnfocused();
                            this.LastInteractable = interactable;
                            this.LastInteractable.OnFocused();
                        }
                    }
                }
                else if (this.LastInteractable != null)
                {
                    this.LastInteractable.OnUnfocused();
                    this.LastInteractable = null;
                }
                #endregion
            }
            else
            {
                if(this.LastInteractable != null)
                {
                    this.LastInteractable.OnUnfocused();
                    this.LastInteractable = null;
                }
                
                GridManager.Instance.LastHoveredCell = null;
            }
        }
        private void _onF4Down(InputAction.CallbackContext ctx) => this.OnF4Down();
        private void OnF4Down()
        {
            if (PlayerInputs.player_alt.IsPressed())
            {
                Debug.Log("Quitting...");
                Application.Quit();
            }
        }

        private void _onLeftClickDown(InputAction.CallbackContext ctx) => this.OnLeftClickDown();
        public void OnLeftClickDown()
        {
            if (this.IsPointingOverUI)
                return;

            if (GridManager.Instance.LastHoveredCell != null)
                this.FireCellClickedDown(GridManager.Instance.LastHoveredCell);
        }

        private void _onLeftClickUp(InputAction.CallbackContext ctx) => this.OnLeftClickUp();
        public void OnLeftClickUp()
        {
            if (this.IsPointingOverUI)
                return;

            if (GridManager.Instance.LastHoveredCell != null)
            {
                this.FireCellClickedUp(GridManager.Instance.LastHoveredCell);
            }
        }

        private void _onRightClickDown(InputAction.CallbackContext ctx) => this.OnRightClickDown();
        public void OnRightClickDown()
        {
            if (this.IsPointingOverUI)
                return;

            if (GridManager.Instance.LastHoveredCell != null)
            {
                this.FireCellRightClick(GridManager.Instance.LastHoveredCell);
            }

            this.FireRightClick();
        }

        private void _onInteract(InputAction.CallbackContext ctx) => this.OnInteract();
        public void OnInteract()
        {

        }

        private void _onEscape(InputAction.CallbackContext ctx) => this.OnEscape();
        public void OnEscape()
        {

        }


        private void _scroll(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => this.Scroll(ctx.ReadValue<float>());
        void Scroll(float value)
        {
            if (_scrollBusy) return;
            _scrollBusy = true;
            int newSlot = _inventorySlotSelected;
            if (value <= -110)
            {
                //If we're scrolling up,
                if (_inventorySlotSelected + 1 >= this._playerInventory.MaxSlots)
                {
                    //If by incrementing our selectedslot we would go over the limit; do a loop
                    newSlot = 0;
                }
                else
                {
                    //Increment simply
                    newSlot++;
                }
                switchSlots(_inventorySlotSelected, newSlot);
            }
            else if (value >= 110)
            {

                if (_inventorySlotSelected - 1 < 0)
                {
                    //if by decrementing we would go below 0;
                    newSlot = this._playerInventory.MaxSlots;
                }
                else
                {
                    //decrement
                    newSlot--;
                }

                switchSlots(_inventorySlotSelected, newSlot);
            }
            else
            {
                //NoScrollin
            }
            _scrollBusy = false;
            processEndScroll();
        }

        void switchSlots(int old, int newSlot)
        {
            UIManager.Instance.SwitchSelectedSlot(old, newSlot);

            _currentSelectedItem = this._playerInventory.StorageItems[newSlot - 1].ItemPreset;
            _inventorySlotSelected = newSlot;
        }

        void processEndScroll()
        {
            if (_lastPlaceable != null)
            {
                OnNewCellHovered -= _lastPlaceable.Previsualize;
                PlayerInputs.player_interact.performed -= _lastPlaceable.AskToPlace;
                _lastPlaceable.StopPrevisualize();
                _lastPlaceable = null;
            }

            if (_currentSelectedItem != null)
            {
                if (_currentSelectedItem is PlaceableItem placeable)
                {
                    _lastPlaceable = placeable;
                    OnNewCellHovered += _lastPlaceable.Previsualize;
                    PlayerInputs.player_interact.performed += _lastPlaceable.AskToPlace;

                    UIManager.Instance.PlayerInventory.PressEIndicator.gameObject.SetActive(true);
                }
                else
                {
                    UIManager.Instance.PlayerInventory.PressEIndicator.gameObject.SetActive(false);

                    if (_lastPlaceable != null)
                    {
                        OnNewCellHovered -= _lastPlaceable.Previsualize;
                        PlayerInputs.player_interact.performed -= _lastPlaceable.AskToPlace;
                        _lastPlaceable = null;
                    }
                }
            }
            else
            {
                UIManager.Instance.PlayerInventory.PressEIndicator.gameObject.SetActive(false);

                if (_lastPlaceable != null)
                {
                    OnNewCellHovered -= _lastPlaceable.Previsualize;
                    PlayerInputs.player_interact.performed -= _lastPlaceable.AskToPlace;
                    _lastPlaceable = null;
                }
            }
        }

        private void OnDestroy()
        {
            PlayerInputs.player_scroll.performed -= this._scroll;
        }

        public void ChangeCursorAppearance(CursorAppearance newAppearance)
        {
            switch (newAppearance)
            {
                case CursorAppearance.Idle:
                    Cursor.SetCursor(null, new Vector2(0f, 20f), CursorMode.Auto);
                    break;
                case CursorAppearance.Card:
                    Cursor.SetCursor(SettingsManager.Instance.InputPreset.CardCursor, new Vector2(0f, 20f), CursorMode.Auto);
                    break;
            }
        }
    }

    public enum CursorAppearance
    {
        Idle = 0,
        Card = 1
    }
}