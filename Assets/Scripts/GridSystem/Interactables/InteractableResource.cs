using DownBelow.Managers;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class InteractableResource : Interactable
    {
        public MeshRenderer GatheredMesh;
        [ReadOnly] public bool isMature = true;
        [Tooltip("In Seconds.")] public int TimeToGrowUp = 80;

        public override void Init(InteractablePreset InteractableRef, Cell RefCell)
        {
            isMature = true;
            base.Init(InteractableRef, RefCell);

            this.Mesh.gameObject.SetActive(true);
            this.GatheredMesh.gameObject.SetActive(false);
        }

        public override void Interact(Entity.PlayerBehavior player)
        {
            this.Mesh.gameObject.SetActive(false);
            this.GatheredMesh.gameObject.SetActive(true);
            ResourcePreset rPreset = this.InteractablePreset as ResourcePreset;

            int gathered = UnityEngine.Random.Range(rPreset.MinGathering, rPreset.MaxGathering);
            Debug.Log("Gathered " + gathered + " of " + rPreset.UName);
            NetworkManager.Instance.GiftOrRemovePlayerItem(player.UID, rPreset.ResourceItem, gathered);
            StartCoroutine(GrowingRoutine());
        }
        IEnumerator GrowingRoutine()
        {
            isMature = false;
            yield return new WaitForSeconds(TimeToGrowUp);
            isMature = true;
            this.Mesh.gameObject.SetActive(true);
            this.GatheredMesh.gameObject.SetActive(false);
        }
    }
}