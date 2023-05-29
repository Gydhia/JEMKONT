using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public string sceneToLoad;
    public GameObject loadingUI;


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        loadingUI.SetActive(true);
        
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad);

        asyncOperation.allowSceneActivation = false;

        while (asyncOperation.progress < 0.9f)
        {
            yield return null;
        }



        asyncOperation.allowSceneActivation = true;
        yield return new WaitForSeconds(5f);

        loadingUI.SetActive(false);

        
    }

}
