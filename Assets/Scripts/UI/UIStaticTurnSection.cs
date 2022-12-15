using DownBelow.Entity;
using DownBelow.Events;
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

        public Slider TimeSlider;
        public Button NextTurnButton;

        public List<Image> CombatEntities;

        public void Init(List<CharacterEntity> combatEntities)
        {
            for (int i = 0; i < combatEntities.Count; i++)
            {
                this.CombatEntities.Add(Instantiate(this.SpritePrefab, this.EntitiesHolder, combatEntities[i]));
                this.CombatEntities[i].sprite = combatEntities[i].IsAlly ? AllySprite : EnemySprite;
                if (i > 0)
                    this.CombatEntities[i].transform.GetChild(0).gameObject.SetActive(false);
            }

            CombatManager.Instance.OnTurnStarted += this._updateTurn;
            CombatManager.Instance.OnTurnEnded+= this._updateTurn;
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
                CombatManager.Instance.ProcessEndTurn(GameManager.Instance.SelfPlayer.UID);
        }

        private void _updateTurn(EntityEventData Data)
        {
            NextTurnButton.interactable = CombatManager.Instance.CurrentPlayingEntity == GameManager.Instance.SelfPlayer;

            if (Data.Entity != GameManager.Instance.SelfPlayer)
                return;
        }
    }

}
