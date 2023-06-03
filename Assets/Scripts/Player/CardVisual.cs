using System;
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
        public Image IllustrationImage;
        public Image ShineImage;

        public TextMeshProUGUI CostText;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI DescText;

        private ScriptableCard _cardReference;
        private void OnEnable()
        {
            this.CostText.color = GameManager.SelfPlayer.Mana < this._cardReference.Cost ? Color.red : Color.white;
        }

        public void Init(ScriptableCard CardReference)
        {
            this._cardReference = CardReference;
            this.ShineImage.enabled = false;
            this.CostText.text = this._cardReference.Cost.ToString();
            this.TitleText.text = this._cardReference.Title;
            this.DescText.text = this._cardReference.Description;
            this.IllustrationImage.sprite = this._cardReference.IllustrationImage;

            switch (this._cardReference.CardType)
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