using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Inventory;
using DownBelow.UI;
using DownBelow.UI.Inventory;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.Managers
{
    public class UIManager : _baseManager<UIManager>
    {
        public UIStaticCombat CombatSection;
        public UIStaticTurnSection TurnSection;
        public UIPlayerInfos PlayerInfos;
        public UICardSection CardSection;
        public EntityTooltipUI EntityTooltipUI;

        public UIPlayerInventory PlayerInventory;
        public UIStorage CurrentStorage;

        private Coroutine _gatheringCor;
        public Image GatheringSlider;
        public Image Outline;
        public Image CollectedItem;
        public TextMeshProUGUI GatheringName;

        public void Init()
        {
            this.TurnSection.gameObject.SetActive(false);
            this.TurnSection.Init();

            this.PlayerInfos.gameObject.SetActive(false);
            this.CardSection.gameObject.SetActive(false);
            this.EntityTooltipUI.gameObject.SetActive(false);

            GameManager.Instance.OnPlayersWelcomed += _subscribe;
        }
        public void SwitchSelectedSlot(int oldSlot, int newSlot)
        {
            if (oldSlot == 0)
            {
                //ActiveSlot
                PlayerInventory.ClassItem.SelectedSlot(false);
            } else
            {
                //Inventory
                PlayerInventory.PlayerStorage.Items[oldSlot - 1].SelectedSlot(false);
            }
            if (newSlot == 0)
            {
                PlayerInventory.ClassItem.SelectedSlot(true);
                //ActiveSlot
            } else
            {
                PlayerInventory.PlayerStorage.Items[newSlot - 1].SelectedSlot(true);
                //Inventory
            }
        }
        private void _subscribe(GameEventData Data)
        {
            CombatManager.Instance.OnCombatStarted += this.SetupCombatInterface;

            GameManager.Instance.SelfPlayer.OnGatheringStarted += StartGather;
            GameManager.Instance.SelfPlayer.OnGatheringEnded += EndGather;
            GameManager.Instance.SelfPlayer.OnGatheringCanceled += EndGather;

            CombatManager.Instance.OnCardBeginUse += this._beginCardDrag;
            CombatManager.Instance.OnCardEndUse += this._endCardDrag;

            InputManager.Instance.OnCellRightClickDown += this.UpdateEntityToolTip;


        }

        /// <summary>
        /// To process the UI when a player moved
        /// </summary>
        public void PlayerMoved()
        {
            this.CurrentStorage.HideStorage();
        }

        public void StartGather(GatheringEventData Data)
        {
            ResourcePreset resource = Data.ResourceRef.InteractablePreset as ResourcePreset;
            this.GatheringName.text = "Gathering " + resource.UName + "... (" + resource.MinGathering + ", " + resource.MaxGathering + ")";
            this.GatheringSlider.fillAmount= 0f;
            this.Outline.fillAmount = 0f;

            this.GatheringSlider.gameObject.SetActive(true);
            this.Outline.gameObject.SetActive(true);
            this.GatheringName.gameObject.SetActive(true);

            this._gatheringCor = StartCoroutine(this._gather(resource));
        }

        public void UpdateEntityToolTip(CellEventData Data)
        {
            if (Data.Cell.EntityIn == null)
                return;
            // TODO: Make sure the entity notice well the cell they're in while entering a grid

            this.EntityTooltipUI.Init(Data.Cell.EntityIn);
            this.EntityTooltipUI.gameObject.SetActive(!this.EntityTooltipUI.isActiveAndEnabled);
            //UPDATES TOOLTIP UI
        }

        public void EndGather(GatheringEventData Data)
        {
            this.GatheringSlider.gameObject.SetActive(false);
            this.GatheringName.gameObject.SetActive(false);

            if (this._gatheringCor != null)
            {
                StopCoroutine(this._gatheringCor);
                this._gatheringCor = null;
            }
        }

        private IEnumerator _gather(ResourcePreset resource)
        {
            CollectedItem.gameObject.SetActive(true);
            CollectedItem.sprite = resource.ResourceItem.InventoryIcon;
            float timer = 0f;
            while (timer <= resource.TimeToGather)
            {
                timer += Time.deltaTime;
                this.GatheringSlider.fillAmount = timer / resource.TimeToGather;
                this.Outline.fillAmount = timer / resource.TimeToGather;
                yield return null;
            }
            CollectedItem.gameObject.SetActive(false);
            Outline.gameObject.SetActive(false);
            this._gatheringCor = null;
        }

        public void SetupCombatInterface(GridEventData Data)
        {
            if (Data.Grid.IsCombatGrid)
            {
                this._setupCombatInterface();
            } else
            {
                this._setupOutOfCombatInterface();
            }
        }

        private void _setupCombatInterface()
        {
            this.TurnSection.gameObject.SetActive(true);
            this.PlayerInfos.gameObject.SetActive(true);
            this.CardSection.gameObject.SetActive(true);

            this.PlayerInventory.gameObject.SetActive(false);
        }
        private void _setupOutOfCombatInterface()
        {
            this.TurnSection.gameObject.SetActive(false);
            this.PlayerInfos.gameObject.SetActive(false);
            this.CardSection.gameObject.SetActive(false);

            this.PlayerInventory.gameObject.SetActive(true);
        }

        private void _beginCardDrag(CardEventData Data)
        {
            InputManager.Instance.ChangeCursorAppearance(CursorAppearance.Card);

        }

        private void _endCardDrag(CardEventData Data)
        {
            InputManager.Instance.ChangeCursorAppearance(CursorAppearance.Idle);
        }
    }
}

