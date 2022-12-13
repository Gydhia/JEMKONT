using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public abstract class Interactable : MonoBehaviour
    {
        [HideInInspector]
        public InteractablePreset InteractableRef;
        public Cell RefCell;

        public MeshRenderer Renderer;

        public Outline Outline;

        public virtual void Init(InteractablePreset InteractableRef, Cell RefCell)
        {
            this.Outline.enabled = false;
            this.RefCell = RefCell;

            this.InteractableRef = InteractableRef;
            this.Outline.OutlineColor = InteractableRef.OutlineColor;
        }

        public abstract void Interact();

        public void OnFocused()
        {
            this.Outline.enabled = true;
        }

        public void OnUnfocused()
        {
            this.Outline.enabled = false;
        }
    }
}
