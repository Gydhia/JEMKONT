using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using DownBelow.Events;

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

            GameManager.SelfPlayer.OnManaAdded += _onManaChanged;
            GameManager.SelfPlayer.OnManaRemoved += _onManaChanged;
            GameManager.SelfPlayer.OnHealthAdded += _onHealthChanged;
            GameManager.SelfPlayer.OnHealthRemoved += _onHealthChanged;
            GameManager.SelfPlayer.OnSpeedAdded += _onMoveChanged;
            GameManager.SelfPlayer.OnSpeedRemoved += _onMoveChanged;
            GameManager.SelfPlayer.OnManaMissing += _onManaMissing;

            GameManager.Instance.OnSelfPlayerSwitched += OnEntityChanged;

            this._onManaChanged(null);
            this._onHealthChanged(null);
            this._onMoveChanged(null);
        }

        public void OnEntityChanged(EntityEventData Data)
        {
            this.SetMana(Data.Entity.Mana, false);
            this.SetHealth(Data.Entity.Health, false);
            this.SetMove(Data.Entity.Speed, false);
        }

        private void _onManaChanged(Events.SpellEventData data)
        {
            this.SetMana(GameManager.SelfPlayer.Mana, true);
        }

        private void _onManaMissing()
        {
            this.ManaText.DOColor(Color.red, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
                this.ManaText.DOColor(Color.white, 0.3f).SetEase(Ease.OutQuad));
            this.ManaText.transform.DOScale(Vector3.one * 2f, 0.15f).SetEase(Ease.InOutElastic).OnComplete((() =>this.ManaText.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutElastic) ));
        }

        private void _onHealthChanged(Events.SpellEventData data)
        {
            this.SetHealth(GameManager.SelfPlayer.Health, true);
        }

        private void _onMoveChanged(Events.SpellEventData data)
        {
            this.SetMove(GameManager.SelfPlayer.Speed, true);
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
                _lifeFill.DOFillAmount((float)((float)value / (float)GameManager.SelfPlayer.MaxHealth), 0.6f)
                    .SetEase(Ease.OutQuart);
                this.HealthText.transform.DOPunchScale(Vector3.one * 1.4f, 0.6f).SetEase(Ease.OutQuint);
                this.HealthText.text = value.ToString();
            }
            else
            {
                _lifeFill.DOFillAmount((float)((float)value / (float)GameManager.SelfPlayer.MaxHealth), 0f);
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
            this.SetMana(GameManager.SelfPlayer.Mana, false);
            this.SetHealth(GameManager.SelfPlayer.Health, false);
            this.SetMove(GameManager.SelfPlayer.Speed, false);
        }
    }
}