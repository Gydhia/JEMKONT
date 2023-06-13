using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UIRewardItem : MonoBehaviour
    {
        public Color BackUnlockedColor;
        public Color CheckUnlockedColor;

        public Image RewardCheck;
        public Image Background;
        public Image RewardIcon;
        public TextMeshProUGUI RewardText;

        public void Init(bool alreadyUnlocked, string text, Sprite icon)
        {
            this.RewardIcon.sprite = icon;
            this.RewardText.text = text;

            if (alreadyUnlocked)
            {
                this.RewardCheck.color = CheckUnlockedColor;
                this.Background.color = BackUnlockedColor;
            }
        }
    }
}
