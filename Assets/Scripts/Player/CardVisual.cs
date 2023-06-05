using System;
using DownBelow.Managers;
using DownBelow.Mechanics;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

            _cardReference = CardReference;

            ShineImage.color = CardReference.CardType switch
            {
                CardType.Attack => SettingsManager.Instance.GameUIPreset.AttackColor,
                CardType.Power => SettingsManager.Instance.GameUIPreset.PowerColor,
                CardType.Skill => SettingsManager.Instance.GameUIPreset.SkillColor,
                CardType.None => SettingsManager.Instance.GameUIPreset.SkillColor,
                _ => SettingsManager.Instance.GameUIPreset.SkillColor,
            };
        }

        private void Update()
        {
            if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, Mouse.current.position.ReadValue()))
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    LeftClick();
                }
            }
        }

        public void LeftClick()
        {
            DeckbuildingSystem.Instance.TryAddCopy(_cardReference, true);
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