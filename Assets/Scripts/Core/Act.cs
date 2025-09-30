using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TheFeelies.Core
{
    /// <summary>
    /// 액트를 관리하는 스크립트
    /// 컷들을 순차적으로 재생하고 액트 흐름을 제어
    /// </summary>
    public class Act : MonoBehaviour
    {
        [Header("Act Settings")]
        [SerializeField] private string actName;
        [SerializeField] private string description;
        [SerializeField] private bool isSavePoint = true;
        [SerializeField] private string backgroundScene;
        
        [Header("Cuts")]
        [SerializeField] private List<Cut> cuts = new List<Cut>();
        
        [Header("Events")]
        [SerializeField] private UnityEvent onActStart;
        [SerializeField] private UnityEvent onActComplete;
        
        private int currentCutIndex = 0;
        private bool isPlaying = false;
        
        public string ActName => actName;
        public bool IsSavePoint => isSavePoint;
        public string BackgroundScene => backgroundScene;
        public bool IsPlaying => isPlaying;
        public int CurrentCutIndex => currentCutIndex;
        public Cut CurrentCut => currentCutIndex < cuts.Count ? cuts[currentCutIndex] : null;
        public int CutCount => cuts.Count;
        
        private void Start()
        {
            // 자동 시작은 하지 않음 - Chapter에서 제어
        }
        
        /// <summary>
        /// 액트 재생 시작
        /// </summary>
        public void StartAct()
        {
            if (isPlaying)
            {
                Debug.LogWarning($"Act {actName} is already playing!");
                return;
            }
            
            if (cuts.Count == 0)
            {
                Debug.LogError($"No cuts available in act: {actName}!");
                return;
            }
            
            isPlaying = true;
            currentCutIndex = 0;
            
            Debug.Log($"Starting Act: {actName}");
            onActStart?.Invoke();
            
            StartCoroutine(PlayActCoroutine());
        }
        
        /// <summary>
        /// 액트 재생 중지
        /// </summary>
        public void StopAct()
        {
            if (!isPlaying) return;
            
            isPlaying = false;
            StopAllCoroutines();
            
            // 현재 컷도 중지
            if (CurrentCut != null)
            {
                CurrentCut.StopCut();
            }
            
            Debug.Log($"Stopping Act: {actName}");
        }
        
        /// <summary>
        /// 액트 재시작
        /// </summary>
        public void RestartAct()
        {
            StopAct();
            StartAct();
        }
        
        /// <summary>
        /// 특정 컷부터 시작
        /// </summary>
        public void StartFromCut(int cutIndex)
        {
            if (cutIndex < 0 || cutIndex >= cuts.Count)
            {
                Debug.LogError($"Invalid cut index: {cutIndex} in act: {actName}");
                return;
            }
            
            StopAct();
            currentCutIndex = cutIndex;
            StartAct();
        }
        
        private IEnumerator PlayActCoroutine()
        {
            while (isPlaying && currentCutIndex < cuts.Count)
            {
                var currentCut = cuts[currentCutIndex];
                
                if (currentCut == null)
                {
                    Debug.LogError($"Cut at index {currentCutIndex} is null in act: {actName}!");
                    currentCutIndex++;
                    continue;
                }
                
                Debug.Log($"Starting Cut {currentCutIndex + 1}: {currentCut.CutName}");
                
                // 컷 시작
                currentCut.StartCut();
                
                // 컷이 완료될 때까지 대기
                yield return new WaitUntil(() => !currentCut.IsPlaying);
                
                Debug.Log($"Cut {currentCutIndex + 1} completed in act: {actName}");
                
                currentCutIndex++;
            }
            
            // 모든 컷 완료
            if (isPlaying)
            {
                Debug.Log($"Act completed: {actName}");
                isPlaying = false;
                onActComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// 다음 컷으로 건너뛰기
        /// </summary>
        public void SkipToNextCut()
        {
            if (currentCutIndex < cuts.Count - 1)
            {
                if (CurrentCut != null)
                {
                    CurrentCut.StopCut();
                }
                currentCutIndex++;
                Debug.Log($"Skipped to Cut {currentCutIndex + 1} in act: {actName}");
            }
        }
        
        /// <summary>
        /// 컷 추가 (에디터용)
        /// </summary>
        public void AddCut(Cut cut)
        {
            if (cut != null && !cuts.Contains(cut))
            {
                cuts.Add(cut);
            }
        }
        
        /// <summary>
        /// 컷 제거 (에디터용)
        /// </summary>
        public void RemoveCut(Cut cut)
        {
            if (cuts.Contains(cut))
            {
                cuts.Remove(cut);
            }
        }
        
        /// <summary>
        /// 특정 컷 인덱스로 이동
        /// </summary>
        public void GoToCut(int cutIndex)
        {
            if (cutIndex >= 0 && cutIndex < cuts.Count)
            {
                if (CurrentCut != null)
                {
                    CurrentCut.StopCut();
                }
                currentCutIndex = cutIndex;
                Debug.Log($"Moved to Cut {cutIndex + 1} in act: {actName}");
            }
        }
        
        /// <summary>
        /// 세이브 포인트인지 확인
        /// </summary>
        public bool IsAtSavePoint()
        {
            return isSavePoint;
        }
        
        private void OnValidate()
        {
            // 에디터에서 컷 리스트 검증
            for (int i = cuts.Count - 1; i >= 0; i--)
            {
                if (cuts[i] == null)
                {
                    cuts.RemoveAt(i);
                }
            }
        }
    }
}
