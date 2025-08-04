using UnityEngine;
using TheFeelies.Core;
using System.Collections;

namespace TheFeelies.Managers
{
    public class SceneManager : MonoBehaviour
    {
        [Header("Story Settings")]
        [SerializeField] private StoryDataAsset currentStoryAsset;
        [SerializeField] private CharacterType selectedCharacter = CharacterType.Sherlock;
        
        [Header("Managers")]
        [SerializeField] private SoundManager soundManager;
        [SerializeField] private AnimationManager animationManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private VFXManager vfxManager;
        [SerializeField] private PlayerManager playerManager;
        
        [Header("Story State")]
        [SerializeField] private int currentChapterIndex = 0;
        [SerializeField] private int currentActIndex = 0;
        [SerializeField] private int currentCutIndex = 0;
        
        private StoryData currentStoryData;
        private Chapter currentChapter;
        private Act currentAct;
        private Cut currentCut;
        
        private bool isStoryPlaying = false;
        private Coroutine storyCoroutine;
        
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
            LoadStory();
            LoadSavePoint();
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
            if (currentStoryAsset == null)
            {
                Debug.LogError("Story Asset이 설정되지 않았습니다!");
                return;
            }
            
            currentStoryData = currentStoryAsset.storyData;
            Debug.Log($"스토리 로드됨: {currentStoryData.storyName}");
        }
        
        public void StartStory()
        {
            if (isStoryPlaying)
            {
                Debug.LogWarning("스토리가 이미 재생 중입니다.");
                return;
            }
            
            isStoryPlaying = true;
            storyCoroutine = StartCoroutine(PlayStoryCoroutine());
        }
        
        public void StopStory()
        {
            if (storyCoroutine != null)
            {
                StopCoroutine(storyCoroutine);
                storyCoroutine = null;
            }
            isStoryPlaying = false;
        }
        
        public void LoadSavePoint()
        {
            // PlayerPrefs에서 세이브 포인트 로드
            string storyID = PlayerPrefs.GetString("CurrentStory", "");
            int chapterIndex = PlayerPrefs.GetInt("CurrentChapter", 0);
            int actIndex = PlayerPrefs.GetInt("CurrentAct", 0);
            
            if (!string.IsNullOrEmpty(storyID) && storyID == currentStoryData.storyID)
            {
                currentChapterIndex = chapterIndex;
                currentActIndex = actIndex;
                currentCutIndex = 0;
                
                Debug.Log($"세이브 포인트 로드됨: Chapter {currentChapterIndex}, Act {currentActIndex}");
            }
        }
        
        public void SaveCurrentPoint()
        {
            if (currentStoryData != null)
            {
                PlayerPrefs.SetString("CurrentStory", currentStoryData.storyID);
                PlayerPrefs.SetInt("CurrentChapter", currentChapterIndex);
                PlayerPrefs.SetInt("CurrentAct", currentActIndex);
                PlayerPrefs.Save();
                
                Debug.Log($"세이브 포인트 저장됨: Chapter {currentChapterIndex}, Act {currentActIndex}");
            }
        }
        
        private IEnumerator PlayStoryCoroutine()
        {
            while (isStoryPlaying)
            {
                // 현재 컷 실행
                if (currentCut != null)
                {
                    yield return StartCoroutine(ExecuteCut(currentCut));
                }
                
                // 다음 컷으로 이동
                if (!MoveToNextCut())
                {
                    // 스토리 종료
                    Debug.Log("스토리 종료");
                    isStoryPlaying = false;
                    break;
                }
            }
        }
        
        private IEnumerator ExecuteCut(Cut cut)
        {
            Debug.Log($"컷 실행: {cut.cutName}");
            
            // 플레이어 위치 설정
            HandlePlayerPosition(cut);
            
            // UI 업데이트
            uiManager?.UpdateDialogue(cut.dialogueText);
            
            // TTS 재생
            if (cut.ttsAudio != null)
            {
                soundManager?.PlayTTS(cut.ttsAudio);
            }
            
            // 캐릭터 애니메이션 실행
            if (cut.characterAnimations.Count > 0)
            {
                animationManager?.PlayCharacterAnimations(cut.characterAnimations, cut.characterNames);
            }
            
            // 컷 조건에 따른 대기
            yield return StartCoroutine(WaitForCutCondition(cut));
        }
        
