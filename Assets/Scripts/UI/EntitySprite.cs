using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace DownBelow.UI
{
    public class EntitySprite : MonoBehaviour
    {
        [SerializeField] private Image _selectedBackground;
        [SerializeField] private Image _characterIcon;
        [SerializeField] private Image _weaponImage;

        public void Init(Sprite character, bool selected, Sprite weapon = null)
        {
            _characterIcon.sprite = character;
            SetSelected(selected);

            if (!ReferenceEquals(weapon, null))
                _weaponImage.sprite = weapon;
            else
                _weaponImage.gameObject.SetActive(false);
        }

        public void SetSelected(bool selected)
        {
            _selectedBackground.gameObject.SetActive(selected);
        }
    }
 
}
