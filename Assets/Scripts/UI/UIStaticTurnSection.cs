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
        // The item to instantiate
        public GameObject SpritePrefab;
        // The parent of the entities
        public Transform EntitiesHolder;

        public Image TimeSlider;
        public Button[] NextTurnButtons;

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
                this.CombatEntities.Add(Instantiate(this.SpritePrefab, this.EntitiesHolder, CombatManager.Instance.PlayingEntities[i]).GetComponent<EntitySprite>());
                this.CombatEntities[i].Init(CombatManager.Instance.PlayingEntities[i], i <= 0);
            }

            foreach (Button button in this.NextTurnButtons)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(AskEndOfTurn);
            }
            /*his.NextTurnButton
            this.NextTurnButton.*/

            CombatManager.Instance.OnTurnStarted += this._updateTurn;
            CombatManager.Instance.OnEntityDeath += this._updateEntityDeath;
        }

        public void ClearFromCombatEnd(GridEventData Data)
        {
            // Dead entities are removed from list, so clear it this way
            foreach (Transform entity in this.EntitiesHolder)
            {
                Destroy(entity.gameObject);
            }
            this.CombatEntities.Clear();

            CombatManager.Instance.OnTurnStarted -= this._updateTurn;
            CombatManager.Instance.OnEntityDeath -= this._updateEntityDeath;
        }

        public void ChangeSelectedEntity(int index)
        {
            if(CombatEntities.Count <= 0) { return; }

            int last = index == 0 ? CombatEntities.Count - 1 : index - 1;

            if (last < this.CombatEntities.Count)
                this.CombatEntities[last].SetSelected(false);
            if (index < this.CombatEntities.Count)
                this.CombatEntities[index].SetSelected(true);
        }

        public void AskEndOfTurn()
        {
            if (CombatManager.CurrentPlayingGrid != null && CombatManager.CurrentPlayingGrid.HasStarted)
            {
                foreach (Button button in this.NextTurnButtons)
                {
                    button.interactable = false;
                }

                NetworkManager.Instance.EntityAskToBuffAction(
                    new EndTurnAction(CombatManager.CurrentPlayingEntity, CombatManager.CurrentPlayingEntity.EntityCell)
                );
            }
        }

        private void _updateTurn(EntityEventData Data)
        {
            StartCoroutine(this._updateButtonsLater(Data.Entity));
        }

        private IEnumerator _updateButtonsLater(CharacterEntity entity)
        {
            yield return new WaitForSeconds(0.3f);

            bool interactable = CombatManager.Instance.IsPlayerOrOwned(entity);

            foreach (Button button in this.NextTurnButtons)
            {
                button.interactable = interactable;
            }
        }

        private void _updateEntityDeath(EntityEventData Data)
        {
            int index = CombatManager.Instance.PlayingEntities.IndexOf(Data.Entity);
            this.CombatEntities.RemoveAt(index);
        }
    }

}
