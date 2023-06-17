using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DownBelow.UI.Menu;
using UnityEngine;

public class MenuPopup_Credits : BaseMenuPopup
{
    [SerializeField] private RectTransform _namesText;
    [SerializeField] private RectTransform _jobText;

    [SerializeField] private float _defilementTime = 15f;
    [SerializeField] private Ease _creditsEase = Ease.Linear;

    private Sequence _namesSequence;
    private Sequence _jobsSequence;


    public override void ShowPopup()
    {
        base.ShowPopup();
        _namesText.gameObject.SetActive(true);
        _jobText.gameObject.SetActive(true);
        _namesText.DOAnchorMin(new Vector2(_namesText.anchorMin.x, -1), 0);
        _namesText.DOAnchorMax(new Vector2(_namesText.anchorMax.x, 0), 0);
        _jobText.DOAnchorMin(new Vector2(_jobText.anchorMin.x, -1), 0);
        _jobText.DOAnchorMax(new Vector2(_jobText.anchorMax.x, 0), 0);
        
        StartDefilement();
    }

    private void StartDefilement()
    {
        _namesSequence = DOTween.Sequence();
        
        _namesSequence.Append(_namesText.DOAnchorMin(new Vector2(_namesText.anchorMin.x, 1), _defilementTime).SetEase(_creditsEase));
        _namesSequence.Join(_namesText.DOAnchorMax(new Vector2(_namesText.anchorMax.x, 2), _defilementTime).SetEase(_creditsEase));
        _namesSequence.Append(_namesText.DOAnchorMin(new Vector2(_jobText.anchorMin.x, -1), 0));
        _namesSequence.Join(_namesText.DOAnchorMax(new Vector2(_jobText.anchorMax.x, 0), 0));
        
        _namesSequence.SetLoops(-1);
        
        
        _jobsSequence = DOTween.Sequence();
        
        _jobsSequence.Append(_jobText.DOAnchorMin(new Vector2(_jobText.anchorMin.x, 1), _defilementTime).SetEase(_creditsEase));
        _jobsSequence.Join(_jobText.DOAnchorMax(new Vector2(_jobText.anchorMax.x, 2), _defilementTime).SetEase(_creditsEase));
        _jobsSequence.Append(_jobText.DOAnchorMin(new Vector2(_jobText.anchorMin.x, -1), 0));
        _jobsSequence.Join(_jobText.DOAnchorMax(new Vector2(_jobText.anchorMax.x, 0), 0));

        _jobsSequence.SetLoops(-1);
        
        _namesSequence.Restart();
        _jobsSequence.Restart();
    }

    public override void HidePopup()
    {
        base.HidePopup();
        _namesSequence.Pause();
        _jobsSequence.Pause();
        _namesText.DOAnchorMin(new Vector2(_namesText.anchorMin.x, -1), 0);
        _namesText.DOAnchorMax(new Vector2(_namesText.anchorMax.x, 0), 0);
        _jobText.DOAnchorMin(new Vector2(_jobText.anchorMin.x, -1), 0);
        _jobText.DOAnchorMax(new Vector2(_jobText.anchorMax.x, 0), 0);
        _namesText.gameObject.SetActive(false);
        _jobText.gameObject.SetActive(false);
    }
}
