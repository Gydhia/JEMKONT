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

        private ScriptableCard dataRef;

        public void Init(ScriptableCard CardReference)
        {
            this.ShineImage.enabled = false;
            this.CostText.text = CardReference.Cost.ToString();
            this.TitleText.text = CardReference.Title;
            this.DescText.text = CardReference.Description;
            this.IllustrationImage.sprite = CardReference.IllustrationImage;

            dataRef = CardReference;

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
            DeckbuildingSystem.Instance.TryAddCopy(dataRef,true);
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