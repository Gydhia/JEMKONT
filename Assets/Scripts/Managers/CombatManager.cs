using Jemkont.Entity;
using Jemkont.GridSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jemkont.Managers
{
    public class CombatManager : _baseManager<CombatManager>
    {
        #region Run-time
        private Coroutine _turnCoroutine;

        public CharacterEntity CurrentPlayingEntity;
        public CombatGrid CurrentPlayingGrid;
        public List<CharacterEntity> PlayingEntities;

        public List<CardComponent> DiscardPile;
        public List<CardComponent> DrawPile;
        public List<CardComponent> HandPile;

        public int TurnNumber;
        #endregion
 
        public void StartCombat()
        {
            if (this.CurrentPlayingGrid.HasStarted)
                return;

            // Think about enabling/initing this UI only when in combat
            UIManager.Instance.PlayerInfos.Init();

            this.TurnNumber = -1;
            this.CurrentPlayingGrid.HasStarted = true;

            this._defineEntitiesTurn();
            UIManager.Instance.TurnSection.Init(this.PlayingEntities);
            this.NextTurn();
        }

        public void NextTurn()
        {
            if (this._turnCoroutine != null) {
                StopCoroutine(this._turnCoroutine);
                this._turnCoroutine = null;
            }
            // Reset the time slider
            UIManager.Instance.TurnSection.TimeSlider.value = 0f;

            this.TurnNumber++;

            if(this.CurrentPlayingEntity != null)
                this.CurrentPlayingEntity.EndTurn();
            this.CurrentPlayingEntity = this.PlayingEntities[this.TurnNumber % this.PlayingEntities.Count];
            this.CurrentPlayingEntity.StartTurn();

            if (GameManager.Instance.SelfPlayer == this.CurrentPlayingEntity)
                this.DrawCard();

            if (this.TurnNumber > 0)
                UIManager.Instance.TurnSection.ChangeSelectedEntity(this.TurnNumber % this.PlayingEntities.Count);
            // TODO : remove this when we'll no longer need to test enemies
            //if (this.CurrentPlayingEntity.IsAlly)
            this._turnCoroutine = StartCoroutine(this._startTurnTimer());
        }

        public void PlayCard(Cell cell)
        {
            if (this.CurrentPlayingEntity == GameManager.Instance.SelfPlayer)
                CardDraggingSystem.instance.DraggedCard.CastSpell(cell);
        }

        public void DiscardCard(CardComponent card)
        {
            UIManager.Instance.CardSection.AddDiscardCard(1);
            this.HandPile.Remove(card);
            this.DiscardPile.Add(card);
        }

        public void DrawCard()
        {
            if(this.DiscardPile.Count > 0)
            {
                this.HandPile.Add(this.DiscardPile[0]);
                this.HandPile[0].DrawCardFromPile();
            }
        }

        private IEnumerator _startTurnTimer()
        {
            float time = SettingsManager.Instance.CombatPreset.TurnTime;
            float timePassed = 0f;

            UIManager.Instance.TurnSection.TimeSlider.minValue = 0f;
            UIManager.Instance.TurnSection.TimeSlider.maxValue = time;

            while (timePassed <= time) {
                yield return new WaitForSeconds(Time.deltaTime);
                timePassed += Time.deltaTime;
                UIManager.Instance.TurnSection.TimeSlider.value = timePassed;
            }

            this.NextTurn();
        }

        private void _defineEntitiesTurn()
        {
            List<CharacterEntity> enemies = this.CurrentPlayingGrid.GridEntities.Where(x=>!x.IsAlly).ToList();
            List<CharacterEntity> players = this.CurrentPlayingGrid.GridEntities.Where(x=>x.IsAlly).ToList();

            for (int i = 0; i < players.Count; i++)
                players[i].TurnOrder = i;
            for (int i = 0; i < enemies.Count; i++)
                enemies[i].TurnOrder = i;

            this.PlayingEntities = new List<CharacterEntity>();
            for (int i = 0; i < enemies.Count; i++)
            {
                this.PlayingEntities.Add(enemies[i]);
                if(i < players.Count)
                    this.PlayingEntities.Add(players[i]);
            }
        }
    }
}