        private void HandlePlayerPosition(Cut cut)
        {
            switch (cut.playerPositionType)
            {
                case PlayerPositionType.MaintainPosition:
                    // 현재 위치 유지
                    break;
                    
                case PlayerPositionType.TeleportToSpawnPoint:
                    if (cut.spawnPoint != null)
                    {
                        playerManager?.TeleportPlayer(cut.spawnPoint.position, cut.spawnPoint.rotation);
                    }
                    break;
                    
                case PlayerPositionType.TeleportToCustom:
                    playerManager?.TeleportPlayer(cut.customPosition, cut.customRotation);
                    break;
            }
        }
        
        private IEnumerator WaitForCutCondition(Cut cut)
        {
            switch (cut.startCondition)
            {
                case CutStartCondition.Automatic:
                    yield return new WaitForSeconds(cut.cutDuration);
                    break;
                    
                case CutStartCondition.PlayerInput:
                    if (cut.waitForPlayerInput)
                    {
                        yield return StartCoroutine(WaitForPlayerInput(cut.requiredButtonText));
                    }
                    else
                    {
                        yield return new WaitForSeconds(cut.cutDuration);
                    }
                    break;
                    
                case CutStartCondition.Timer:
                    yield return new WaitForSeconds(cut.cutDuration);
                    break;
            }
        }
        
        private IEnumerator WaitForPlayerInput(string buttonText)
        {
            bool inputReceived = false;
            
            // UI에서 버튼 표시
            uiManager?.ShowContinueButton(buttonText, () => inputReceived = true);
            
            // 입력 대기
            while (!inputReceived)
            {
                yield return null;
            }
            
            // UI에서 버튼 숨김
            uiManager?.HideContinueButton();
        }
        
        private bool MoveToNextCut()
        {
            if (currentStoryData == null || currentStoryData.chapters.Count == 0)
                return false;
            
            // 현재 챕터와 액트 업데이트
            currentChapter = currentStoryData.chapters[currentChapterIndex];
            currentAct = currentChapter.acts[currentActIndex];
            
            // 다음 컷으로 이동
            currentCutIndex++;
            
            // 액트의 모든 컷을 다 재생했는지 확인
            if (currentCutIndex >= currentAct.cuts.Count)
            {
                // 다음 액트로 이동
                currentActIndex++;
                currentCutIndex = 0;
                
                // 챕터의 모든 액트를 다 재생했는지 확인
                if (currentActIndex >= currentChapter.acts.Count)
                {
                    // 다음 챕터로 이동
                    currentChapterIndex++;
                    currentActIndex = 0;
                    currentCutIndex = 0;
                    
                    // 모든 챕터를 다 재생했는지 확인
                    if (currentChapterIndex >= currentStoryData.chapters.Count)
                    {
                        return false; // 스토리 종료
                    }
                }
                
                // 세이브 포인트인지 확인
                if (currentAct.isSavePoint)
                {
                    SaveCurrentPoint();
                }
            }
            
            // 현재 컷 업데이트
            currentCut = currentAct.cuts[currentCutIndex];
            return true;
        }
        
        #region Public Methods
        public void SetStoryAsset(StoryDataAsset storyAsset)
        {
            currentStoryAsset = storyAsset;
            LoadStory();
        }
        
        public void SetCharacter(CharacterType character)
        {
            selectedCharacter = character;
        }
        
        public void JumpToChapter(int chapterIndex)
        {
            if (chapterIndex >= 0 && chapterIndex < currentStoryData.chapters.Count)
            {
                currentChapterIndex = chapterIndex;
                currentActIndex = 0;
                currentCutIndex = 0;
                SaveCurrentPoint();
            }
        }
        
        public void JumpToAct(int actIndex)
        {
            if (actIndex >= 0 && actIndex < currentChapter.acts.Count)
            {
                currentActIndex = actIndex;
                currentCutIndex = 0;
                SaveCurrentPoint();
            }
        }
        #endregion
    }
} 