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
    [SerializeField] private bool waitBeforeStart = false;
    [SerializeField] private bool waitBeforeEnd = false;
        
        [Header("Events")]
        [SerializeField] private List<CutEvent> startEvents = new List<CutEvent>();
        [SerializeField] private List<CutEvent> middleEvents = new List<CutEvent>();
        [SerializeField] private List<CutEvent> endEvents = new List<CutEvent>();
        
    private bool isPlaying = false;
    private bool isWaitingBeforeStart = false;
    private bool isWaitingBeforeEnd = false;
    private bool skipRequested = false; // 스킵 요청 플래그

    public string CutName => cutName;
    public float CutDuration => cutDuration;
    public bool WaitBeforeStart => waitBeforeStart;
    public bool WaitBeforeEnd => waitBeforeEnd;
    public bool IsPlaying => isPlaying;
    public bool IsWaitingBeforeStart => isWaitingBeforeStart;
    public bool IsWaitingBeforeEnd => isWaitingBeforeEnd;
    public CutEvent CurrentEvent { get; private set; } // 현재 실행 중인 이벤트

        private void Start()
        {
            // 자동 시작은 하지 않음 - Act에서 제어
        }
        
    /// <summary>
    /// 컷 재생 시작 (내부용)
    /// </summary>
    private void StartCut()
    {
        if (isPlaying)
        {
            Debug.LogWarning($"Cut {cutName} is already playing!");
            return;
        }
        
        isPlaying = true;
        // isWaitingBeforeStart는 OnPlayerInputBeforeStart에서 설정되므로 여기서 초기화하지 않음
        // isWaitingBeforeEnd는 여기서 초기화
        isWaitingBeforeEnd = false;
        
        Debug.Log($"[Cut] Starting: {cutName}");
        
        StartCoroutine(PlayCutCoroutine());
    }
        
    /// <summary>
    /// 컷 재생 중지
    /// </summary>
    public void StopCut()
    {
        if (!isPlaying) return;
        
        isPlaying = false;
        isWaitingBeforeStart = false;
        isWaitingBeforeEnd = false;
        StopAllCoroutines();
        
        Debug.Log($"[Cut] Stopping: {cutName}");
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
    /// 컷 시작 요청 (이벤트 또는 트리거에서 호출)
    /// waitBeforeStart가 true면 대기 상태로 진입, false면 즉시 시작
    /// </summary>
    public void OnPlayerInputBeforeStart()
    {
        // 이미 재생 중이고 대기 상태가 아니면 무시
        if (isPlaying && !isWaitingBeforeStart)
        {
            Debug.LogWarning($"Cut {cutName} is already playing!");
            return;
        }
        
        // waitBeforeStart가 true이고 이미 대기 중이면 대기 해제
        if (waitBeforeStart && isWaitingBeforeStart)
        {
            isWaitingBeforeStart = false;
            return;
        }
        
        // 이미 재생 중이 아닐 때만 새로 시작
        if (!isPlaying)
        {
            // waitBeforeStart가 true면 대기 상태로 시작
            if (waitBeforeStart)
            {
                isWaitingBeforeStart = true;
            }
            else
            {
                isWaitingBeforeStart = false;
            }
            
            // 컷 시작
            StartCut();
        }
    }
    
    /// <summary>
    /// 종료 전 플레이어 입력 대기 중일 때 호출
    /// </summary>
    public void OnPlayerInputBeforeEnd()
    {
        if (isWaitingBeforeEnd)
        {
            isWaitingBeforeEnd = false;
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

    /// <summary>
    /// 현재 진행 중인 대기(이벤트 딜레이 등)를 건너뛰기
    /// </summary>
    public void SkipCurrentWait()
    {
        if (isPlaying)
        {
            // 대기 상태라면 입력 처리
            if (isWaitingBeforeStart)
            {
                OnPlayerInputBeforeStart();
                return;
            }
            if (isWaitingBeforeEnd)
            {
                OnPlayerInputBeforeEnd();
                return;
            }

            // 이벤트 실행 중 대기(딜레이)라면 스킵 플래그 설정
            skipRequested = true;
            Debug.Log($"[Cut] Skip requested: {cutName}");
        }
    }
        
    private IEnumerator PlayCutCoroutine()
    {
        // 1. 시작 전 플레이어 입력 대기
        if (waitBeforeStart && isWaitingBeforeStart)
        {
            yield return new WaitUntil(() => !isWaitingBeforeStart);
        }
            
            // 2. 시작 이벤트 실행
            yield return StartCoroutine(ExecuteEvents(startEvents));
            
            // 3. 중간 이벤트 실행
            if (middleEvents.Count > 0)
            {
                yield return StartCoroutine(ExecuteEvents(middleEvents));
            }
            
            // 4. 지속 시간 대기
            if (cutDuration > 0)
            {
                yield return StartCoroutine(WaitForSecondsOrSkip(cutDuration));
            }
            
        // 5. 종료 전 플레이어 입력 대기
        if (waitBeforeEnd)
        {
            isWaitingBeforeEnd = true;
            yield return new WaitUntil(() => !isWaitingBeforeEnd);
        }
            
            // 6. 종료 이벤트 실행
            yield return StartCoroutine(ExecuteEvents(endEvents));
            
            // 7. 컷 완료
            isPlaying = false;
        }
        
        private IEnumerator ExecuteEvents(List<CutEvent> events)
        {
            foreach (var cutEvent in events)
            {
                if (cutEvent == null) 
                {
                    continue;
                }
                
                CurrentEvent = cutEvent; // 현재 이벤트 업데이트
                
                // 지연 시간 대기
                if (cutEvent.Delay > 0)
                {
                    yield return StartCoroutine(WaitForSecondsOrSkip(cutEvent.Delay));
                }
                
                // 이벤트 실행
                cutEvent.ExecuteEvent();
                
                // 이벤트 간 추가 대기 시간 (필요한 경우)
                if (cutEvent.WaitAfterExecution > 0)
                {
                    yield return StartCoroutine(WaitForSecondsOrSkip(cutEvent.WaitAfterExecution));
                }
            }
            
            CurrentEvent = null; // 이벤트 목록 종료 시 초기화
        }

        /// <summary>
        /// 지정된 시간 동안 대기하거나 스킵 요청이 있으면 즉시 종료
        /// </summary>
        private IEnumerator WaitForSecondsOrSkip(float duration)
        {
            float timer = 0f;
            skipRequested = false;

            while (timer < duration)
            {
                if (skipRequested)
                {
                    skipRequested = false;
                    Debug.Log("[Cut] Wait skipped!");
                    yield break;
                }

                timer += Time.deltaTime;
                yield return null;
            }
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
