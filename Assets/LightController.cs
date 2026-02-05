using UnityEngine;
using DG.Tweening;

public class LightController : MonoBehaviour
{
    [Header("Light Settings")]
    [Tooltip("동시에 제어할 라이트들")]
    [SerializeField] private Light[] targetLights;

    [Tooltip("페이드(0↔1) 진행 시간(초)")]
    [SerializeField] private float changeDuration = 1f;

    [Tooltip("0으로 내리기 시작할 때 각 라이트의 현재 intensity를 '원래값'으로 다시 캐싱합니다.")]
    [SerializeField] private bool recacheBaseIntensitiesOnFadeOut = true;

    private float[] _baseIntensities;
    private float _fadeFactor = 1f; // 1 = 원래값, 0 = 완전 소등
    private Tween _fadeTween;

    private void Awake() {
        EnsureCache();
        CacheCurrentIntensitiesAsBase();
        ApplyFactor();
    }

    private void OnDestroy() {
        _fadeTween?.Kill();
        _fadeTween = null;
    }
    
    /// <summary>
    /// (호환용) 전체를 동시에 0까지 내림
    /// </summary>
    public void SetToTargetValue()
    {
        FadeOutAll();
    }
    
    /// <summary>
    /// (호환용) 전체를 동시에 원래값으로 복귀
    /// </summary>
    public void SetToCurrentValue()
    {
        FadeInAll();
    }

    /// <summary>
    /// 현재 intensity들을 각 라이트의 "원래값"으로 저장합니다.
    /// </summary>
    public void CacheCurrentIntensitiesAsBase() {
        EnsureCache();

        for (int i = 0; i < targetLights.Length; i++) {
            var l = targetLights[i];
            _baseIntensities[i] = l ? Mathf.Max(0f, l.intensity) : 0f;
        }
    }

    /// <summary>
    /// intensity가 서로 달라도, 각 라이트의 현재값을 1로 normalize한 것처럼
    /// "같은 시간" 안에 모두 0까지 내려가게 합니다.
    /// </summary>
    public void FadeOutAll() {
        if (targetLights == null || targetLights.Length == 0) return;

        if (recacheBaseIntensitiesOnFadeOut) {
            CacheCurrentIntensitiesAsBase();
        }

        StartFade(0f);
    }

    /// <summary>
    /// FadeOutAll로 내려가기 전의 "원래값"으로, 모두 같은 시간에 복귀합니다.
    /// 예: 5와 10이면 같은 duration에 5/10으로 동시에 돌아옵니다.
    /// </summary>
    public void FadeInAll() {
        if (targetLights == null || targetLights.Length == 0) return;
        StartFade(1f);
    }

    private void StartFade(float targetFactor) {
        EnsureCache();

        // 라이트에 걸린 intensity 트윈이 있으면 충돌 방지
        for (int i = 0; i < targetLights.Length; i++) {
            var l = targetLights[i];
            if (l != null) l.DOKill();
        }

        _fadeTween?.Kill();
        _fadeTween = DOTween.To(
                () => _fadeFactor,
                v => {
                    _fadeFactor = v;
                    ApplyFactor();
                },
                targetFactor,
                changeDuration
            )
            .SetTarget(this);
    }

    private void ApplyFactor() {
        if (targetLights == null || _baseIntensities == null) return;

        float f = Mathf.Clamp01(_fadeFactor);
        int count = Mathf.Min(targetLights.Length, _baseIntensities.Length);

        for (int i = 0; i < count; i++) {
            var l = targetLights[i];
            if (l == null) continue;
            l.intensity = _baseIntensities[i] * f;
        }
    }

    private void EnsureCache() {
        if (targetLights == null) targetLights = new Light[0];
        if (_baseIntensities == null || _baseIntensities.Length != targetLights.Length) {
            _baseIntensities = new float[targetLights.Length];
        }
    }
}
