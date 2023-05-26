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
    private bool sceneLoaded = false;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        // Afficher l'interface utilisateur de chargement
        loadingUI.SetActive(true);
        
        // Démarrer le chargement de la scène de manière asynchrone
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        // Charger la scène de manière asynchrone
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad);

        // Permettre au chargement de la scène de s'exécuter en arrière-plan
        asyncOperation.allowSceneActivation = false;

        // Attendre que le chargement de la scène atteigne 90%
        while (asyncOperation.progress < 0.9f)
        {
            yield return null;
        }

        // Mettre à jour le booléen pour indiquer que la scène est chargée
        sceneLoaded = true;

        asyncOperation.allowSceneActivation = true;
        // Attendre 10 secondes
        yield return new WaitForSeconds(5f);

        // Désactiver l'interface utilisateur de chargement
        loadingUI.SetActive(false);

        
    }

}
