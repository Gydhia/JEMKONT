using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ToolOnGround : MonoBehaviour
{
    [SerializeField] private float _delayBeforeStartLoop;
    
    public bool IsOnGround = true;

    private Sequence _loopSequence;

    //TEMP

    public void Init(bool isOnGround)
    {
        this.IsOnGround = isOnGround;
        
        if(this.IsOnGround)
            StartCoroutine(DelayBeforeStart());
    }

    private IEnumerator DelayBeforeStart()
    {
        yield return new WaitForSeconds(_delayBeforeStartLoop);
        SetSequence();
    }

    private void SetSequence()
    {
        Vector3 jumpPos = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y + 0.8f,this.transform.localPosition.z);
        
        _loopSequence = DOTween.Sequence();
        _loopSequence.Append(this.transform.DOLocalJump(jumpPos,0.5f, 2, 4f,false).SetEase(Ease.InBounce));
        _loopSequence.SetLoops(-1, LoopType.Yoyo);
        _loopSequence.Restart();
    }
}
