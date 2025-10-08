using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace TheFeelies.Managers
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager instance;
        public static UIManager Instance => instance;
        
        [Header("Dialogue UI")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI characterNameText;
        
        [Header("Continue Button")]
        [SerializeField] private GameObject continueButton;
        [SerializeField] private Button continueButtonComponent;
        [SerializeField] private TextMeshProUGUI continueButtonText;
        
        [Header("UI Settings")]
        [SerializeField] private float textSpeed = 0.05f;
        [SerializeField] private bool useTypewriterEffect = true;
        
        private Action onContinueButtonPressed;
        private Coroutine typewriterCoroutine;
        
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
    }
} 