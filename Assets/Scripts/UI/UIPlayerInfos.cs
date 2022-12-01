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

            GameManager.Instance.SelfPlayer.OnManaAdded += _onManaChanged;
            GameManager.Instance.SelfPlayer.OnManaRemoved += _onManaChanged;
            GameManager.Instance.SelfPlayer.OnHealthAdded += _onHealthChanged;
            GameManager.Instance.SelfPlayer.OnHealthRemoved += _onHealthChanged;
            GameManager.Instance.SelfPlayer.OnMovementAdded += _onMoveChanged;
            GameManager.Instance.SelfPlayer.OnMovementRemoved += _onMoveChanged;

            this._onManaChanged(null);
            this._onHealthChanged(null);
            this._onMoveChanged(null);
        }

        private void _onManaChanged(Events.SpellEventData data) { this.SetManaText(GameManager.Instance.SelfPlayer.Mana); }
        private void _onHealthChanged(Events.SpellEventData data) { this.SetHealthText(GameManager.Instance.SelfPlayer.Health); }
        private void _onMoveChanged(Events.SpellEventData data) { this.SetMoveText(GameManager.Instance.SelfPlayer.Movement); }

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
