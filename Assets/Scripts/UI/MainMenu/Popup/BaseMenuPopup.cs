using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.UI.Menu
{
    public class BaseMenuPopup : MonoBehaviour
    {
        public MenuPopup PopupType;

        public void ShowPopup()
        {
            this.gameObject.SetActive(true);
        }

        public void HidePopup()
        {
            this.gameObject.SetActive(false);
        }
    }

}