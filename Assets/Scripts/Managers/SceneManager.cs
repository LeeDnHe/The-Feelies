using UnityEngine;
using TheFeelies.Core;
using System.Collections;

namespace TheFeelies.Managers
{
    /// <summary>
    /// 새로운 스토리 시스템을 사용하는 씬 매니저
    /// 기존 시스템과의 호환성을 위해 유지되지만, 새로운 Scene 컴포넌트를 사용
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        [Header("Story Settings")]
        [SerializeField] private Scene currentScene;
        [SerializeField] private CharacterType selectedCharacter = CharacterType.Sherlock;
        
        [Header("Managers")]
        [SerializeField] private SoundManager soundManager;
        [SerializeField] private AnimationManager animationManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private VFXManager vfxManager;
        [SerializeField] private PlayerManager playerManager;
        
        private bool isStoryPlaying = false;
        
        #region Singleton
        public static SceneManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManagers();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion
        
        private void Start()
        {
            InitializeManagers();
            LoadStory();
        }
        
        private void InitializeManagers()
        {
            // 매니저들이 없으면 자동으로 찾거나 생성
            if (soundManager == null)
                soundManager = FindObjectOfType<SoundManager>();
            if (animationManager == null)
                animationManager = FindObjectOfType<AnimationManager>();
            if (uiManager == null)
                uiManager = FindObjectOfType<UIManager>();
            if (vfxManager == null)
                vfxManager = FindObjectOfType<VFXManager>();
            if (playerManager == null)
                playerManager = FindObjectOfType<PlayerManager>();
        }
        
        public void LoadStory()
        {
            if (currentScene == null)
            {
                Debug.LogError("Scene이 설정되지 않았습니다!");
                return;
            }
            
            Debug.Log($"스토리 로드됨: {currentScene.SceneName}");
        }
        
        public void StartStory()
        {
            if (isStoryPlaying)
            {
                Debug.LogWarning("스토리가 이미 재생 중입니다.");
                return;
            }
            
            if (currentScene == null)
            {
                Debug.LogError("Scene이 설정되지 않았습니다!");
                return;
            }
            
            isStoryPlaying = true;
            currentScene.StartScene();
        }
        
        public void StopStory()
        {
            if (currentScene != null)
            {
                currentScene.StopScene();
            }
            isStoryPlaying = false;
        }
        
        #region Public Methods
        public void SetScene(Scene scene)
        {
            currentScene = scene;
            LoadStory();
        }
        
        public void SetCharacter(CharacterType character)
        {
            selectedCharacter = character;
        }
        
        public void JumpToChapter(int chapterIndex)
        {
            if (currentScene != null)
            {
                currentScene.StartFromChapter(chapterIndex);
            }
        }
        
        public void SkipToNextChapter()
        {
            if (currentScene != null)
            {
                currentScene.SkipToNextChapter();
            }
        }
        
        public void RestartStory()
        {
            if (currentScene != null)
            {
                currentScene.RestartScene();
            }
        }
        #endregion
    }
} 