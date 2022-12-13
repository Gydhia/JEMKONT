using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Inventory;
using DownBelow.Managers;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.Entity
{
    public class PlayerBehavior : CharacterEntity
    {
        public Inventory.Inventory PlayerInventory;
        
        private DateTime _lastTimeAsked = DateTime.Now;
        private string _nextGrid = string.Empty;
        public Interactable _nextInteract = null;
       

        public MeshRenderer PlayerBody;
        public string PlayerID;
        public PhotonView PlayerView;

        public List<Cell> NextPath { get; private set; }
        public bool CanEnterGrid => true; 
        

        public override void MoveWithPath(List<Cell> newPath, string otherGrid)
        {
            // Useless to animate hidden players
            if (!this.gameObject.activeSelf)
            {
                // /!\ TEMPORY ONLY, SET THE CELL AS THE LAST ONE OF PATH
                // We should have events instead for later on
                this.EntityCell = newPath[^1];
                return;
            }
                

            this._nextGrid = otherGrid;

            if (this.moveCor == null)
            {
                this.CurrentPath = newPath;
                // That's ugly, find a clean way to build the path instead
                if(!this.CurrentPath.Contains(this.EntityCell))
                    this.CurrentPath.Insert(0, this.EntityCell);
                this.moveCor = StartCoroutine(FollowPath());
            }
            else
            {
                this.NextPath = newPath;
                if(this._nextInteract != null)
                    this._nextInteract = null;
            }
        }

        public override IEnumerator FollowPath()
        {
            this.IsMoving = true;
            int currentCell = 0, targetCell = 1;

            float timer;
            while(currentCell < this.CurrentPath.Count - 1)
            {
                timer = 0f;
                while (timer <= 0.2f)
                {
                    this.transform.position = Vector3.Lerp(CurrentPath[currentCell].gameObject.transform.position, CurrentPath[targetCell].gameObject.transform.position, timer / 0.2f);
                    timer += Time.deltaTime;
                    yield return null;
                }

                this.EntityCell = CurrentPath[targetCell];

                if (this.NextPath != null) {
                    this.NextCell = null;
                    break;
                }

                currentCell++;
                targetCell++;

                if(targetCell <= this.CurrentPath.Count - 1)
                    this.NextCell = CurrentPath[targetCell];
            }

            this.moveCor = null;
            this.IsMoving = false;

            if (this.NextPath != null)
            {
                this.MoveWithPath(this.NextPath, _nextGrid);
                this.NextPath = null;
            }
            else if (this._nextGrid != string.Empty)
            {
                NetworkManager.Instance.PlayerAsksToEnterGrid(this, this.CurrentGrid, this._nextGrid);
                this.NextCell = null;
                this._nextGrid = string.Empty;
            }
            else if (this._nextInteract != null)
            {
                this._nextInteract.Interact();
                this._nextInteract = null;
            }
        }

        public void EnterNewGrid(CombatGrid grid)
        {
            Cell closestCell = GridUtility.GetClosestAvailableCombatCell(this.CurrentGrid, grid, this.EntityCell.PositionInGrid);

            while(closestCell.Datas.state != CellState.Walkable) {
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
        public void AskToGo(Cell cell, string otherGrid)
        {
            // To avoid too many requests we put a delay for asking
            if(this.RespectedDelayToAsk())
            {
                this._lastTimeAsked = DateTime.Now;
                NetworkManager.Instance.PlayerAsksForPath(this, cell, otherGrid);
            }    
        }

        public bool RespectedDelayToAsk()
        {
            return (System.DateTime.Now - this._lastTimeAsked).Seconds >= SettingsManager.Instance.InputPreset.PathRequestDelay; 
        }
    }
}
