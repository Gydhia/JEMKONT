using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UICraftSectionAnim : MonoBehaviour
{
    [Header("Gears")] 
    [SerializeField] private Image _outerGear;
    [SerializeField] private Image _innerGear;
    
    [Header("Fire")]
    [SerializeField] private Image _fire;


    private Sequence gearsSequence;
    private Sequence fireSequence;
    

    public void Init()
    {

        gearsSequence = DOTween.Sequence();
        fireSequence = DOTween.Sequence();

        gearsSequence.Append(_outerGear.transform.DORotate(new Vector3(0, 0, 360), 4f).SetEase(Ease.Linear));
        gearsSequence.Join(_innerGear.transform.DORotate(new Vector3(0, 0, 360), 4f).SetEase(Ease.Linear));
       
        gearsSequence.SetLoops(-1, LoopType.Restart);
        gearsSequence.Pause();

        fireSequence.Append(_fire.DOFillAmount(1, 3f).SetEase(Ease.OutQuad));
        
        fireSequence.SetLoops(-1, LoopType.Restart);
        fireSequence.Pause();
    }

    public void ShowWorkshop()
    {
        _fire.gameObject.SetActive(false);
        _outerGear.gameObject.SetActive(true);
    }

    public void ShowFurnace()
    {
        _fire.gameObject.SetActive(true);
        _outerGear.gameObject.SetActive(false);
    }

    public void TempPlayAnims()
    {
        PlayFireSequence();
        PlayGearsSequence();
    }

    public void PlayGearsSequence()
    {
        gearsSequence.Restart();
    }

    public void PlayFireSequence()
    {
        fireSequence.Restart();
    }

    private void OnDisable()
    {
        gearsSequence.Pause();
        fireSequence.Pause();
    }
}
