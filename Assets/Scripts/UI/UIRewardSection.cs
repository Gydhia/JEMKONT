using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UIRewardSection : MonoBehaviour
    {
        public TextMeshProUGUI LevelNumber;
        public GameObject Aura;

        private void Start()
        {
            this.gameObject.SetActive(false);
        }

        private void Update()
        {
            Aura.transform.Rotate(Vector3.forward * Time.deltaTime * 6f);
        }
    }
}