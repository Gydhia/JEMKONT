using Cinemachine;
using DownBelow.Entity;
using DownBelow.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DownBelow.Managers
{
    public class CameraManager : _baseManager<CameraManager>
    {
        [SerializeField]
        private CinemachineVirtualCamera VirtualCamera;
        [SerializeField]
        private Camera MainCamera;
        public void AttachPlayerToCamera(PlayerBehavior player)
        {
            this.VirtualCamera.Follow = player.gameObject.transform;

            GameManager.Instance.OnEnteredGrid += this._setupCamera;
 
      
        }

        private void _setupCamera(EntityEventData Data)
        {
            if (GameManager.Instance.SelfPlayer == Data.Entity)
            {
                if (GameManager.Instance.SelfPlayer.CurrentGrid.IsCombatGrid) 
                {
                    this.VirtualCamera.Follow = null;

                    this.VirtualCamera.transform.position = new Vector3(52f, 25f, -45f);
                    this.VirtualCamera.transform.eulerAngles = new Vector3(70f, 0f, 0f);
                }
                else
                {
                    this.VirtualCamera.Follow = Data.Entity.transform;
                }


             

    }

        }

       
    }

}
