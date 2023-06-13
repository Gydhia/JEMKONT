using DG.Tweening;
using DownBelow.Events;
using DownBelow.Managers;
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

        public void Init()
        {
            this.Vignette.color = new Color(Vignette.color.r, Vignette.color.g, Vignette.color.b, 0f);
            this.VictoryCanvas.alpha = 0f;
            this.VictoryCanvas.blocksRaycasts = false;
            this.DefeatCanvas.alpha = 0f;
            this.DefeatCanvas.blocksRaycasts = false;
            this.Continue.gameObject.SetActive(false);

            CombatManager.Instance.OnCombatEnded += this.ShowRewards;
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

            Vignette.DOFade(1f, 1f);

            if (Data.AlliesVictory)
            {
                this.VictoryCanvas.DOFade(1f, 2.5f);
            }
            else
            {
                this.DefeatCanvas.DOFade(1f, 2.5f);
            }
        }

        public void OnClickContinue()
        {
            GameManager.Instance.AskExitAllFromCombat();
        }
    }
}