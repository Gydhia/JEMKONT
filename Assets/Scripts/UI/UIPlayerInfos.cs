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

        private void _onManaChanged(Events.SpellEventData data) { this.SetManaText(GameManager.Instance.SelfPlayer.Mana); }
        private void _onHealthChanged(Events.SpellEventData data) { this.SetHealthText(GameManager.Instance.SelfPlayer.Health); }
        private void _onMoveChanged(Events.SpellEventData data) { this.SetMoveText(GameManager.Instance.SelfPlayer.Speed); }

        public void SetManaText(int value)
        {
            this.ManaText.text = value.ToString();
        }
        public void SetHealthText(int value, bool animated = true)
        {
            this.HealthText.text = value.ToString();

            if (value < 0)
                value = 0;

            if (animated)
            {
                _lifeFill.DOFillAmount((float)((float)value / (float)GameManager.Instance.SelfPlayer.MaxHealth), 0.6f).SetEase(Ease.OutQuart);
            }
            else
            {
                _lifeFill.DOFillAmount((float)((float)value / (float)GameManager.Instance.SelfPlayer.MaxHealth), 0f);
            }
            
        }
        public void SetMoveText(int value)
        {
            this.MoveText.text = value.ToString();
        }
    }

}
