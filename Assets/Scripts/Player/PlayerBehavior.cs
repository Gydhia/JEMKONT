using Jemkont.GridSystem;
using Jemkont.Managers;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Entity
{
    public class PlayerBehavior : CharacterEntity
    {
        private DateTime _lastTimeAsked = DateTime.Now;

        public MeshRenderer PlayerBody;
        public string PlayerID;
        public PhotonView PlayerView;

        public List<Cell> CurrentPath;
        public List<Cell> NextPath { get; private set; }
        private Coroutine _moveCor = null;

        public void MoveWithPath(List<Cell> newPath)
        {
            if (this._moveCor == null)
            {
                this.CurrentPath = newPath;
                // That's ugly, find a clean way to build the path instead
                if(!this.CurrentPath.Contains(this.EntityCell))
                    this.CurrentPath.Insert(0, this.EntityCell);
                this._moveCor = StartCoroutine(FollowPath());
            }
            else
            {
                this.NextPath = newPath;
            }
        }

        public IEnumerator FollowPath()
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

            this._moveCor = null;
            this.IsMoving = false;

            if(this.NextPath != null)
            {
                this.MoveWithPath(this.NextPath);
                this.NextPath = null;
            }
        }

        
        /// <summary>
        /// Ask the server to go to a target cell. Only used by LocalPlayer
        /// </summary>
        /// <param name="cell"></param>
        public void AskToGo(Cell cell)
        {
            // To avoid too many requests we put a delay for asking
            if(this.RespectedDelayToAsk())
            {
                this._lastTimeAsked = DateTime.Now;
                NetworkManager.Instance.PlayerAsksForPath(this, cell);
            }    
        }

        public bool RespectedDelayToAsk()
        {
            return (System.DateTime.Now - this._lastTimeAsked).Seconds >= SettingsManager.Instance.InputPreset.PathRequestDelay; 
        }
    }
}
