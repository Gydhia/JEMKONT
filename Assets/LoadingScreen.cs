using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public List<Sprite> SpritePool;
    public Image TargetImage;

    private bool _inited = false;
    
    public void Init()
    {
        if (this._inited) return;

        this._inited = true;

        DontDestroyOnLoad(this.gameObject);
        this.gameObject.SetActive(false);
    }

    public void Show()
    {
        this.ChangeImageSprite();
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void ChangeImageSprite()
    {
        if (SpritePool.Count == 0)
        {
            Debug.LogError("La liste des sprites est vide !");
            return;
        }

        int randomIndex = Random.Range(0, SpritePool.Count);

        TargetImage.sprite = SpritePool[randomIndex];
    }
}
