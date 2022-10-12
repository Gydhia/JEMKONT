using Jemkont.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Managers
{
    public class InputManager : _baseManager<InputManager>
    {
        

        private void Update()
        {
            if (CombatManager.Instance.CurrentPlayingEntity == null)
                return;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // layer 7 = Cell
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 7))
            {
                if (hit.collider != null && hit.collider.TryGetComponent(out Cell cell))
                {
                    // Avoid executing this code when it has already been done
                    if (cell != GridManager.Instance.LastHoveredCell)
                    {
                        GridManager.Instance.OnNewCellHovered(CombatManager.Instance.CurrentPlayingEntity, cell);
                    }
                }
            }
            else
            {
                GridManager.Instance.LastHoveredCell = null;
            }

            // Teleport player to location
            if (Input.GetMouseButtonUp(0))
            {
                GridManager.Instance.ClickOnCell();
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