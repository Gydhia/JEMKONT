using DownBelow.Events;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Managers
{
    public class InputManager : _baseManager<InputManager>
    {
        #region EVENTS
        public event CellEventData.Event OnCellRightClick;

        public event CellEventData.Event OnCellClickedUp;
        public event CellEventData.Event OnCellClickedDown;

        public event CellEventData.Event OnNewCellHovered;

        public void FireCellRightClick(Cell Cell)
        {
            this.OnCellRightClick?.Invoke(new CellEventData(Cell));
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

        private void Update()
        {
            if (!GameManager.GameStarted)
                return;

            #region CELLS_RAYCAST
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 8))
            {
                if (hit.collider != null && hit.collider.TryGetComponent(out Interactable interactable))
                {
                    if(interactable != this.LastInteractable)
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
            if (Input.GetMouseButtonUp(1))
            {
                if (GridManager.Instance.LastHoveredCell != null)
                    this.FireCellRightClick(GridManager.Instance.LastHoveredCell);
            }
            if (Input.GetMouseButtonUp(0))
            {
                if(GridManager.Instance.LastHoveredCell != null)
                    this.FireCellClickedUp(GridManager.Instance.LastHoveredCell);
            }
            if (Input.GetMouseButtonDown(0)) {
                if (GridManager.Instance.LastHoveredCell != null)
                    this.FireCellClickedDown(GridManager.Instance.LastHoveredCell);
            }

            // UTILITY : To mark a cell as non-walkable
            if (Input.GetMouseButtonUp(1))
            {
                // layer 7 = Cell
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 7))
                {
                    if (hit.collider.TryGetComponent(out Cell cell))
                    {
                        cell.ChangeCellState(cell.Datas.state == CellState.Blocked ? CellState.Walkable : CellState.Blocked);
                    }
                }
            }
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