using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TheFeelies.Core
{
    /// <summary>
    /// 챕터를 관리하는 스크립트
    /// 액트들을 순차적으로 재생하고 챕터 흐름을 제어
    /// </summary>
    public class Chapter : MonoBehaviour
    {
        [Header("Chapter Settings")]
        [SerializeField] private string chapterName;
        [SerializeField] private string description;
        [SerializeField] private CharacterType characterType;
        
        [Header("Acts")]
        [SerializeField] private List<Act> acts = new List<Act>();
        
        [Header("Events")]
        [SerializeField] private UnityEvent onChapterStart;
        [SerializeField] private UnityEvent onChapterComplete;
        
        private int currentActIndex = 0;
        private bool isPlaying = false;
        
        public string ChapterName => chapterName;
        public CharacterType CharacterType => characterType;
        public bool IsPlaying => isPlaying;
        public int CurrentActIndex => currentActIndex;
        public Act CurrentAct => currentActIndex < acts.Count ? acts[currentActIndex] : null;
        public int ActCount => acts.Count;
        
        private void Start()
        {
            // 자동 시작은 하지 않음 - Scene에서 제어
        }
        
        /// <summary>
        /// 챕터 재생 시작
        /// </summary>
        public void StartChapter()
        {
            if (isPlaying)
            {
                Debug.LogWarning($"Chapter {chapterName} is already playing!");
                return;
            }
            
            if (acts.Count == 0)
            {
                Debug.LogError($"No acts available in chapter: {chapterName}!");
                return;
            }
            
            isPlaying = true;
            currentActIndex = 0;
            
            Debug.Log($"[Chapter] Starting: {chapterName}");
            onChapterStart?.Invoke();
            
            StartCoroutine(PlayChapterCoroutine());
        }
        
        /// <summary>
        /// 챕터 재생 중지
        /// </summary>
        public void StopChapter()
        {
            if (!isPlaying) return;
            
            isPlaying = false;
            StopAllCoroutines();
            
            // 현재 액트도 중지
            if (CurrentAct != null)
            {
                CurrentAct.StopAct();
            }
            
            Debug.Log($"[Chapter] Stopping: {chapterName}");
        }
        
        /// <summary>
        /// 챕터 재시작
        /// </summary>
        public void RestartChapter()
        {
            StopChapter();
            StartChapter();
        }
        
        /// <summary>
        /// 특정 액트부터 시작
        /// </summary>
        public void StartFromAct(int actIndex)
        {
            if (actIndex < 0 || actIndex >= acts.Count)
            {
                Debug.LogError($"Invalid act index: {actIndex} in chapter: {chapterName}");
                return;
            }
            
            StopChapter();
            currentActIndex = actIndex;
            StartChapter();
        }
        
        private IEnumerator PlayChapterCoroutine()
        {
            while (isPlaying && currentActIndex < acts.Count)
            {
                var currentAct = acts[currentActIndex];
                
                if (currentAct == null)
                {
                    Debug.LogError($"Act at index {currentActIndex} is null in chapter: {chapterName}!");
                    currentActIndex++;
                    continue;
                }
                
                Debug.Log($"[Chapter] Starting Act {currentActIndex + 1}: {currentAct.ActName}");
                
                // 액트 시작
                currentAct.StartAct();
                
                // 액트가 완료될 때까지 대기
                yield return new WaitUntil(() => !currentAct.IsPlaying);
                
                currentActIndex++;
            }
            
            // 모든 액트 완료
            if (isPlaying)
            {
                Debug.Log($"[Chapter] Completed: {chapterName}");
                isPlaying = false;
                onChapterComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// 다음 액트로 건너뛰기
        /// </summary>
        public void SkipToNextAct()
        {
            if (currentActIndex < acts.Count - 1)
            {
                if (CurrentAct != null)
                {
                    CurrentAct.StopAct();
                }
                currentActIndex++;
                Debug.Log($"Skipped to Act {currentActIndex + 1} in chapter: {chapterName}");
            }
        }
        
        /// <summary>
        /// 액트 추가 (에디터용)
        /// </summary>
        public void AddAct(Act act)
        {
            if (act != null && !acts.Contains(act))
            {
                acts.Add(act);
            }
        }
        
        /// <summary>
        /// 액트 제거 (에디터용)
        /// </summary>
        public void RemoveAct(Act act)
        {
            if (acts.Contains(act))
            {
                acts.Remove(act);
            }
        }
        
        /// <summary>
        /// 특정 액트 인덱스로 이동
        /// </summary>
        public void GoToAct(int actIndex)
        {
            if (actIndex >= 0 && actIndex < acts.Count)
            {
                if (CurrentAct != null)
                {
                    CurrentAct.StopAct();
                }
                currentActIndex = actIndex;
                Debug.Log($"Moved to Act {actIndex + 1} in chapter: {chapterName}");
            }
        }
        
        private void OnValidate()
        {
            // 에디터에서 액트 리스트 검증
            for (int i = acts.Count - 1; i >= 0; i--)
            {
                if (acts[i] == null)
                {
                    acts.RemoveAt(i);
                }
            }
        }
    }
}
