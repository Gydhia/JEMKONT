using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace DownBelow.Loading
{
    public class LoadingScreen : MonoBehaviour
    {
        public GameObject Content;
        public List<Sprite> SpritePool;
        public Image TargetImage;

        // Special Singleton for the loading screen
        private static LoadingScreen _instance;
        public static LoadingScreen Instance
        {
            get
            {
                if (_instance == null)
                {
                    LoadingScreen sceneLoader = FindObjectOfType<LoadingScreen>(true);
                    if (sceneLoader != null)
                    {
                        _instance = sceneLoader;
                    }
                    else
                    {
                        LoadingScreen SceneLoaderPrefab = Resources.Load<LoadingScreen>("Prefabs/Loading/LoadingScreen");
                        _instance = Instantiate(SceneLoaderPrefab);
                    }
                }
                return _instance;
            }
        }

        public void Awake()
        {
            // Get rid of any old LoadingScreen
            if (Instance != null)
            {
                if (Instance != this)
                {
                    Destroy(this);
                    return;
                }

            }

            Object.DontDestroyOnLoad(this.gameObject);
            this.Content.SetActive(false);
        }

        public void Show()
        {
            if (this.Content.activeInHierarchy) { return; }

            this.Content.SetActive(true);
            this.ChangeImageSprite();
        }

        public void Hide()
        {
            this.Content.SetActive(false);
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

}