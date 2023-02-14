using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Inventory;
using DownBelow.Managers;
using DownBelow.Spells;
using DownBelow.UI.Inventory;

using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace DownBelow.Entity {
    public class PlayerBehavior : CharacterEntity {
        #region EVENTS

        public event CellEventData.Event OnEnteredCell;
        public event CellEventData.Event OnExitedCell;

        public event GatheringEventData.Event OnGatheringStarted;
        public event GatheringEventData.Event OnGatheringCanceled;
        public event GatheringEventData.Event OnGatheringEnded;

        public void FireEnteredCell(Cell cell) {
            this.OnEnteredCell?.Invoke(new CellEventData(cell));
        }

        public void FireExitedCell(Cell cell) {
            this.OnExitedCell?.Invoke(new CellEventData(cell));
        }

        public void FireGatheringStarted(InteractableResource resource) {
            this.OnGatheringStarted?.Invoke(new(resource));
        }
        public void FireGatheringCanceled(InteractableResource resource = null) {
            this.OnGatheringCanceled?.Invoke(new(resource));
        }
        public void FireGatheringEnded(InteractableResource resource = null) {
            this.OnGatheringEnded?.Invoke(new(resource));
        }

        #endregion

        public BaseStorage PlayerInventory;

        private DateTime _lastTimeAsked = DateTime.Now;
        private string _nextGrid = string.Empty;
        private Coroutine _gatheringCor = null;
        private bool _stopGathering = false;

        private InteractableResource _currentResource = null;


        public Interactable NextInteract = null;

        public MeshRenderer PlayerBody;
        public string PlayerID;
        public PhotonView PlayerView;

        public List<Cell> NextPath { get; private set; }
        public bool CanEnterGrid => true;

        public Tool ActiveTool;
        public bool IsAutoAttacking = false;
        public Deck Deck { get => testingDeck; set => testingDeck = value; }
        Deck testingDeck;

        public override int Mana { get => Mathf.Min(Statistics[EntityStatistics.Mana] + NumberOfTurnsPlayed, Statistics[EntityStatistics.MaxMana]); }
        #region cards constants
        public const int MAXCARDSINHAND = 7;
        public const int CARDSTOSTARTTURNWITH = 3;
        public const int MAXMANA = 6;
        public const int MAXMANAHARDCAP = 7;
        #endregion

        public override void Init(EntityStats stats, Cell refCell, WorldGrid refGrid, int order = 0) {
            base.Init(stats, refCell, refGrid, order);

            this.PlayerInventory = new BaseStorage();
            this.PlayerInventory.Init(
                SettingsManager.Instance.GameUIPreset.SlotsByPlayer[Photon.Pun.PhotonNetwork.PlayerList.Length - 1]);
        }

        public void SetActiveTool(Tool activeTool) {
            activeTool.ActualPlayer = this;
            this.ActiveTool = activeTool;
            this.RefStats = ToolsManager.Instance.ToolStats[activeTool.Class];
        }
        public override void StartTurn() {
            base.StartTurn();
        }
        public override void EndTurn() {
            base.EndTurn();
        }

        #region MOVEMENTS
        public override void MoveWithPath(List<Cell> newPath, string otherGrid) {
            // Useless to animate hidden players
            if (!this.gameObject.activeSelf) {
                // /!\ TEMPORY ONLY, SET THE CELL AS THE LAST ONE OF PATH
                // We should have events instead for later on
                if (newPath.Count > 0)
                    this.EntityCell = newPath[^1];
                return;
            }

            if (this.CurrentGrid.IsCombatGrid) {
                if (this.IsMoving)
                    return;

                this.ApplyStat(EntityStatistics.Speed, -newPath.Count);
            }


            if (this._gatheringCor != null)
                NetworkManager.Instance.PlayerCanceledInteract(this._currentResource.RefCell);

            // TODO: Process this in network directly
            // We don't want another player to ask 2 times to enter grid
            if (Photon.Pun.PhotonNetwork.LocalPlayer.UserId == this.PlayerID)
                this._nextGrid = otherGrid;

            if (this.moveCor == null) {
                this.CurrentPath = newPath;
                // That's ugly, find a clean way to build the path instead
                if (!this.CurrentPath.Contains(this.EntityCell))
                    this.CurrentPath.Insert(0, this.EntityCell);
                this.moveCor = StartCoroutine(FollowPath());
            } else {
                this.NextPath = newPath;
                if (this.NextInteract != null)
                    this.NextInteract = null;
            }
        }

        public override IEnumerator FollowPath() {
            this.IsMoving = true;
            int currentCell = 0, targetCell = 1;

            float timer;
            while (currentCell < this.CurrentPath.Count - 1) {
                timer = 0f;
                while (timer <= 0.2f) {
                    this.transform.position = Vector3.Lerp(CurrentPath[currentCell].gameObject.transform.position, CurrentPath[targetCell].gameObject.transform.position, timer / 0.2f);
                    timer += Time.deltaTime;
                    yield return null;
                }

                // TODO : process these with events only
                this.FireExitedCell(this.EntityCell);

                this.EntityCell.Datas.state = CellState.Walkable;
                this.EntityCell.EntityIn = null;

                this.EntityCell = CurrentPath[targetCell];

                this.EntityCell.Datas.state = CellState.EntityIn;
                this.EntityCell.EntityIn = this;

                this.FireEnteredCell(this.EntityCell);

                if (this.NextPath != null) {
                    this.NextCell = null;
                    break;
                }

                currentCell++;
                targetCell++;

                if (targetCell <= this.CurrentPath.Count - 1)
                    this.NextCell = CurrentPath[targetCell];
            }

            this.moveCor = null;
            this.IsMoving = false;

            // TODO: parse these values in a different way later
            if (this.CurrentGrid.IsCombatGrid) {
                GridManager.Instance.ShowPossibleCombatMovements(this);
            }
            if (this.NextPath != null) {
                this.MoveWithPath(this.NextPath, _nextGrid);
                this.NextPath = null;
            } else if (this._nextGrid != string.Empty) {
                // TODO: This means we shouldn't ask network to change grid, it's made up when moving
                if (Photon.Pun.PhotonNetwork.LocalPlayer.UserId == GameManager.Instance.SelfPlayer.PlayerID)
                    NetworkManager.Instance.PlayerAsksToEnterGrid(this, this.CurrentGrid, this._nextGrid);
                this.NextCell = null;
                this._nextGrid = string.Empty;
            } else if (this.NextInteract != null) {
                NetworkManager.Instance.PlayerAsksToInteract(NextInteract.RefCell);
                this.NextInteract = null;
            }
        }

        public void EnterNewGrid(CombatGrid grid) {
            Cell closestCell = GridUtility.GetClosestAvailableCombatCell(this.CurrentGrid, grid, this.EntityCell.PositionInGrid);

            while (closestCell.Datas.state != CellState.Walkable) {
                List<Cell> neighbours = GridManager.Instance.GetCombatNeighbours(closestCell, grid);
                closestCell = neighbours.First(cell => cell.Datas.state == CellState.Walkable);
                if (closestCell == null)
                    closestCell = neighbours[0];
            }

            this.CurrentGrid = grid;
            this.EntityCell = closestCell;
            closestCell.Datas.state = CellState.EntityIn;

            this.transform.position = closestCell.WorldPosition;
        }

        /// <summary>
        /// Ask the server to go to a target cell. Only used by LocalPlayer
        /// </summary>
        /// <param name="cell"></param>
        public void AskToGo(Cell cell, string otherGrid) {
            // To avoid too many requests we put a delay for asking
            if (this.RespectedDelayToAsk()) {
                this._lastTimeAsked = DateTime.Now;
                NetworkManager.Instance.PlayerAsksForPath(this, cell, otherGrid);
            }
        }

        public bool RespectedDelayToAsk() {
            return (System.DateTime.Now - this._lastTimeAsked).Seconds >= SettingsManager.Instance.InputPreset.PathRequestDelay;
        }
        #endregion

        #region INTERACTIONS

        public void Interact(Cell target) {
            if (target.AttachedInteract is InteractableResource iResource) {
                if (this._gatheringCor == null) {
                    this._currentResource = iResource;
                    this._gatheringCor = StartCoroutine(_gatherResource());
                }
            } else {
                target.AttachedInteract.Interact(this);
            }
        }

        public void CancelInteract() {
            this._stopGathering = true;
        }

        public IEnumerator _gatherResource() {
            ResourcePreset preset = _currentResource.InteractablePreset as ResourcePreset;
            this.FireGatheringStarted(_currentResource);

            float timer = 0f;
            while (timer < preset.TimeToGather) {
                timer += Time.deltaTime;
                yield return null;

                if (this._stopGathering) {
                    this.FireGatheringCanceled(_currentResource);
                    this._gatheringCor = null;
                    this._stopGathering = false;
                    yield break;
                }
            }
            _currentResource.Interact(this);
            this._gatheringCor = null;

            this.FireGatheringEnded(_currentResource);
        }

        public void TakeResources(ItemPreset resource, int quantity)
        {
            this.PlayerInventory.TryAddItem(resource, quantity);
        }
        #endregion

        public override Spell AutoAttackSpell() {
            return new Spell(CombatManager.Instance.PossibleAutoAttacks.Find(x => x is DamageStrengthSpellAction));
        }
    }
}
