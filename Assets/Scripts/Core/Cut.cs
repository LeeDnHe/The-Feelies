using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TheFeelies.Core
{
    /// <summary>
    /// 컷을 관리하는 스크립트
    /// 시작/중간/종료 이벤트를 순차적으로 재생하고 컷 흐름을 제어
    /// </summary>
    public class Cut : MonoBehaviour
    {
        [Header("Cut Settings")]
        [SerializeField] private string cutName;
        [SerializeField] private string description;
        [SerializeField] private float cutDuration = 3f;
        [SerializeField] private bool waitForPlayerInputBeforeStart = false;
        [SerializeField] private bool waitForPlayerInputBeforeEnd = false;
        
        [Header("Events")]
        [SerializeField] private List<CutEvent> startEvents = new List<CutEvent>();
        [SerializeField] private List<CutEvent> middleEvents = new List<CutEvent>();
        [SerializeField] private List<CutEvent> endEvents = new List<CutEvent>();
        
        private bool isPlaying = false;
        private bool isWaitingForInputBeforeStart = false;
        private bool isWaitingForInputBeforeEnd = false;
        
        public string CutName => cutName;
        public float CutDuration => cutDuration;
        public bool WaitForPlayerInputBeforeStart => waitForPlayerInputBeforeStart;
        public bool WaitForPlayerInputBeforeEnd => waitForPlayerInputBeforeEnd;
        public bool IsPlaying => isPlaying;
        public bool IsWaitingForInputBeforeStart => isWaitingForInputBeforeStart;
        public bool IsWaitingForInputBeforeEnd => isWaitingForInputBeforeEnd;
        
        private void Start()
        {
            // 자동 시작은 하지 않음 - Act에서 제어
        }
        
        /// <summary>
        /// 컷 재생 시작
        /// </summary>
        public void StartCut()
        {
            if (isPlaying)
            {
                Debug.LogWarning($"Cut {cutName} is already playing!");
                return;
            }
            
            isPlaying = true;
            isWaitingForInputBeforeStart = false;
            isWaitingForInputBeforeEnd = false;
            
            Debug.Log($"Starting Cut: {cutName}");
            
            StartCoroutine(PlayCutCoroutine());
        }
        
        /// <summary>
        /// 컷 재생 중지
        /// </summary>
        public void StopCut()
        {
            if (!isPlaying) return;
            
            isPlaying = false;
            isWaitingForInputBeforeStart = false;
            isWaitingForInputBeforeEnd = false;
            StopAllCoroutines();
            
            Debug.Log($"Stopping Cut: {cutName}");
        }
        
        /// <summary>
        /// 컷 재시작
        /// </summary>
        public void RestartCut()
        {
            StopCut();
            StartCut();
        }
        
        /// <summary>
        /// 시작 전 플레이어 입력 대기 중일 때 호출
        /// </summary>
        public void OnPlayerInputBeforeStart()
        {
            if (isWaitingForInputBeforeStart)
            {
                isWaitingForInputBeforeStart = false;
                Debug.Log($"Player input before start received for cut: {cutName}");
            }
        }
        
        /// <summary>
        /// 종료 전 플레이어 입력 대기 중일 때 호출
        /// </summary>
        public void OnPlayerInputBeforeEnd()
        {
            if (isWaitingForInputBeforeEnd)
            {
                isWaitingForInputBeforeEnd = false;
                Debug.Log($"Player input before end received for cut: {cutName}");
            }
        }
        
        /// <summary>
        /// 플레이어 입력 대기 중일 때 호출 (하위 호환성을 위해 유지)
        /// </summary>
        public void OnPlayerInput()
        {
            OnPlayerInputBeforeStart();
            OnPlayerInputBeforeEnd();
        }
        
        private IEnumerator PlayCutCoroutine()
        {
            // 1. 시작 전 플레이어 입력 대기
            if (waitForPlayerInputBeforeStart)
            {
                isWaitingForInputBeforeStart = true;
                Debug.Log($"Waiting for player input before start in cut: {cutName}");
                yield return new WaitUntil(() => !isWaitingForInputBeforeStart);
            }
            
            // 2. 시작 이벤트 실행
            Debug.Log($"Executing start events for cut: {cutName}");
            yield return StartCoroutine(ExecuteEvents(startEvents));
            
            // 3. 중간 이벤트 실행
            if (middleEvents.Count > 0)
            {
                Debug.Log($"Executing middle events for cut: {cutName}");
                yield return StartCoroutine(ExecuteEvents(middleEvents));
            }
            
            // 4. 지속 시간 대기
            if (cutDuration > 0)
            {
                Debug.Log($"Waiting {cutDuration} seconds in cut: {cutName}");
                yield return new WaitForSeconds(cutDuration);
            }
            
            // 5. 종료 전 플레이어 입력 대기
            if (waitForPlayerInputBeforeEnd)
            {
                isWaitingForInputBeforeEnd = true;
                Debug.Log($"Waiting for player input before end in cut: {cutName}");
                yield return new WaitUntil(() => !isWaitingForInputBeforeEnd);
            }
            
            // 6. 종료 이벤트 실행
            Debug.Log($"Executing end events for cut: {cutName}");
            yield return StartCoroutine(ExecuteEvents(endEvents));
            
            // 7. 컷 완료
            Debug.Log($"Cut completed: {cutName}");
            isPlaying = false;
        }
        
        private IEnumerator ExecuteEvents(List<CutEvent> events)
        {
            Debug.Log($"ExecuteEvents called with {events.Count} events");
            
            foreach (var cutEvent in events)
            {
                if (cutEvent == null) 
                {
                    Debug.LogWarning("Null cutEvent found in events list, skipping...");
                    continue;
                }
                
                Debug.Log($"Executing event: {cutEvent.EventName} (Type: {cutEvent.EventType}, Delay: {cutEvent.Delay})");
                
                // 지연 시간 대기
                if (cutEvent.Delay > 0)
                {
                    Debug.Log($"Waiting {cutEvent.Delay} seconds before executing event: {cutEvent.EventName}");
                    yield return new WaitForSeconds(cutEvent.Delay);
                }
                
                // 이벤트 실행
                Debug.Log($"About to execute event: {cutEvent.EventName}");
                cutEvent.ExecuteEvent();
                Debug.Log($"Event execution completed: {cutEvent.EventName}");
                
                // 이벤트 간 추가 대기 시간 (필요한 경우)
                if (cutEvent.WaitAfterExecution > 0)
                {
                    Debug.Log($"Waiting {cutEvent.WaitAfterExecution} seconds after executing event: {cutEvent.EventName}");
                    yield return new WaitForSeconds(cutEvent.WaitAfterExecution);
                }
            }
            
            Debug.Log("All events in ExecuteEvents completed");
        }
        
        /// <summary>
        /// 시작 이벤트 추가 (에디터용)
        /// </summary>
        public void AddStartEvent(CutEvent cutEvent)
        {
            if (cutEvent != null && !startEvents.Contains(cutEvent))
            {
                startEvents.Add(cutEvent);
            }
        }
        
        /// <summary>
        /// 중간 이벤트 추가 (에디터용)
        /// </summary>
        public void AddMiddleEvent(CutEvent cutEvent)
        {
            if (cutEvent != null && !middleEvents.Contains(cutEvent))
            {
                middleEvents.Add(cutEvent);
            }
        }
        
        /// <summary>
        /// 종료 이벤트 추가 (에디터용)
        /// </summary>
        public void AddEndEvent(CutEvent cutEvent)
        {
            if (cutEvent != null && !endEvents.Contains(cutEvent))
            {
                endEvents.Add(cutEvent);
            }
        }
        
        /// <summary>
        /// 이벤트 제거 (에디터용)
        /// </summary>
        public void RemoveEvent(CutEvent cutEvent)
        {
            startEvents.Remove(cutEvent);
            middleEvents.Remove(cutEvent);
            endEvents.Remove(cutEvent);
        }
        
        private void OnValidate()
        {
            // 에디터에서 이벤트 리스트 검증
            ValidateEventList(startEvents);
            ValidateEventList(middleEvents);
            ValidateEventList(endEvents);
        }
        
        private void ValidateEventList(List<CutEvent> eventList)
        {
            for (int i = eventList.Count - 1; i >= 0; i--)
            {
                if (eventList[i] == null)
                {
                    eventList.RemoveAt(i);
                }
            }
        }
    }
}
