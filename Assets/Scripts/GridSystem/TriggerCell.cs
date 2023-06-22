using DownBelow.Events;
using DownBelow.Managers;
using DownBelow.Mechanics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class TriggerCell : MonoBehaviour
    {
        public DialogPreset Preset;
        public Cell RefCell;

        public void Init(DialogPreset Preset, Cell RefCell)
        {
            this.Preset = Preset;
            this.RefCell = RefCell;

            GameManager.Instance.OnGameStarted += _subscribe;   
        }

        private void _subscribe(GameEventData Data)
        {
            GameManager.Instance.OnGameStarted -= _subscribe;

            foreach (var player in GameManager.Instance.Players.Values)
            {
                player.OnEnteredCell += OnTrigger;
            }
        }

        // A bit heavy but just a &reference comparison
        private void OnTrigger(CellEventData Data)
        {
            if(Data.Cell == this.RefCell)
            {
                this._unsubscribe();
                UIManager.Instance.DialogSection.ShowDialog(this.Preset);
            }
        }

        private void OnDestroy()
        {
            this._unsubscribe();
        }

        private void _unsubscribe()
        {
            if(GameManager.Instance != null && GameManager.Instance.Players != null)
            {
                foreach (var player in GameManager.Instance.Players.Values)
                {
                    player.OnEnteredCell -= OnTrigger;
                }
            }
        }
    }
}