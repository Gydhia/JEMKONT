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
                this.FireCellRightClick(GridManager.Instance.LastHoveredCell);
            else
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