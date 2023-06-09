using UnityEngine;

namespace DownBelow.GridSystem
{

    public abstract class Interactable<T> : Interactable where T : InteractablePreset
    {
        public T LocalPreset => this.InteractablePreset as T;
    }
    public abstract class Interactable : MonoBehaviour
    {

        [HideInInspector]
        public InteractablePreset InteractablePreset;
        public Cell RefCell;

        public MeshRenderer Mesh;
        public DownBelow.Outlining.Outline Outline;

        public virtual void Init(InteractablePreset InteractableRef, Cell RefCell) 
        {
            this.Outline.enabled = false;
            this.RefCell = RefCell;

            this.InteractablePreset = InteractableRef;
            this.Outline.OutlineColor = InteractableRef.OutlineColor;
        }

        public abstract void Interact(Entity.PlayerBehavior p);

        public virtual void OnFocused()
        {
            this.Outline.enabled = true;
        }

        public virtual void OnUnfocused()
        {
            this.Outline.enabled = false;
        }
    }
}
