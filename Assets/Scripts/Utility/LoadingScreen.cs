using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DownBelow.Managers;
using DG.Tweening;

public class LoadingScreen : MonoBehaviour
{
    public GameObject loadingScreen;
    public Slider slider;
    public TextMeshProUGUI progressText;
    public CanvasGroup Group;

    private bool _canHideLoading = false;
    private bool _isLoading = false;

    public void Start()
    {

        Group.alpha = 1;
        slider.value = 0f;
        progressText.text = 0 + "%";

        slider.DOValue(99f, 3.5f).SetEase(Ease.OutQuart).OnComplete(() => LoadLevel());
    }

    public void LoadLevel()
    {
        slider.DOValue(100f, 1f).SetEase(Ease.OutQuart).OnComplete(() =>
        {
            this.gameObject.SetActive(true);
            _isLoading = true;

        });
       
    }

    private void Update()
    {
        progressText.text = (int)slider.value + "%";
    }



}
