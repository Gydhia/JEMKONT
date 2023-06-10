using DownBelow.Entity;
using DownBelow.Managers;
using DownBelow.Mechanics;
using DownBelow.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class PurchaseCardsAction : EntityAction
    {
        public ScriptableCard card;

        public PurchaseCardsAction(CharacterEntity RefEntity, Cell TargetCell) : base(RefEntity, TargetCell)
        {
            Init();
        }

        public void Init(ScriptableCard card)
        {
            this.card = card;
        }

        public override void ExecuteAction()
        {
            if (SettingsManager.Instance.OwnedCards.Contains(card))
            {
                Debug.LogError(card.name + " IS ALREADY IN THE OWNED CARDS");
                EndAction();
                return;
            }

            SettingsManager.Instance.OwnedCards.Add(card);
            EndAction();
        }

        public override object[] GetDatas()
        {
            return new object[1] { card.UID };
        }

        public override void SetDatas(object[] Datas)
        {
            this.card = SettingsManager.Instance.ScriptableCards[Guid.Parse(Datas[0] as string)];
        }
    }

    public class Interactable_P_Card : InteractablePurchase<ScriptableCard>
    {
        public UIExtensibleCard UICard;
        public Transform OrbHolder;
        public ParticleSystem BuyParticle;

        public override List<ScriptableCard> GetItemsPool()
        {
            return SettingsManager.Instance.ScriptableCards.Values.Where(c => c.Class == this.Preset.SpecificClass).Except(SettingsManager.Instance.OwnedCards).ToList();
        }

        public override void Init(InteractablePreset InteractableRef, Cell RefCell)
        {
            base.Init(InteractableRef, RefCell);

            var particle = Instantiate(this.Preset.OrbParticlePrefab, this.OrbHolder);
            particle.gameObject.transform.localScale = new Vector3(4f, 4f, 4f);

            GameManager.Instance.OnGameStarted += Instance_OnGameStarted;
        }

        private void Instance_OnGameStarted(Events.GameEventData Data)
        {
            this.RefreshPurchase();
        }

        public override void GiveItemToPlayer(ScriptableCard Item)
        {
            var act = new PurchaseCardsAction(GameManager.SelfPlayer, GameManager.SelfPlayer.EntityCell);
            act.Init(Item);
            NetworkManager.Instance.EntityAskToBuffAction(act);
        }

        protected override void RefreshPurchase()
        {
            var pooledCards = this.GetItemsPool();

            if (pooledCards == null || pooledCards.Count <= 0)
            {
                this.RefCell.ChangeCellState(CellState.Walkable);
                this.gameObject.SetActive(false);
                return;
            }
            string UID = GameManager.MasterPlayer.UID;

            var choosenCard = pooledCards[RandomHelper.RandInt(0, pooledCards.Count, UID)];

            this.Preset.ActualizeCosts();
            this.CurrentItemPurchase = choosenCard;
            this.UICard.Init(this.CurrentItemPurchase);
        }

        protected override void TryBuy(PlayerBehavior player)
        {
            if (this.CanBuy())
            {
                Debug.Log("Just bought : " + this.CurrentItemPurchase);

                this.BuyParticle.Play();

                this.GiveItemToPlayer(this.CurrentItemPurchase);

                foreach (var item in this.Preset.Costs)
                    player.PlayerInventory.RemoveItem(item.Key, item.Value);

                this.RefreshPurchase();
            } else
            {
                UIManager.Instance.DatasSection.ShowWarningText("You're missing ressources to buy");
            }
        }
    }
}