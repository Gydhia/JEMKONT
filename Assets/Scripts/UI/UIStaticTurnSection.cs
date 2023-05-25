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
        public GameObject SpritePrefab;
        // The parent of the entities
        public Transform EntitiesHolder;

        public Image TimeSlider;
        public Button NextTurnButton;

        public List<EntitySprite> CombatEntities;

        public void Init()
        {
            CombatManager.Instance.OnCombatStarted += SetupFromCombatBegin;
            CombatManager.Instance.OnCombatEnded += ClearFromCombatEnd;
        }

        public void SetupFromCombatBegin(GridEventData Data)
        {
            for (int i = 0; i < CombatManager.Instance.PlayingEntities.Count; i++)
            {
                Sprite weapon = null;
                if (CombatManager.Instance.PlayingEntities[i] is PlayerBehavior player)
                {
                    weapon = player.ActiveTool.InventoryIcon;
                }
                
                this.CombatEntities.Add(Instantiate(this.SpritePrefab, this.EntitiesHolder, CombatManager.Instance.PlayingEntities[i]).GetComponent<EntitySprite>());
                this.CombatEntities[i].Init(CombatManager.Instance.PlayingEntities[i].IsAlly ? AllySprite : EnemySprite, i > 0, weapon);

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
            if (CombatManager.CurrentPlayingGrid != null && CombatManager.CurrentPlayingGrid.HasStarted)
            {
                GameManager.Instance.BuffAction(
                    new EndTurnAction(CombatManager.CurrentPlayingEntity, null),
                    false
                );

                NextTurnButton.interactable = false;
            }
        }

        private void _updateTurn(EntityEventData Data)
        {
            NextTurnButton.interactable = CombatManager.CurrentPlayingEntity == GameManager.SelfPlayer;

            if (Data.Entity != GameManager.SelfPlayer)
                return;
        }
    }

}
