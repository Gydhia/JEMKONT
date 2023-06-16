using DownBelow;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEnchantRaw : MonoBehaviour
{
    public Color BuffColor;
    public Image StatIcon;
    public TextMeshProUGUI AmountText;

    public void Refresh(EntityStatistics stat, int baseAmount, int upgradeAmount)
    {
        this.StatIcon.sprite = SettingsManager.Instance.GameUIPreset.StatisticSprites[stat];

        string color = "<color=#" + ColorUtility.ToHtmlStringRGB(BuffColor) + ">";
        this.AmountText.text = baseAmount.ToString() + " + " + color + upgradeAmount.ToString() + "</color>";
    }
}
