using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIPlayerInfos : MonoBehaviour
{
    public TextMeshProUGUI ManaText;
    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI MoveText;

    public void SetManaText(int value)
    {
        this.ManaText.text = value.ToString();
    }
    public void SetHealthText(int value)
    {
        this.HealthText.text = value.ToString();
    }
    public void SetMoveText(int value)
    {
        this.MoveText.text = value.ToString();
    }
}
