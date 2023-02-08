using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class InteractableResource : Interactable
    {
        public override void Init(InteractablePreset InteractableRef, Cell RefCell)
        {
            base.Init(InteractableRef, RefCell);

            this.Mesh.gameObject.SetActive(true);
            this.GatheredMesh.gameObject.SetActive(false);
        }

        public override void Interact(Entity.PlayerBehavior player)
        {
            this.Mesh.gameObject.SetActive(false);
            this.GatheredMesh.gameObject.SetActive(true);
            ResourcePreset rPreset = this.InteractablePreset as ResourcePreset;

            int gathered = Random.Range(rPreset.MinGathering, rPreset.MaxGathering);
            Debug.Log("Gathered " + gathered + " of " + rPreset.UName);
            NetworkManager.Instance.GiftOrRemovePlayerItem(player.PlayerID, rPreset.ResourceItem, gathered);
        }
    }
}