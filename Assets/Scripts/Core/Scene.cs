using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace TheFeelies.Core
{
    /// <summary>
    /// 씬을 관리하는 최상위 스크립트
    /// 챕터들을 순차적으로 재생하고 전체 스토리 흐름을 제어
    /// 멀티 씬 방식으로 챕터별 배경 씬을 Additive 로드
    /// </summary>
    public class Scene : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string sceneName;
        [SerializeField] private string description;
        [SerializeField] private bool useMultiSceneLoading = true;
        
        [Header("Chapters")]
        [Tooltip("멀티 씬 모드: 챕터 씬 이름 리스트 / 단일 씬 모드: Chapter 컴포넌트 직접 참조")]
        [SerializeField] private List<string> chapterSceneNames = new List<string>();
        [SerializeField] private List<Chapter> chapters = new List<Chapter>(); // 단일 씬 모드용
        
        [Header("Fade Settings")]
        [Tooltip("챕터 전환 시 페이드 아웃 지속 시간")]
        [SerializeField] private float fadeOutDuration = 1.5f;
        [Tooltip("챕터 전환 시 페이드 인 지속 시간")]
        [SerializeField] private float fadeInDuration = 1.5f;
        [Tooltip("챕터 전환 시 페이드 효과 사용 여부")]
        [SerializeField] private bool useChapterFade = true;
        
        [Header("Events")]
        [SerializeField] private UnityEvent onSceneStart;
        [SerializeField] private UnityEvent onSceneComplete;
        
        // 페이드 이벤트 델리게이트
        public delegate void FadeDelegate(float duration);
        public static event FadeDelegate FadeIn;
        public static event FadeDelegate FadeOut;
        
        private int currentChapterIndex = 0;
        private bool isPlaying = false;
        private UnityEngine.SceneManagement.Scene currentLoadedChapterScene;
        private Chapter currentChapter; // 현재 활성 챕터 (멀티 씬 모드에서 씬 로드 후 찾아서 저장)
        
        public string SceneName => sceneName;
        public bool IsPlaying => isPlaying;
        public int CurrentChapterIndex => currentChapterIndex;
        public Chapter CurrentChapter => currentChapter;
        public int TotalChapters => useMultiSceneLoading ? chapterSceneNames.Count : chapters.Count;
        
        private void Start()
        {
            // 자동 시작이 필요한 경우 여기서 시작
            StartScene();
        }
        
        /// <summary>
        /// 씬 재생 시작
        /// </summary>
        public void StartScene()
        {
            if (isPlaying)
            {
                Debug.LogWarning("Scene is already playing!");
                return;
            }
            
            int chapterCount = useMultiSceneLoading ? chapterSceneNames.Count : chapters.Count;
            if (chapterCount == 0)
            {
                Debug.LogError("No chapters available in scene!");
                return;
            }
            
            isPlaying = true;
            currentChapterIndex = 0;
            
            Debug.Log($"Starting Scene: {sceneName}");
            onSceneStart?.Invoke();
            
            StartCoroutine(PlaySceneCoroutine());
        }
        
        /// <summary>
        /// 씬 재생 중지
        /// </summary>
        public void StopScene()
        {
            if (!isPlaying) return;
            
            isPlaying = false;
            StopAllCoroutines();
            
            Debug.Log($"Stopping Scene: {sceneName}");
        }
        
        /// <summary>
        /// 씬 재시작
        /// </summary>
        public void RestartScene()
        {
            StopScene();
            StartScene();
        }
        
        /// <summary>
        /// 특정 챕터부터 시작
        /// </summary>
        public void StartFromChapter(int chapterIndex)
        {
            int totalChapters = useMultiSceneLoading ? chapterSceneNames.Count : chapters.Count;
            if (chapterIndex < 0 || chapterIndex >= totalChapters)
            {
                Debug.LogError($"Invalid chapter index: {chapterIndex}");
                return;
            }
            
            StopScene();
            currentChapterIndex = chapterIndex;
            isPlaying = true;
            StartCoroutine(PlaySceneCoroutine());
        }
        
        private IEnumerator PlaySceneCoroutine()
        {
            int totalChapters = useMultiSceneLoading ? chapterSceneNames.Count : chapters.Count;
            
            // 첫 번째 챕터는 페이드 인만 수행
            bool isFirstChapter = true;
            
            while (isPlaying && currentChapterIndex < totalChapters)
            {
                // 챕터 전환 시 페이드 아웃 및 플레이어 위치 조정
                if (useChapterFade && !isFirstChapter)
                {
                    FadeOut?.Invoke(fadeOutDuration);
                    yield return new WaitForSeconds(fadeOutDuration);
                }
                
                // 모든 챕터에서 플레이어 위치 조정
                if (Managers.PlayerManager.Instance != null)
                {
                    Managers.PlayerManager.Instance.TeleportPlayerToChapterStart(currentChapterIndex);
                }
                
                // 멀티 씬 모드: 씬 로드 후 Chapter 컴포넌트 찾기
                if (useMultiSceneLoading)
                {
                    string chapterSceneName = chapterSceneNames[currentChapterIndex];
                    
                    if (string.IsNullOrEmpty(chapterSceneName))
                    {
                        Debug.LogError($"Chapter scene name at index {currentChapterIndex} is empty!");
                        currentChapterIndex++;
                        continue;
                    }
                    
                    // 챕터 씬 로드
                    yield return StartCoroutine(LoadChapterScene(chapterSceneName));
                    
                    // 로드된 씬에서 Chapter 컴포넌트 찾기
                    currentChapter = FindChapterInScene(currentLoadedChapterScene);
                    
                    if (currentChapter == null)
                    {
                        Debug.LogError($"No Chapter component found in scene: {chapterSceneName}");
                        yield return StartCoroutine(UnloadChapterScene());
                        currentChapterIndex++;
                        continue;
                    }
                }
                // 단일 씬 모드: 직접 참조된 Chapter 사용
                else
                {
                    currentChapter = chapters[currentChapterIndex];
                    
                    if (currentChapter == null)
                    {
                        Debug.LogError($"Chapter at index {currentChapterIndex} is null!");
                        currentChapterIndex++;
                        continue;
                    }
                }
                
                Debug.Log($"[Scene] Starting Chapter {currentChapterIndex + 1}: {currentChapter.ChapterName}");
                
                // 페이드 인 (챕터 시작 시)
                if (useChapterFade)
                {
                    FadeIn?.Invoke(fadeInDuration);
                    yield return new WaitForSeconds(fadeInDuration);
                }
                
                // 챕터 시작
                currentChapter.StartChapter();
                
                // 챕터가 완료될 때까지 대기
                yield return new WaitUntil(() => !currentChapter.IsPlaying);
                
                // 멀티 씬 모드: 챕터 씬 언로드
                if (useMultiSceneLoading && currentLoadedChapterScene.IsValid())
                {
                    yield return StartCoroutine(UnloadChapterScene());
                    currentChapter = null;
                }
                
                currentChapterIndex++;
                isFirstChapter = false;
            }
            
            // 모든 챕터 완료
            if (isPlaying)
            {
                Debug.Log($"[Scene] Completed: {sceneName}");
                isPlaying = false;
                onSceneComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// 로드된 씬에서 Chapter 컴포넌트 찾기
        /// </summary>
        private Chapter FindChapterInScene(UnityEngine.SceneManagement.Scene scene)
        {
            if (!scene.IsValid())
                return null;
            
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (var rootObject in rootObjects)
            {
                Chapter chapter = rootObject.GetComponentInChildren<Chapter>();
                if (chapter != null)
                {
                    return chapter;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 챕터 씬을 Additive 방식으로 로드
        /// </summary>
        private IEnumerator LoadChapterScene(string chapterSceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(chapterSceneName, LoadSceneMode.Additive);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            currentLoadedChapterScene = SceneManager.GetSceneByName(chapterSceneName);
            
            if (currentLoadedChapterScene.IsValid())
            {
                SceneManager.SetActiveScene(currentLoadedChapterScene);
            }
            else
            {
                Debug.LogError($"Failed to load chapter scene: {chapterSceneName}");
            }
        }
        
        /// <summary>
        /// 현재 로드된 챕터 씬을 언로드
        /// </summary>
        private IEnumerator UnloadChapterScene()
        {
            if (!currentLoadedChapterScene.IsValid())
            {
                yield break;
            }
            
            string sceneName = currentLoadedChapterScene.name;
            
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(currentLoadedChapterScene);
            
            while (!asyncUnload.isDone)
            {
                yield return null;
            }
            
            // 가비지 컬렉션으로 메모리 정리
            yield return Resources.UnloadUnusedAssets();
        }
        
        /// <summary>
        /// 다음 챕터로 건너뛰기
        /// </summary>
        public void SkipToNextChapter()
        {
            int totalChapters = useMultiSceneLoading ? chapterSceneNames.Count : chapters.Count;
            if (currentChapterIndex < totalChapters - 1)
            {
                if (currentChapter != null)
                {
                    currentChapter.StopChapter();
                }
                Debug.Log($"Skipped to Chapter {currentChapterIndex + 1}");
            }
        }
        
        /// <summary>
        /// 챕터 씬 이름 추가 (에디터용 - 멀티 씬 모드)
        /// </summary>
        public void AddChapterScene(string chapterSceneName)
        {
            if (!string.IsNullOrEmpty(chapterSceneName) && !chapterSceneNames.Contains(chapterSceneName))
            {
                chapterSceneNames.Add(chapterSceneName);
            }
        }
        
        /// <summary>
        /// 챕터 씬 이름 제거 (에디터용 - 멀티 씬 모드)
        /// </summary>
        public void RemoveChapterScene(string chapterSceneName)
        {
            if (chapterSceneNames.Contains(chapterSceneName))
            {
                chapterSceneNames.Remove(chapterSceneName);
            }
        }
        
        /// <summary>
        /// 챕터 추가 (에디터용 - 단일 씬 모드)
        /// </summary>
        public void AddChapter(Chapter chapter)
        {
            if (chapter != null && !chapters.Contains(chapter))
            {
                chapters.Add(chapter);
            }
        }
        
        /// <summary>
        /// 챕터 제거 (에디터용 - 단일 씬 모드)
        /// </summary>
        public void RemoveChapter(Chapter chapter)
        {
            if (chapters.Contains(chapter))
            {
                chapters.Remove(chapter);
            }
        }
        
        private void OnValidate()
        {
            // 멀티 씬 모드: 씬 이름 리스트 검증
            if (useMultiSceneLoading)
            {
                for (int i = chapterSceneNames.Count - 1; i >= 0; i--)
                {
                    if (string.IsNullOrEmpty(chapterSceneNames[i]))
                    {
                        chapterSceneNames.RemoveAt(i);
                    }
                }
            }
            // 단일 씬 모드: 챕터 컴포넌트 리스트 검증
            else
            {
                for (int i = chapters.Count - 1; i >= 0; i--)
                {
                    if (chapters[i] == null)
                    {
                        chapters.RemoveAt(i);
                    }
                }
            }
        }
    }
}
