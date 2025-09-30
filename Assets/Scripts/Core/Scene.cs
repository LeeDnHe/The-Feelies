using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TheFeelies.Core
{
    /// <summary>
    /// 씬을 관리하는 최상위 스크립트
    /// 챕터들을 순차적으로 재생하고 전체 스토리 흐름을 제어
    /// </summary>
    public class Scene : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string sceneName;
        [SerializeField] private string description;
        
        [Header("Chapters")]
        [SerializeField] private List<Chapter> chapters = new List<Chapter>();
        
        [Header("Events")]
        [SerializeField] private UnityEvent onSceneStart;
        [SerializeField] private UnityEvent onSceneComplete;
        
        private int currentChapterIndex = 0;
        private bool isPlaying = false;
        
        public string SceneName => sceneName;
        public bool IsPlaying => isPlaying;
        public int CurrentChapterIndex => currentChapterIndex;
        public Chapter CurrentChapter => currentChapterIndex < chapters.Count ? chapters[currentChapterIndex] : null;
        
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
            
            if (chapters.Count == 0)
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
            if (chapterIndex < 0 || chapterIndex >= chapters.Count)
            {
                Debug.LogError($"Invalid chapter index: {chapterIndex}");
                return;
            }
            
            StopScene();
            currentChapterIndex = chapterIndex;
            StartScene();
        }
        
        private IEnumerator PlaySceneCoroutine()
        {
            while (isPlaying && currentChapterIndex < chapters.Count)
            {
                var currentChapter = chapters[currentChapterIndex];
                
                if (currentChapter == null)
                {
                    Debug.LogError($"Chapter at index {currentChapterIndex} is null!");
                    currentChapterIndex++;
                    continue;
                }
                
                Debug.Log($"Starting Chapter {currentChapterIndex + 1}: {currentChapter.ChapterName}");
                
                // 챕터 시작
                currentChapter.StartChapter();
                
                // 챕터가 완료될 때까지 대기
                yield return new WaitUntil(() => !currentChapter.IsPlaying);
                
                Debug.Log($"Chapter {currentChapterIndex + 1} completed");
                
                currentChapterIndex++;
            }
            
            // 모든 챕터 완료
            if (isPlaying)
            {
                Debug.Log($"Scene completed: {sceneName}");
                isPlaying = false;
                onSceneComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// 다음 챕터로 건너뛰기
        /// </summary>
        public void SkipToNextChapter()
        {
            if (currentChapterIndex < chapters.Count - 1)
            {
                if (CurrentChapter != null)
                {
                    CurrentChapter.StopChapter();
                }
                currentChapterIndex++;
                Debug.Log($"Skipped to Chapter {currentChapterIndex + 1}");
            }
        }
        
        /// <summary>
        /// 챕터 추가 (에디터용)
        /// </summary>
        public void AddChapter(Chapter chapter)
        {
            if (chapter != null && !chapters.Contains(chapter))
            {
                chapters.Add(chapter);
            }
        }
        
        /// <summary>
        /// 챕터 제거 (에디터용)
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
            // 에디터에서 챕터 리스트 검증
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
