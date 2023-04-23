using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

namespace DownBelow.UI
{
    public class UIPlayerInfos : MonoBehaviour
    {
        public TextMeshProUGUI ManaText;
        public TextMeshProUGUI HealthText;
        public TextMeshProUGUI MoveText;

        [SerializeField] private Image _lifeFill;

        private int _previousHealthValue, _previousManaValue, _previousMoveValue;

        public void Init()
        {
            this.gameObject.SetActive(true);

            GameManager.Instance.SelfPlayer.OnManaAdded += _onManaChanged;
            GameManager.Instance.SelfPlayer.OnManaRemoved += _onManaChanged;
            GameManager.Instance.SelfPlayer.OnHealthAdded += _onHealthChanged;
            GameManager.Instance.SelfPlayer.OnHealthRemoved += _onHealthChanged;
            GameManager.Instance.SelfPlayer.OnSpeedAdded += _onMoveChanged;
            GameManager.Instance.SelfPlayer.OnSpeedRemoved += _onMoveChanged;

            this._onManaChanged(null);
            this._onHealthChanged(null);
            this._onMoveChanged(null);
        }

        private void _onManaChanged(Events.SpellEventData data)
        {
            this.SetMana(GameManager.Instance.SelfPlayer.Mana, true);
        }

        private void _onHealthChanged(Events.SpellEventData data)
        {
            this.SetHealth(GameManager.Instance.SelfPlayer.Health, true);
        }

        private void _onMoveChanged(Events.SpellEventData data)
        {
            this.SetMove(GameManager.Instance.SelfPlayer.Speed, true);
        }

        public void SetMana(int value, bool animated = true)
        {
            if (animated)
            {
                this.ManaText.transform.DOPunchScale(Vector3.one * 1.4f, 0.6f).SetEase(Ease.OutQuint);
                this.ManaText.text = value.ToString();
            }
            else
            {
                this.ManaText.text = value.ToString();
            }
        }

        public void SetHealth(int value, bool animated = true)
        {
            if (value < 0)
                value = 0;

            if (animated)
            {
                _lifeFill.DOFillAmount((float)((float)value / (float)GameManager.Instance.SelfPlayer.MaxHealth), 0.6f)
                    .SetEase(Ease.OutQuart);
                this.HealthText.transform.DOPunchScale(Vector3.one * 1.4f, 0.6f).SetEase(Ease.OutQuint);
                this.HealthText.text = value.ToString();
            }
            else
            {
                _lifeFill.DOFillAmount((float)((float)value / (float)GameManager.Instance.SelfPlayer.MaxHealth), 0f);
                this.HealthText.text = value.ToString();
            }
        }

        public void SetMove(int value, bool animated = true)
        {
            if (animated)
            {
                this.MoveText.transform.DOPunchScale(Vector3.one * 1.4f, 0.6f).SetEase(Ease.OutQuint);
                this.MoveText.text = value.ToString();
            }
            else
            {
                this.MoveText.text = value.ToString();
            }
        }

        public void UpdateAllTexts()
        {
            this.SetMana(GameManager.Instance.SelfPlayer.Mana, false);
            this.SetHealth(GameManager.Instance.SelfPlayer.Health, false);
            this.SetMove(GameManager.Instance.SelfPlayer.Speed, false);
        }
    }
}