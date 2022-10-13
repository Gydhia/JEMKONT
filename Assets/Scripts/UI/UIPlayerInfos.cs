using Jemkont.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Jemkont.UI
{
    public class UIPlayerInfos : MonoBehaviour
    {
        public TextMeshProUGUI ManaText;
        public TextMeshProUGUI HealthText;
        public TextMeshProUGUI MoveText;

        public void Init()
        {
            this.gameObject.SetActive(true);

            PlayerManager.Instance.SelfPlayer.OnManaAdded += _onManaChanged;
            PlayerManager.Instance.SelfPlayer.OnManaRemoved += _onManaChanged;
            PlayerManager.Instance.SelfPlayer.OnHealthAdded += _onHealthChanged;
            PlayerManager.Instance.SelfPlayer.OnHealthRemoved += _onHealthChanged;
            PlayerManager.Instance.SelfPlayer.OnMovementAdded += _onMoveChanged;
            PlayerManager.Instance.SelfPlayer.OnMovementRemoved += _onMoveChanged;

            this._onManaChanged();
            this._onHealthChanged();
            this._onMoveChanged();
        }

        private void _onManaChanged() { this.SetManaText(PlayerManager.Instance.SelfPlayer.Mana); }
        private void _onHealthChanged() { this.SetHealthText(PlayerManager.Instance.SelfPlayer.Health); }
        private void _onMoveChanged() { this.SetMoveText(PlayerManager.Instance.SelfPlayer.Movement); }

        public void SetManaText(int value)
        {
            this.ManaText.text = value.ToString();
        }
        public void SetHealthText(int value)
        {
            this.HealthText.text = value.ToString();
        }
        public void SetMoveText(int value)
        {
            this.MoveText.text = value.ToString();
        }
    }

}
