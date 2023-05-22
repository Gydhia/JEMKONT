using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UIStaticCombat : MonoBehaviour
    {
        public RectTransform LeftPin;
        public RectTransform RightPin;

        public Button StartCombat;

        private void Awake()
        {
            StartCombat.onClick.AddListener(() => NetworkManager.Instance.PlayerAsksToStartCombat());
        }

    }
}