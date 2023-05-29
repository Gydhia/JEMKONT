using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageChanger : MonoBehaviour
{
    public List<Sprite> spritePool; 
    public Image targetImage; 

    private void Start()
    {
        ChangeImageSprite();
    }

    public void ChangeImageSprite()
    {
     
        if (spritePool.Count == 0)
        {
            Debug.LogError("La liste des sprites est vide !");
            return;
        }

        int randomIndex = Random.Range(0, spritePool.Count);

        targetImage.sprite = spritePool[randomIndex];
    }
}