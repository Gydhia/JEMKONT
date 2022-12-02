using Jemkont.GridSystem;
using Jemkont.Managers;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Entity
{
    public class PlayerBehavior : CharacterEntity
    {
        public MeshRenderer PlayerBody;
        public string PlayerID;
        public PhotonView PlayerView;

        public List<Cell> CurrentPath;
        private Coroutine _moveCor = null;

        public void MoveWithPath(List<Cell> newPath)
        {
            this.CurrentPath = newPath;
            newPath.Insert(0, this.EntityCell);
            this._moveCor = StartCoroutine(FollowPath());
        }

        public IEnumerator FollowPath()
        {
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

                currentCell++;
                targetCell++;
            }

            this._moveCor = null;
        }

        
        /// <summary>
        /// Ask the server to go to a target cell. Only used by LocalPlayer
        /// </summary>
        /// <param name="cell"></param>
        public void AskToGo(Cell cell)
        {
            if (!PhotonNetwork.IsMasterClient)
                this.PlayerView.RPC("RPC_AskForPath", RpcTarget.MasterClient, GameManager.Instance.SelfPlayer, cell);
            else
                NetworkManager.Instance.ProcessAskedPath(this, cell);
        }

        [PunRPC]
        public void RPC_AskForPath(Player player, Cell cell)
        {
            NetworkManager.Instance.ProcessAskedPath(player, cell);
        }
    }
}
