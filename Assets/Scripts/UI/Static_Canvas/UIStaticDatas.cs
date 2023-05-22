using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DownBelow.UI
{
    public class UIStaticDatas : MonoBehaviour
    {
        public TextMeshProUGUI WarningText;

        public void Init()
        {
            this.WarningText.gameObject.SetActive(false);
        }

        public void ShowWarningText()
        {
            // Currently only to inform that we cannot enter grid without item equiped
            this.WarningText.gameObject.SetActive(true);
            this.WarningText.DOFade(0f, 1f).SetDelay(3f).OnComplete(() =>
            {
                this.WarningText.alpha = 1f;
                this.WarningText.gameObject.SetActive(false);
            }
            );
        }
    }
}
