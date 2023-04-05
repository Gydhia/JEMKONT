using DownBelow.Managers;
using DownBelow.Mechanics;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class CardVisual : MonoBehaviour
    {
        public ScriptableCard CardReference;

        public Image IllustrationImage;
        public Image ShineImage;

        public TextMeshProUGUI CostText;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI DescText;

        public void Init(ScriptableCard CardReference)
        {
            this.CardReference = CardReference;

            this.UpdateVisual();
        }

        public void UpdateVisual()
        {
            this.ShineImage.enabled = false;
            this.CostText.text = CardReference.Cost.ToString();
            this.TitleText.text = CardReference.Title;
            this.DescText.text = CardReference.Description;
            this.IllustrationImage.sprite = CardReference.IllustrationImage;

            switch (CardReference.CardType)
            {
                case CardType.Attack:
                    ShineImage.color = SettingsManager.Instance.GameUIPreset.AttackColor;
                    break;
                case CardType.Power:
                    ShineImage.color = SettingsManager.Instance.GameUIPreset.PowerColor;
                    break;
                case CardType.Skill:
                    ShineImage.color = SettingsManager.Instance.GameUIPreset.SkillColor;
                    break;
                default:
                    break;
            }
        }

        public void Hover()
        {
            this.ShineImage.enabled = true;
        }
        public void Unhover()
        {
            this.ShineImage.enabled = false;
        }
    }
}