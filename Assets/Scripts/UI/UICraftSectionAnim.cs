using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UICraftSectionAnim : MonoBehaviour
{
    [Header("Gears")] 
    [SerializeField] private GameObject _gearParent;
    [SerializeField] private Image _outerGear;
    [SerializeField] private Image _innerGear;
    
    [Header("Fire")]
    [SerializeField] private GameObject _fireParent;
    [SerializeField] private Image _fire;


    private Sequence gearsSequence;
    private Sequence fireSequence;

    public Action OnCraftComplete;
    
    public void Init()
    {
        
        gearsSequence = DOTween.Sequence();
        fireSequence = DOTween.Sequence();

        gearsSequence.Append(_outerGear.rectTransform.DOLocalRotate(new Vector3(0, 0, 360), 4f).SetEase(Ease.Linear)); 
        gearsSequence.Join(_innerGear.rectTransform.DOLocalRotate(new Vector3(0, 0, 360), 4f).SetEase(Ease.Linear).OnComplete((() => OnCraftComplete?.Invoke())));
       
        gearsSequence.SetLoops(-1, LoopType.Restart);
        gearsSequence.Pause();

        fireSequence.Append(_fire.DOFillAmount(1, 3f).SetEase(Ease.OutQuad)).OnComplete((() => OnCraftComplete?.Invoke()));;
        
        fireSequence.SetLoops(-1, LoopType.Restart);
        fireSequence.Pause();
    }

    public void ShowWorkshop()
    {
        _fireParent.gameObject.SetActive(false);
        _gearParent.gameObject.SetActive(true);
    }

    public void ShowFurnace()
    {
        _fireParent.gameObject.SetActive(true);
        _gearParent.gameObject.SetActive(false);
    }

    public void PlayAnims()
    {
        if(_fireParent.gameObject.activeInHierarchy)
            PlayFireSequence();
        else
            PlayGearsSequence();
    }

    private void PlayGearsSequence()
    {
        gearsSequence.Restart();
    }

    private void PlayFireSequence()
    {
        fireSequence.Restart();
    }

    private void OnDisable()
    {
        gearsSequence.Pause();
        fireSequence.Pause();
    }
}
