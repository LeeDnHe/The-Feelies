using System.Collections;
using TMPro;
using UnityEngine;

namespace TheFeelies.UI
{
    /// <summary>
    /// Canvas를 페이드 인/아웃하는 컴포넌트
    /// 챕터 전환 시 화면 전체를 페이드 효과로 덮음
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeCanvas : MonoBehaviour
    {
        [Header("Fade Settings")]
        [SerializeField] private float quickFadeDuration = 0.5f;
        
        public Coroutine CurrentRoutine { get; private set; } = null;
        private CanvasGroup canvasGroup = null;
        private float alpha = 0.0f;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            
            // Scene의 페이드 이벤트 구독
            Core.Scene.FadeIn += StartFadeIn;
            Core.Scene.FadeOut += StartFadeOut;
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            Core.Scene.FadeIn -= StartFadeIn;
            Core.Scene.FadeOut -= StartFadeOut;
        }

        /// <summary>
        /// 페이드 인 시작 (화면이 밝아짐 - 알파값 1 -> 0)
        /// </summary>
        public void StartFadeIn(float fadeDuration)
        {
            canvasGroup.alpha = 1f;
            StopAllCoroutines();
            CurrentRoutine = StartCoroutine(FadeIn(fadeDuration));
        }

        /// <summary>
        /// 페이드 아웃 시작 (화면이 어두워짐 - 알파값 0 -> 1)
        /// </summary>
        public void StartFadeOut(float fadeDuration)
        {
            StopAllCoroutines();
            CurrentRoutine = StartCoroutine(FadeOut(fadeDuration));
        }

        /// <summary>
        /// 빠른 페이드 인
        /// </summary>
        public void QuickFadeIn()
        {
            StopAllCoroutines();
            CurrentRoutine = StartCoroutine(FadeIn(quickFadeDuration));
        }

        /// <summary>
        /// 빠른 페이드 아웃
        /// </summary>
        public void QuickFadeOut()
        {
            StopAllCoroutines();
            CurrentRoutine = StartCoroutine(FadeOut(quickFadeDuration));
        }

        /// <summary>
        /// 페이드 인 코루틴 (1 -> 0)
        /// </summary>
        private IEnumerator FadeIn(float duration)
        {
            float elapsedTime = 0.0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                SetAlpha(1f - (elapsedTime / duration));
                yield return null;
            }
            
            SetAlpha(0f);
        }

        /// <summary>
        /// 페이드 아웃 코루틴 (0 -> 1)
        /// </summary>
        private IEnumerator FadeOut(float duration)
        {
            float elapsedTime = 0.0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                SetAlpha(elapsedTime / duration);
                yield return null;
            }
            
            SetAlpha(1f);
        }

        /// <summary>
        /// 알파값 설정
        /// </summary>
        private void SetAlpha(float value)
        {
            alpha = Mathf.Clamp01(value);
            canvasGroup.alpha = alpha;
        }

        /// <summary>
        /// 즉시 알파값 설정 (페이드 효과 없이)
        /// </summary>
        public void SetAlphaImmediate(float value)
        {
            StopAllCoroutines();
            SetAlpha(value);
        }
    }
}

