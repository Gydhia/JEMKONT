using DownBelow.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        public Slider DurabilitySlider;
        public TextMeshProUGUI DurabilityAmount;

        public virtual void Init(InteractablePreset InteractableRef, Cell RefCell) 
        {
            this.Outline.enabled = false;
            this.RefCell = RefCell;

            this.InteractablePreset = InteractableRef;
            this.Outline.OutlineColor = InteractableRef.OutlineColor;

            if(InteractableRef.Durability == -1)
            {
                if (this.DurabilitySlider != null)
                    this.DurabilitySlider.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                this.CurrentDurability = InteractableRef.Durability;
                this.DurabilitySlider.maxValue = InteractableRef.Durability;
                this.DurabilitySlider.value = InteractableRef.Durability;
                this.DurabilitySlider.minValue = 0;
                this.DurabilityAmount.text = InteractableRef.Durability.ToString();
            }
        }

        public abstract void Interact(Entity.PlayerBehavior p);

        public void ModifyDurability(int amount)
        {
            this.CurrentDurability += amount;
            this.DurabilitySlider.value = this.CurrentDurability;
            this.DurabilityAmount.text = this.CurrentDurability.ToString() + " / " + this.InteractablePreset.Durability;

            if (this.CurrentDurability <= 0 && DestroyOnUse)
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
