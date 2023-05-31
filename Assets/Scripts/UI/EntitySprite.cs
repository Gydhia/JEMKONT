using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace DownBelow.UI
{
    public class EntitySprite : MonoBehaviour
    {
        [SerializeField] private Image _playingBackground;
        [SerializeField] private Image _characterIcon;
        [SerializeField] private Image _weaponImage;
        [SerializeField] private Image _ownedImage;
        [SerializeField] private TextMeshProUGUI _ownedInput;
        [SerializeField] private GameObject _selected;

        private CharacterEntity _refEntity;

        public void Init(CharacterEntity character, bool selected)
        {
            this._refEntity = character;
            this._refEntity.OnDeath += this.SetDead;
            GameManager.Instance.OnSelfPlayerSwitched += _toggleSelectedState;

            this._characterIcon.sprite = character.IsAlly ? 
                SettingsManager.Instance.GameUIPreset.Ally :
                ((EnemyEntity)character).EnemyStyle.EntityIcon;
            
            if (character is PlayerBehavior player)
            {
                this._ownedImage.gameObject.SetActive(CombatManager.Instance.IsPlayerOrOwned(player));
                this._weaponImage.sprite = player.CombatTool.FightIcon;
                this._ownedInput.text = (player.Index + 1).ToString();
            }
            else
            {
                this._ownedImage.gameObject.SetActive(false);
                this._weaponImage.gameObject.SetActive(false);
            }

            SetSelected(selected);
        }

        private void _toggleSelectedState(EntityEventData Data)
        {
            this._selected.SetActive(Data.Entity == this._refEntity);
        }

        public void SetSelected(bool selected)
        {
            this._playingBackground.sprite = selected ?
                SettingsManager.Instance.GameUIPreset.SelectedBackground :
                SettingsManager.Instance.GameUIPreset.NormalBackground;
        }

        public void SetDead(EntityEventData Data)
        {
            this._refEntity.OnDeath -= this.SetDead;

            this._playingBackground.gameObject.SetActive(false);
            this._ownedImage.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            this._refEntity.OnDeath -= this.SetDead;
            GameManager.Instance.OnSelfPlayerSwitched -= _toggleSelectedState;
        }
    }
 
}
