
using DownBelow.Managers;
using DownBelow.Mechanics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DownBelow.Events;

using System.Linq;

namespace DownBelow.UI
{
    public class CardVisual : MonoBehaviour
    {
        public Image IllustrationImage;
        public Image ShineImage;
        public Image BackImage;

        public TextMeshProUGUI CostText;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI DescText;

        [HideInInspector] public ScriptableCard CardReference;

        private void OnEnable()
        {
            if(this.CardReference != null)
            {
                this.CostText.color = GameManager.SelfPlayer.Mana < this.CardReference.Cost ? Color.red : Color.white;
            }
        }

        public void Init(ScriptableCard CardReference)
        {
            this.CardReference = CardReference;
            this.ShineImage.enabled = false;
            this.CostText.text = this.CardReference.Cost.ToString();
            this.TitleText.text = this.CardReference.Title;
            this.DescText.text = this.CardReference.Description;
            this.IllustrationImage.sprite = this.CardReference.IllustrationImage;

            this.CardReference = CardReference;

            ShineImage.color = CardReference.CardType switch
            {
                CardType.Attack => SettingsManager.Instance.GameUIPreset.AttackColor,
                CardType.Power => SettingsManager.Instance.GameUIPreset.PowerColor,
                CardType.Skill => SettingsManager.Instance.GameUIPreset.SkillColor,
                CardType.None => SettingsManager.Instance.GameUIPreset.SkillColor,
                _ => SettingsManager.Instance.GameUIPreset.SkillColor,
            };


            if (GameManager.RealSelfPlayer.CurrentGrid.IsCombatGrid)
            {
                var refDeck = CardsManager.Instance.AvailableDecks.First(d => d.Class == this.CardReference.Class);

                refDeck.LinkedPlayer.OnManaAdded += _refreshManaColor;
                refDeck.LinkedPlayer.OnManaRemoved += _refreshManaColor;
            }
        }

        private void _refreshManaColor(SpellEventData Data)
        {
            this.CostText.color = GameManager.SelfPlayer.Mana < this.CardReference.Cost ? Color.red : Color.white;
        }

        public void Hover()
        {
            this.ShineImage.enabled = true;
        }
        public void Unhover()
        {
            this.ShineImage.enabled = false;
        }

        public void ReverseCard()
        {
            BackImage.gameObject.SetActive(!BackImage.gameObject.activeInHierarchy);
        }
    }
}