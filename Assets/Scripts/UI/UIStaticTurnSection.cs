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

        public List<Image> CombatEntities;

        public void Init()
        {
            CombatManager.Instance.OnCombatStarted += SetupFromCombatBegin;
            CombatManager.Instance.OnCombatEnded += ClearFromCombatEnd;
        }

        public void SetupFromCombatBegin(GridEventData Data)
        {
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
        }

        public void ClearFromCombatEnd(GridEventData Data)
        {
            for (int i = 0; i < this.CombatEntities.Count; i++)
            {
                Destroy(this.CombatEntities[i].gameObject);
            }
            this.CombatEntities.Clear();

            CombatManager.Instance.OnTurnStarted -= this._updateTurn;
        }

        public void ChangeSelectedEntity(int index)
        {
            int last = index == 0 ? CombatEntities.Count - 1 : index - 1;

            this.CombatEntities[last].transform.GetChild(0).gameObject.SetActive(false);
            this.CombatEntities[index].transform.GetChild(0).gameObject.SetActive(true);
        }

        public void AskEndOfTurn()
        {
            Debug.LogError("ASKING TO END TURN");
            if (CombatManager.CurrentPlayingGrid != null && CombatManager.CurrentPlayingGrid.HasStarted)
            {
                NetworkManager.Instance.EntityAskToBuffAction(
                    new EndTurnAction(CombatManager.CurrentPlayingEntity, CombatManager.CurrentPlayingEntity.EntityCell)
                );

                NextTurnButton.interactable = false;
            }
        }

        private void _updateTurn(EntityEventData Data)
        {
            NextTurnButton.interactable = CombatManager.Instance.IsPlayerOrOwned(Data.Entity);
        }
    }

}
