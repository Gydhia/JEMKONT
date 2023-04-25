using DownBelow.Mechanics;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UIExtensibleCard : MonoBehaviour
    {
        [FoldoutGroup("D&D Attributes")]
        public TextMeshProUGUI ManaCost;
        [FoldoutGroup("D&D Attributes")]
        public TextMeshProUGUI CardName;
        [FoldoutGroup("D&D Attributes")]
        public TextMeshProUGUI Description;

        [FoldoutGroup("D&D Attributes")]
        public Image CardImage;

        [FoldoutGroup("D&D Attributes")]
        public GameObject Body;

        private bool _expanded = false;

        public void Init(ScriptableCard Card)
        {
            this.ManaCost.text = Card.Cost.ToString();
            this.CardName.text = Card.Title;
            this.Description.text = Card.Description;
            this.CardImage.sprite = Card.IllustrationImage;
        }

        //private void Update()
        //{
        //   if(RectTransformUtility.RectangleContainsScreenPoint((RectTransform)this.transform, Input.mousePosition))
        //    {
        //        if (Input.GetMouseButtonDown(0))
        //        {
        //            this.ToggleExpandCard();
        //        }
        //    }
        //}

        public void ToggleExpandCard()
        {
            this._expanded = !this._expanded;
            this.Body.gameObject.SetActive(this._expanded);
        }
    }

}
