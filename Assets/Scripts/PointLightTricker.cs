using UnityEngine;
using DG.Tweening;

public class PointLightTricker : MonoBehaviour
{
    [Header("Point Light Settings")]
    [SerializeField] private Light pointLight;
    [SerializeField] private float flickerIntensity = 0.1f;
    [SerializeField] private float flickerDuration = 2f;
    
    private Sequence flickerSequence;
    private bool isFlickering = false;
    
    void Start()
    {
        
    }
    
    /// <summary>
    /// 포인트라이트 깜빡거림 시작 (0 -> 0.1 -> 0 반복)
    /// </summary>
    public void StartFlickering()
    {
        if (pointLight == null) return;
        
        if (isFlickering)
        {
            Debug.LogWarning($"PointLight on {gameObject.name} is already flickering!");
            return;
        }
        
        isFlickering = true;
        
        // 기존 시퀀스가 있다면 정리
        if (flickerSequence != null)
        {
            flickerSequence.Kill();
        }
        
        // 깜빡거림 시퀀스 생성
        flickerSequence = DOTween.Sequence();
        
        flickerSequence.Append(DOTween.To(() => pointLight.intensity, x => pointLight.intensity = x, flickerIntensity, flickerDuration))
                      .Append(DOTween.To(() => pointLight.intensity, x => pointLight.intensity = x, 0f, flickerDuration))
                      .SetLoops(-1); // 무한 반복
        
        Debug.Log($"PointLight on {gameObject.name} started flickering");
    }
    
    /// <summary>
    /// 깜빡거림 중지하고 현재 Intensity에서 1초만에 0으로 만들기
    /// </summary>
    public void StopFlickering()
    {
        if (pointLight == null) return;
        
        if (!isFlickering)
        {
            Debug.LogWarning($"PointLight on {gameObject.name} is not flickering!");
            return;
        }
        
        isFlickering = false;
        
        // 기존 시퀀스 정리
        if (flickerSequence != null)
        {
            flickerSequence.Kill();
            flickerSequence = null;
        }
        
        // 현재 Intensity에서 1초만에 0으로
        DOTween.To(() => pointLight.intensity, x => pointLight.intensity = x, 0f, 1f)
               .SetEase(Ease.OutQuad);
        
        Debug.Log($"PointLight on {gameObject.name} stopped flickering and fading to 0");
    }
    
    void OnDestroy()
    {
        // 오브젝트 파괴 시 시퀀스 정리
        if (flickerSequence != null)
        {
            flickerSequence.Kill();
        }
    }
}
