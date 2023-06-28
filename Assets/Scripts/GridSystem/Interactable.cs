using DownBelow.Managers;
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

        public GameObject Mesh;
        public Outlining.Outline Outline;

        [Tooltip("For Furnace and SawStood. We need to pick resources before destroying item")]
        public bool DestroyOnUse = false;
        public int CurrentDurability; 

        public virtual void Init(InteractablePreset InteractableRef, Cell RefCell) 
        {
            this.Outline.enabled = false;
            this.RefCell = RefCell;

            this.InteractablePreset = InteractableRef;
            this.Outline.OutlineColor = InteractableRef.OutlineColor;

            this.CurrentDurability = InteractableRef.Durability;
        }

        public abstract void Interact(Entity.PlayerBehavior p);

        public void ModifyDurability(int amount)
        {
            this.CurrentDurability += amount;

            if(this.CurrentDurability <= 0 && DestroyOnUse)
            {
                NetworkManager.Instance.DestroyInteractable(this);
            }
        }

        public void RemoveSelf()
        {
            this.RefCell.AttachedInteract = null;
            this.RefCell.Datas.state = CellState.Walkable;

            var particle = Instantiate(this.InteractablePreset.DestroySFX, this.RefCell.WorldPosition, Quaternion.identity, null);
            particle.gameObject.transform.Rotate(new Vector3(-90f, 0f, 0f));
            Destroy(particle.gameObject, 6f);

            Destroy(this.gameObject);
        }

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
