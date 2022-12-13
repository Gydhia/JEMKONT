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

        public override void Interact()
        {
            ResourcePreset preset = this.InteractablePreset as ResourcePreset;

            this.Mesh.gameObject.SetActive(false);
            this.GatheredMesh.gameObject.SetActive(true);
        }
    }
}