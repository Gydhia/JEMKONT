using DG.Tweening;
using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UIRewardSection : MonoBehaviour
    {
        public TextMeshProUGUI LevelNumber;
        public GameObject Aura;

        public Image Vignette;
        public CanvasGroup VictoryCanvas;
        public CanvasGroup DefeatCanvas;
        public Button Continue;

        [Header("Rewards")] 
        public GameObject ResourcesSliderParent;
        public Slider RefilledResourcesSlider;
        public GameObject CardsRewardParent;
        public Button CardReward;
        public TextMeshProUGUI QuantityText;
        
        private bool _alliesVictory;

        public void Init()
        {
            this.Vignette.color = new Color(Vignette.color.r, Vignette.color.g, Vignette.color.b, 0f);
            this.VictoryCanvas.alpha = 0f;
            this.VictoryCanvas.blocksRaycasts = false;
            this.DefeatCanvas.alpha = 0f;
            this.DefeatCanvas.blocksRaycasts = false;
            this.Continue.gameObject.SetActive(false);

            CombatManager.Instance.OnCombatEnded += this.ShowRewards;
            CombatManager.Instance.OnCombatStarted += _disableContinue;
            
            CardReward.onClick.AddListener(OnRewardClicked);
        }

        private void _disableContinue(GridEventData Data)
        {
            this.Continue.interactable = false;
        }

        public void EnableContinue()
        {
            this.Continue.interactable = true;
        }


        private void Update()
        {
            Aura.transform.Rotate(Vector3.forward * Time.deltaTime * 6f);
        }

        public void ShowRewards(GridEventData Data)
        {
            this.DefeatCanvas.blocksRaycasts = true;
            this.VictoryCanvas.blocksRaycasts = true;
            this.Continue.gameObject.SetActive(true);

            Vignette.DOFade(0.7f, 1f);

            this._alliesVictory = Data.AlliesVictory;
            if (this._alliesVictory)
            {
                this.VictoryCanvas.DOFade(1f, 2.5f);
                
                string abyssName = (Data.Grid as CombatGrid).ParentGrid.UName;
                var abyss = SettingsManager.Instance.AbyssesPresets.Find(x=> x.name == abyssName);
                
                ResourcesSliderParent.SetActive(false);
                CardsRewardParent.SetActive(true);
                QuantityText.text = "x" + abyss.GiftedCards.Count.ToString();
            }
            else
            {
                this.DefeatCanvas.DOFade(1f, 2.5f);
            }
        }

        public void OnClickContinue()
        {
            GameManager.RealSelfPlayer.gameObject.SetActive(true);

            Vignette.DOFade(0f, 0.5f);
            if (this._alliesVictory)
            {
                this.VictoryCanvas.DOFade(0f, 0.5f).OnComplete(() => { this.gameObject.SetActive(false); });
            }
            else
            {
                this.DefeatCanvas.DOFade(0f, 0.5f).OnComplete(() => { this.gameObject.SetActive(false); });
            }

            // We obviously are now in a combat grid
            var grid = ((CombatGrid)GameManager.RealSelfPlayer.CurrentGrid).ParentGrid;
            var exitAction = new EnterGridAction(GameManager.RealSelfPlayer, grid.Cells[0, 0]);
            exitAction.Init(grid.UName);

            NetworkManager.Instance.EntityAskToBuffAction(exitAction);
        }

        private void OnRewardClicked()
        {
            CardReward.transform.DOPunchScale(new Vector3(0.01f, 0.01f, 0.01f), 0.6f).SetEase((Ease.OutQuad))
                .OnComplete((
                    () =>
                    {
                        CardsRewardParent.gameObject.SetActive(false);
                        RefilledResourcesSlider.DOValue(0, 0f);
                        ResourcesSliderParent.gameObject.SetActive(true);
                        RefilledResourcesSlider.DOValue(1, 0.7f).SetEase(Ease.OutQuint).OnComplete((() =>
                        {
                            ResourcesSliderParent.gameObject.SetActive(false);
                        }));
                    }));
        }
    }
}