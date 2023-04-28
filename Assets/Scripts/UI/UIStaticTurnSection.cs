using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UIStaticTurnSection : MonoBehaviour
    {
        public Sprite AllySprite;
        public Sprite EnemySprite;
        // The item to instantiate
        public Image SpritePrefab;
        // The parent of the entities
        public Transform EntitiesHolder;

        public Image TimeSlider;
        public Button NextTurnButton;
        public Button StartCombatButton;

        public List<Image> CombatEntities;

        public void Init()
        {
            this.StartCombatButton.gameObject.SetActive(false);

            CombatManager.Instance.OnCombatStarted += SetupFromCombatBegin;
            CombatManager.Instance.OnCombatEnded += ClearFromCombatEnd;
            GameManager.Instance.OnEnteredGrid += _showHideStartButton;
        }

        private void _showHideStartButton(EntityEventData Data)
        {
            if (Data.Entity == GameManager.Instance.SelfPlayer && Data.Entity.CurrentGrid.IsCombatGrid)
            {
                this.StartCombatButton.gameObject.SetActive(true);
            }
        }

        public void SetupFromCombatBegin(GridEventData Data)
        {
            this.StartCombatButton.gameObject.SetActive(false);

            for (int i = 0; i < CombatManager.Instance.PlayingEntities.Count; i++)
            {
                this.CombatEntities.Add(Instantiate(this.SpritePrefab, this.EntitiesHolder, CombatManager.Instance.PlayingEntities[i]));
                this.CombatEntities[i].sprite = CombatManager.Instance.PlayingEntities[i].IsAlly ? AllySprite : EnemySprite;
                if (i > 0)
                    this.CombatEntities[i].transform.GetChild(0).gameObject.SetActive(false);
            }

            this.NextTurnButton.onClick.RemoveAllListeners();
            this.NextTurnButton.onClick.AddListener(AskEndOfTurn);

            CombatManager.Instance.OnTurnStarted += this._updateTurn;
            CombatManager.Instance.OnTurnEnded += this._updateTurn;
        }

        public void ClearFromCombatEnd(GridEventData Data)
        {
            for (int i = 0; i < this.CombatEntities.Count; i++)
            {
                Destroy(this.CombatEntities[i].gameObject);
            }
            this.CombatEntities.Clear();

            CombatManager.Instance.OnTurnStarted -= this._updateTurn;
            CombatManager.Instance.OnTurnEnded -= this._updateTurn;
        }

        public void ChangeSelectedEntity(int index)
        {
            int last = index == 0 ? CombatEntities.Count - 1 : index - 1;

            this.CombatEntities[last].transform.GetChild(0).gameObject.SetActive(false);
            this.CombatEntities[index].transform.GetChild(0).gameObject.SetActive(true);
        }

        public void AskEndOfTurn()
        {
            if (CombatManager.Instance.CurrentPlayingGrid != null && CombatManager.Instance.CurrentPlayingGrid.HasStarted)
            {
                GameManager.Instance.BuffAction(
                    new EndTurnAction(CombatManager.Instance.CurrentPlayingEntity, null),
                    false
                );

                NextTurnButton.interactable = false;
            }
        }

        private void _updateTurn(EntityEventData Data)
        {
            NextTurnButton.interactable = CombatManager.Instance.CurrentPlayingEntity == GameManager.Instance.SelfPlayer;

            if (Data.Entity != GameManager.Instance.SelfPlayer)
                return;
        }
    }

}
