using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.UI;
using DownBelow.UI.Inventory;
using TMPro;
using UnityEngine.UI;

namespace DownBelow.Managers
{
    public class UIManager : _baseManager<UIManager>
    {
        public UIStaticCombat CombatSection;
        public UIStaticTurnSection TurnSection;
        public UIStaticEscape EscapeSection;
        public UIStaticDatas DatasSection;
        public UIStaticGather GatherSection;
        public UIPlayerInfos PlayerInfos;
        public UICardSection CardSection;
        public UIRewardSection RewardSection;
        public UIAbyssesSection AbyssesSection;
        public UICraftingSection CraftingSection;
        public UIEnchantSection EnchantSection;
        public UIWorkshopSection WorkshopSection;
        public UIDialogSection DialogSection;

        public EntityTooltipUI EntityTooltipUI;

        public UIPlayerInventory PlayerInventory;
        public UIStorage CurrentStorage;

        public Image Outline;
        public Image CollectedItem;
        public TextMeshProUGUI GatheringName;

        public void Init()
        {
            this.TurnSection.Init();
            this.DatasSection.Init();
            this.CombatSection.Init();
            this.CardSection.Init();
            this.RewardSection.Init();
            this.AbyssesSection.Init();
            this.CraftingSection.Init();
            this.EnchantSection.Init();
            this.WorkshopSection.Init();
            this.DialogSection.Init();

            this.TurnSection.gameObject.SetActive(false);
            this.PlayerInfos.gameObject.SetActive(false);
            this.CardSection.gameObject.SetActive(false);
            this.EntityTooltipUI.gameObject.SetActive(false);

            this._subscribe();
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
        private void _subscribe()
        {
            CombatManager.Instance.OnCombatStarted += this.SetupCombatInterface;

            CombatManager.Instance.OnCardBeginUse += this._beginCardDrag;
            CombatManager.Instance.OnCardEndUse += this._endCardDrag;

            InputManager.Instance.OnCellRightClickDown += this.UpdateEntityToolTip;
            PlayerInputs.player_escape.canceled += this._switchEscapeState;
          //  PlayerInputs.player_escape.canceled += this._hideInteractables;
            
        }
        private void _unsubscribe()
        {
            CombatManager.Instance.OnCombatStarted -= this.SetupCombatInterface;

            CombatManager.Instance.OnCardBeginUse -= this._beginCardDrag;
            CombatManager.Instance.OnCardEndUse -= this._endCardDrag;

            InputManager.Instance.OnCellRightClickDown -= this.UpdateEntityToolTip;
            PlayerInputs.player_escape.canceled -= this._switchEscapeState;
           // PlayerInputs.player_escape.canceled += this._hideInteractables;
        }

        private void _switchEscapeState(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => this.SwitchEscapeState();
        public void SwitchEscapeState()
        {
            if (!this.CurrentStorage.gameObject.activeInHierarchy &&
                !this.EnchantSection.gameObject.activeInHierarchy &&
                !this.AbyssesSection.gameObject.activeInHierarchy &&
                !this.WorkshopSection.gameObject.activeInHierarchy &&
                !this.CraftingSection.gameObject.activeInHierarchy)
            {
                bool isActive = EscapeSection.gameObject.activeSelf;

                EscapeSection.gameObject.SetActive(!isActive);
            }
            
        }

        public void UpdateEntityToolTip(CellEventData Data)
        {
            return;
            // TODO : The tooltip is taking too much places, rework it or find another utility

            if (Data.Cell.EntityIn == null)
                return;
            // TODO: Make sure the entity notice well the cell they're in while entering a grid

            this.EntityTooltipUI.Init(Data.Cell.EntityIn);
            this.EntityTooltipUI.gameObject.SetActive(!this.EntityTooltipUI.isActiveAndEnabled);
            //UPDATES TOOLTIP UI
        }

        public void SetupCombatInterface(GridEventData Data)
        {
            if (GameManager.RealSelfPlayer.CurrentGrid != Data.Grid)
                return;

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

        private void _hideInteractables(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => this.HideInteractables();
        public void HideInteractables()
        {
            this.CurrentStorage.HideStorage();
            this.EnchantSection.ClosePanel();
            this.AbyssesSection.OnClickClose();
            this.WorkshopSection.ClosePanel();
            this.CraftingSection._closePanel(new EntityEventData(new EnemyEntity()));
        }

        private void OnDestroy()
        {
            this._unsubscribe();
        }
    }
}

