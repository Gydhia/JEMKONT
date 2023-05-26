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
        
        // D�marrer le chargement de la sc�ne de mani�re asynchrone
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        // Charger la sc�ne de mani�re asynchrone
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad);

        // Permettre au chargement de la sc�ne de s'ex�cuter en arri�re-plan
        asyncOperation.allowSceneActivation = false;

        // Attendre que le chargement de la sc�ne atteigne 90%
        while (asyncOperation.progress < 0.9f)
        {
            yield return null;
        }

        // Mettre � jour le bool�en pour indiquer que la sc�ne est charg�e
        sceneLoaded = true;

        asyncOperation.allowSceneActivation = true;
        // Attendre 10 secondes
        yield return new WaitForSeconds(5f);

        // D�sactiver l'interface utilisateur de chargement
        loadingUI.SetActive(false);

        
    }

}
