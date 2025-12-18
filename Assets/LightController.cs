using UnityEngine;
using DG.Tweening;

public class LightController : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Light targetLight;
    [SerializeField] private float currentValue = 1f;
    [SerializeField] private float targetValue = 5f;
    [SerializeField] private float changeDuration = 1f;
    
    /// <summary>
    /// 현재값에서 목표값으로 변경
    /// </summary>
    public void SetToTargetValue()
    {
        if (targetLight != null)
        {
            targetLight.DOKill();
            targetLight.DOIntensity(targetValue, changeDuration);
        }
    }
    
    /// <summary>
    /// 목표값에서 현재값으로 되돌림
    /// </summary>
    public void SetToCurrentValue()
    {
        if (targetLight != null)
        {
            targetLight.DOKill();
            targetLight.DOIntensity(currentValue, changeDuration);
        }
    }
}
