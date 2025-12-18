using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TheFeelies.Managers
{
    [System.Serializable]
    public class SubtitleData
    {
        public string text;
        public float duration;
    }

    [System.Serializable]
    public class SubtitleCollection
    {
        public List<SubtitleData> Chapter01;
    }

    public class UIManager : MonoBehaviour
    {
        private static UIManager instance;
        public static UIManager Instance => instance;
        
        [Header("Dialogue UI")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI characterNameText;
        
        [Header("Subtitle UI")]
        [SerializeField] private TextMeshProUGUI subtitleText;
        
        [Header("Continue Button")]
        [SerializeField] private GameObject continueButton;
        [SerializeField] private Button continueButtonComponent;
        [SerializeField] private TextMeshProUGUI continueButtonText;
        
        [Header("UI Settings")]
        [SerializeField] private float textSpeed = 0.05f;
        [SerializeField] private bool useTypewriterEffect = true;
        
        [Header("Subtitle Settings")]
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float fadeOutDuration = 1f;
        
        private Action onContinueButtonPressed;
        private Coroutine typewriterCoroutine;
        private Coroutine subtitleCoroutine;
        private SubtitleCollection subtitleCollection;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            if (continueButtonComponent != null)
            {
                continueButtonComponent.onClick.AddListener(OnContinueButtonPressed);
            }
            
            // 초기 상태 설정
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            if (continueButton != null)
                continueButton.SetActive(false);
            
            // 자막 초기화
            if (subtitleText != null)
            {
                subtitleText.text = "";
                SetSubtitleAlpha(0f);
            }
            
            // 자막 데이터 로드
            LoadSubtitleData();
        }
        
        private void LoadSubtitleData()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("Subtitles");
            if (jsonFile != null)
            {
                subtitleCollection = JsonUtility.FromJson<SubtitleCollection>(jsonFile.text);
                Debug.Log("자막 데이터 로드 완료");
            }
            else
            {
                Debug.LogError("Subtitles.json 파일을 찾을 수 없습니다. Resources 폴더에 파일이 있는지 확인하세요.");
            }
        }
        
        public void UpdateDialogue(string dialogue)
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);
                
            if (useTypewriterEffect)
            {
                if (typewriterCoroutine != null)
                    StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = StartCoroutine(TypewriterEffect(dialogue));
            }
            else
            {
                if (dialogueText != null)
                    dialogueText.text = dialogue;
            }
        }
        
        private System.Collections.IEnumerator TypewriterEffect(string text)
        {
            if (dialogueText == null) yield break;
            
            dialogueText.text = "";
            
            for (int i = 0; i < text.Length; i++)
            {
                dialogueText.text += text[i];
                yield return new WaitForSeconds(textSpeed);
            }
        }
        
        public void SetCharacterName(string characterName)
        {
            if (characterNameText != null)
                characterNameText.text = characterName;
        }
        
        public void ShowContinueButton(string buttonText, Action onPressed)
        {
            if (continueButton != null)
                continueButton.SetActive(true);
                
            if (continueButtonText != null)
                continueButtonText.text = buttonText;
                
            onContinueButtonPressed = onPressed;
        }
        
        public void HideContinueButton()
        {
            if (continueButton != null)
                continueButton.SetActive(false);
                
            onContinueButtonPressed = null;
        }
        
        private void OnContinueButtonPressed()
        {
            onContinueButtonPressed?.Invoke();
        }
        
        public void HideDialogue()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            if (continueButton != null)
                continueButton.SetActive(false);
                
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
        }
        
        public void SetTextSpeed(float speed)
        {
            textSpeed = Mathf.Max(0.01f, speed);
        }
        
        public void SetUseTypewriterEffect(bool use)
        {
            useTypewriterEffect = use;
        }
        
        public bool IsDialogueVisible()
        {
            return dialoguePanel != null && dialoguePanel.activeSelf;
        }
        
        public bool IsContinueButtonVisible()
        {
            return continueButton != null && continueButton.activeSelf;
        }
        
        public void ShowUI()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);
        }
        
        public void HideUI()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            if (continueButton != null)
                continueButton.SetActive(false);
        }
        
        // ==================== 자막 시스템 ====================
        
        /// <summary>
        /// 지정된 이름의 자막 시퀀스를 재생합니다.
        /// </summary>
        /// <param name="sequenceName">재생할 자막 시퀀스 이름 (예: "Chapter01")</param>
        public void PlaySubtitle(string sequenceName)
        {
            if (subtitleCollection == null)
            {
                Debug.LogError("자막 데이터가 로드되지 않았습니다.");
                return;
            }
            
            List<SubtitleData> subtitles = GetSubtitleSequence(sequenceName);
            
            if (subtitles == null || subtitles.Count == 0)
            {
                Debug.LogWarning($"'{sequenceName}' 자막 시퀀스를 찾을 수 없습니다.");
                return;
            }
            
            // 기존 자막이 재생 중이면 중지
            if (subtitleCoroutine != null)
            {
                StopCoroutine(subtitleCoroutine);
            }
            
            subtitleCoroutine = StartCoroutine(PlaySubtitleSequence(subtitles));
        }
        
        /// <summary>
        /// 자막 재생을 중지합니다.
        /// </summary>
        public void StopSubtitle()
        {
            if (subtitleCoroutine != null)
            {
                StopCoroutine(subtitleCoroutine);
                subtitleCoroutine = null;
            }
            
            if (subtitleText != null)
            {
                subtitleText.text = "";
                SetSubtitleAlpha(0f);
            }
        }
        
        private List<SubtitleData> GetSubtitleSequence(string sequenceName)
        {
            switch (sequenceName)
            {
                case "Chapter01":
                    return subtitleCollection.Chapter01;
                // 추후 다른 챕터 추가 시 여기에 case 추가
                default:
                    return null;
            }
        }
        
        private IEnumerator PlaySubtitleSequence(List<SubtitleData> subtitles)
        {
            foreach (SubtitleData subtitle in subtitles)
            {
                if (subtitleText != null)
                {
                    // 페이드인
                    subtitleText.text = subtitle.text;
                    yield return StartCoroutine(FadeSubtitle(0f, 1f, fadeInDuration));
                    
                    // 지속시간에서 페이드인/아웃 시간을 뺀 만큼 대기
                    float displayTime = subtitle.duration - fadeInDuration - fadeOutDuration;
                    if (displayTime > 0)
                    {
                        yield return new WaitForSeconds(displayTime);
                    }
                    
                    // 페이드아웃
                    yield return StartCoroutine(FadeSubtitle(1f, 0f, fadeOutDuration));
                    
                    subtitleText.text = "";
                }
            }
            
            subtitleCoroutine = null;
        }
        
        private IEnumerator FadeSubtitle(float startAlpha, float endAlpha, float duration)
        {
            if (subtitleText == null) yield break;
            
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                SetSubtitleAlpha(alpha);
                yield return null;
            }
            
            SetSubtitleAlpha(endAlpha);
        }
        
        private void SetSubtitleAlpha(float alpha)
        {
            if (subtitleText != null)
            {
                Color color = subtitleText.color;
                color.a = alpha;
                subtitleText.color = color;
            }
        }
        
        /// <summary>
        /// 자막이 현재 재생 중인지 확인합니다.
        /// </summary>
        public bool IsSubtitlePlaying()
        {
            return subtitleCoroutine != null;
        }
    }
} 