using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class AlterationsFeedback : MonoBehaviour
    {
        [SerializeField] private Image _alterationIcon;

        public void SetAlteration(Sprite sprite)
        {
            _alterationIcon.sprite = sprite;
        }
    }

}

