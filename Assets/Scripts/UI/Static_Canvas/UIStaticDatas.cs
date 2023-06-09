using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DownBelow.UI
{
    public class UIStaticDatas : MonoBehaviour
    {
        public TextMeshProUGUI WarningText;
        private TweenerCore<Color, Color, ColorOptions> WarningTween;

        public void Init()
        {
            this.WarningText.gameObject.SetActive(false);
        }

        public void ShowWarningText(string message)
        {
            if (WarningTween != null)
            {
                WarningTween.Complete();
            }

            // Currently only to inform that we cannot enter grid without item equiped
            this.WarningText.gameObject.SetActive(true);
            this.WarningText.text = message;

            WarningTween = this.WarningText.DOFade(0f, 1f).SetDelay(3f).OnComplete(() =>
            {
                this.WarningText.alpha = 1f;
                this.WarningText.gameObject.SetActive(false);
                this.WarningTween = null;
            }
            );
        }
    }
}
