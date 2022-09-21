using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.UI;
using TMPro;
public class CardComponent : MonoBehaviour {
	
	public ScriptableCard CardData;
	public SpriteRenderer IllustrationImage;
	public TextMeshPro CostText;
	public TextMeshPro TitleText;
	public TextMeshPro DescText;
	public void Hydrate(ScriptableCard CardData) {
		this.CardData = CardData;
		this.IllustrationImage.sprite = CardData.IllustrationImage;
		this.CostText.text = CardData.Cost.ToString();
		this.TitleText.text = CardData.Title;
		this.DescText.text = CardData.Description;
	}
	public void Hydrate() {
		//CardData is already the card we want:
		this.IllustrationImage.sprite = this.CardData.IllustrationImage;
		this.CostText.text = this.CardData.Cost.ToString();
		this.TitleText.text = this.CardData.Title;
		this.DescText.text = this.CardData.Description;
	}
}
