using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DotweenSequence : MonoBehaviour
{
    [Header("닷트윈 애니메이션 리스트")]
    public List<DOTweenAnimation> animations = new List<DOTweenAnimation>();
    
    private Sequence sequence;
    
    void Start()
    {
        CreateSequence();
    }
    
    void CreateSequence()
    {
        sequence = DOTween.Sequence();
        
        foreach (var anim in animations)
        {
            if (anim != null && anim.isValid)
            {
                anim.CreateTween();
                if (anim.tween != null)
                {
                    sequence.Append(anim.tween);
                }
            }
        }
    }
    
    [ContextMenu("시퀀스 재생")]
    public void PlaySequence()
    {
        if (sequence != null)
        {
            sequence.Restart();
        }
    }
    
    [ContextMenu("시퀀스 중지")]
    public void StopSequence()
    {
        if (sequence != null)
        {
            sequence.Pause();
        }
    }
}
