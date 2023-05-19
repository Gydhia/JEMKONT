using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Spells;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.Pools
{
    public class CellIndicatorPool : ObjectPool<CellIndicator>
    {
        private Dictionary<EntityAction, List<CellIndicator>> _actionsRef;
        private List<CellIndicator> _pathRef;
        private Dictionary<Cell, CellIndicator> _spellsRef;
        private Spell _currentSpell;
        private int _lastSpellAngle = 0;

        public readonly static Color GreenColor = new Color(0.1709f, 0.6320f, 0.1709f, .7f);
        public readonly static Color RedColor = new Color(0.8392f, 0.3686f, 0.4015f, .7f);
        public readonly static Color BlueColor = new Color(0.1725f, 0.4182f, 0.6313f, .7f);

        public readonly static Color GreenColorTransparent = new Color(0.1709f, 0.6320f, 0.1709f, .15f);
        public readonly static Color BlueColorTransparent = new Color(0.1725f, 0.4182f, 0.6313f, .15f);

        public void Init()
        {
            this._actionsRef = new Dictionary<EntityAction, List<CellIndicator>>();
            this._pathRef = new List<CellIndicator>();
            this._spellsRef = new Dictionary<Cell, CellIndicator>();

            CombatManager.Instance.OnCombatStarted += this._hidePlacementCells;
            CombatManager.Instance.OnSpellBeginTargetting += this.beginShowSpellTargetting;
            CombatManager.Instance.OnSpellEndTargetting += this.endShowSpellTargetting;
        }

        private void _hidePlacementCells(GridEventData Data)
        {
            this.DisplayPathIndicators(null);
        }

        #region ACTIONS
        public void DisplayActionIndicators(EntityAction action, Color? color = null)
        {
            color ??= this._getColorFromAction(action);

            CellIndicator indicator = GetPooled();

            indicator.gameObject.SetActive(true);
            indicator.transform.position = action.TargetCell.transform.position;
            indicator.CellRenderer.material.color = color.Value;

            this._actionsRef.Add(action, new List<CellIndicator> { indicator });
        }

        public void HideActionIndicators(EntityAction action)
        {
            // It may have already be removed
            if (!this._actionsRef.ContainsKey(action))
                return;

            foreach (var actionCells in this._actionsRef[action])
                actionCells.TryReleaseToPool();

            // Maybe useless, but to indicate to the GC that we doesn't need the list anymore
            this._actionsRef[action] = null;
            this._actionsRef.Remove(action);             

            // Next action's indicator should have the right alpha
            if(this._actionsRef.Count >= 1)
            {
                var nextAction = this._actionsRef.ElementAt(0);
                Color newColor = this._getColorFromAction(nextAction.Key);
                foreach (var actionCell in nextAction.Value)
                {
                    actionCell.Color = newColor;
                }
            }
        }

        private Color _getColorFromAction(EntityAction action)
        {
            if (action is MovementAction)
                return action.RefBuffer[0] == action ? GreenColor : GreenColorTransparent;
            else
                return action.RefBuffer[0] == action ? BlueColor : BlueColorTransparent;
        }
        #endregion

        #region BASICS
        // Simple unoptimized func to show on the fly any path

        public void DisplayPathIndicators(List<Cell> cells)
        {
            this.HidePathIndicators();
            if(cells == null)
            {
                return;
            }
            foreach (Cell cell in cells)
            {
                CellIndicator indicator = GetPooled();

                indicator.gameObject.SetActive(true);
                indicator.transform.position = cell.transform.position;
                indicator.Color = GreenColor;

                this._pathRef.Add(indicator);
            }
        }

        public void HidePathIndicators()
        {
            foreach (CellIndicator cellIndicator in this._pathRef)
                cellIndicator.TryReleaseToPool();

            this._pathRef.Clear();
        }

        #endregion

        #region SPELLS
        private void _displaySpellIndicators(ref bool[,] matrix, Cell origin, bool isShape)
        {
            var cells = GridUtility.TransposeShapeToCells(ref matrix, origin, isShape ? _currentSpell.Data.RotatedShapePosition : _currentSpell.Data.CasterPosition);

            foreach (var cell in cells)
            {
                if (!this._spellsRef.ContainsKey(cell))
                {
                    this._spellsRef.Add(cell, this.GetPooled());
                    this._spellsRef[cell].gameObject.SetActive(true);
                    this._spellsRef[cell].transform.position = cell.transform.position;

                    if (isShape)
                    {
                        this._spellsRef[cell].Color = RedColor;
                    }
                    else
                    {
                        this._spellsRef[cell].Color = CombatManager.IsCellCastable(cell, this._currentSpell) ? GreenColor : GreenColorTransparent;
                    }
                }
                else
                {
                    CellIndicator indicator = this._spellsRef[cell];
                    // We're gonna override this color
                    if (indicator.Color == GreenColor && isShape)
                    {
                        indicator.IsOveridden = true;
                        indicator.Color = RedColor;
                    }
                }
            }
        }

        private void _hideShapeIndicators()
        {
            for (int i = 0; i < this._spellsRef.Count; i++)
            {
                var indicator = this._spellsRef.ElementAt(i).Value;
                if (indicator.Color == RedColor)
                {
                    if (indicator.IsOveridden)
                    {
                        indicator.Color = GreenColor;
                        indicator.IsOveridden = false;
                    }
                    else
                    {
                        indicator.TryReleaseToPool();
                        this._spellsRef.Remove(_spellsRef.ElementAt(i).Key);
                        i--;
                    }
                }
            }                
        }

        protected void beginShowSpellTargetting(SpellTargetEventData Data)
        {
            this._currentSpell = Data.TargetSpell;
            this._displaySpellIndicators(ref this._currentSpell.Data.CastingMatrix, CombatManager.CurrentPlayingEntity.EntityCell, false);

            if (CombatManager.IsCellCastable(Data.Cell, this._currentSpell))
                this._displaySpellIndicators(ref this._currentSpell.Data.SpellShapeMatrix, Data.Cell, true);

            InputManager.Instance.OnNewCellHovered += this.updateSpellTargetting;
        }

        protected void updateSpellTargetting(CellEventData Data)
        {
            this._hideShapeIndicators();

            if(CombatManager.IsCellCastable(Data.Cell, this._currentSpell))
            {
                // Should we do this here ? Since it's handled by events, we can't control the call order
                if (this._currentSpell.Data.RotateShapeWithCast)
                {
                    int newAngle = GridUtility.GetRotationForMatrix(CombatManager.CurrentPlayingEntity.EntityCell, Data.Cell);
                    // Means that we should rotate the matrix
                    if(this._lastSpellAngle != newAngle)
                    {
                        this._currentSpell.Data.RotatedShapeMatrix =
                            GridUtility.RotateSpellMatrix(
                                this._currentSpell.Data.SpellShapeMatrix,
                                newAngle
                                );

                        this._currentSpell.Data.RotatedShapePosition =
                            GridUtility.RotateSpellMatrixOrigin(
                                this._currentSpell.Data.SpellShapeMatrix,
                                newAngle,
                                this._currentSpell.Data.ShapePosition
                                );

                        this._lastSpellAngle = newAngle;
                    }
                }
                
                this._displaySpellIndicators(ref this._currentSpell.Data.RotatedShapeMatrix, Data.Cell, true);
            }
        }

        protected void endShowSpellTargetting(SpellTargetEventData Data)
        {
            foreach (var indicator in this._spellsRef.Values)
                indicator.TryReleaseToPool();

            this._spellsRef.Clear();
            this._currentSpell = null;
            this._lastSpellAngle = 0;

            InputManager.Instance.OnNewCellHovered -= this.updateSpellTargetting;
        }
        #endregion
    }

}