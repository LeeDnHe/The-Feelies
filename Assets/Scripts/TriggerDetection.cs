using UnityEngine;
using UnityEngine.Events;
using TheFeelies.Managers;

public class TriggerDetection : MonoBehaviour
{
    [Header("Trigger Detection")]
    [SerializeField] private string[] allowedTriggerTags = { "GameController"};
    [SerializeField] private bool preventMultipleTriggers = true;
    [SerializeField] private float triggerCooldown = 0.5f;
    
    [Header("Contact Duration")]
    [SerializeField] private bool enableDurationTrigger = false;
    [SerializeField] private float requiredContactTime = 1f;
    
    [Header("Haptic Feedback")]
    [SerializeField] private bool enableHaptics = true;
    [SerializeField] private float hapticIntensity = 0.5f;
    [SerializeField] private float hapticDuration = 0.1f;
    [SerializeField] private bool continuousHaptics = false;
    [SerializeField] private float continuousHapticInterval = 0.5f;
    [SerializeField] private bool autoDetectHand = true; // 자동으로 접촉한 손 감지
    [Tooltip("자동 감지 비활성화 시 사용")]
    [SerializeField] private bool sendToLeftHand = true;
    [Tooltip("자동 감지 비활성화 시 사용")]
    [SerializeField] private bool sendToRightHand = true;
    
    [Header("Events")]
    [Tooltip("접촉 즉시 발동하는 이벤트")]
    [SerializeField] private UnityEvent onImmediateTrigger;
    [Tooltip("일정 시간 접촉 후 발동하는 이벤트")]
    [SerializeField] private UnityEvent onDurationTrigger;
    [Tooltip("접촉 시작 이벤트")]
    [SerializeField] private UnityEvent onContactStart;
    [Tooltip("접촉 종료 이벤트")]
    [SerializeField] private UnityEvent onContactEnd;
    
    private bool hasImmediateTriggered = false;
    private bool hasDurationTriggered = false;
    private float lastTriggerTime = 0f;
    private bool isInContact = false;
    private float contactStartTime = 0f;
    private float lastHapticTime = 0f;
    private Collider currentTriggerObject = null;
    private bool isLeftHand = false;
    private bool isRightHand = false;
    
    void Start()
    {
        // 콜라이더가 없으면 자동으로 추가
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"TriggerDetection on {gameObject.name} has no Collider. Adding BoxCollider as trigger.");
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }
        else
        {
            // 기존 콜라이더를 트리거로 설정
            GetComponent<Collider>().isTrigger = true;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // null 체크
        if (other == null) return;
        
        // 중복 트리거 방지 체크
        if (preventMultipleTriggers && hasImmediateTriggered && (!enableDurationTrigger || hasDurationTriggered)) 
            return;
        
        // 쿨다운 체크
        if (Time.time - lastTriggerTime < triggerCooldown) return;
        
        // 허용된 태그인지 확인
        bool isValidTrigger = IsValidTrigger(other);
        
        if (isValidTrigger)
        {
            Debug.Log($"TriggerDetection {gameObject.name} contact started with {other.name}");
            
            // 접촉 상태 시작
            isInContact = true;
            contactStartTime = Time.time;
            currentTriggerObject = other;
            
            // 자동 감지: 접촉한 손 판별
            if (autoDetectHand)
            {
                DetectHandFromCollider(other);
            }
            
            // 접촉 시작 이벤트
            onContactStart?.Invoke();
            
            // 햅틱 피드백 (접촉 시작)
            SendHapticFeedback();
            
            // 즉시 트리거 발동 (중복 방지 체크)
            if (!hasImmediateTriggered || !preventMultipleTriggers)
            {
                ActivateImmediateTrigger(other);
            }
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        // 접촉 중이 아니면 무시
        if (!isInContact || other != currentTriggerObject) return;
        
        // Duration 트리거가 활성화되어 있고, 아직 발동되지 않았다면
        if (enableDurationTrigger && !hasDurationTriggered)
        {
            float contactDuration = Time.time - contactStartTime;
            
            // 지속 햅틱 피드백
            if (continuousHaptics && Time.time - lastHapticTime >= continuousHapticInterval)
            {
                SendHapticFeedback();
            }
            
            // 필요한 접촉 시간에 도달했으면 발동
            if (contactDuration >= requiredContactTime)
            {
                ActivateDurationTrigger(other);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // 현재 접촉 중인 오브젝트가 나갔는지 확인
        if (other == currentTriggerObject)
        {
            Debug.Log($"TriggerDetection {gameObject.name} contact ended with {other.name}");
            
            // 접촉 상태 종료
            isInContact = false;
            currentTriggerObject = null;
            isLeftHand = false;
            isRightHand = false;
            
            // 접촉 종료 이벤트
            onContactEnd?.Invoke();
        }
    }
    
    private bool IsValidTrigger(Collider other)
    {
        foreach (string tag in allowedTriggerTags)
        {
            if (!string.IsNullOrEmpty(tag) && other.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }
    
    private void ActivateImmediateTrigger(Collider other)
    {
        Debug.Log($"[즉시 트리거] {gameObject.name} activated by {other.name}");
        
        // 트리거 상태 업데이트
        hasImmediateTriggered = true;
        lastTriggerTime = Time.time;
        
        // 강한 햅틱 피드백 (트리거 발동 시)
        SendHapticFeedback(hapticIntensity * 1.5f, hapticDuration * 1.5f);
        
        // 즉시 트리거 이벤트 호출
        onImmediateTrigger?.Invoke();
    }
    
    private void ActivateDurationTrigger(Collider other)
    {
        Debug.Log($"[시간 트리거] {gameObject.name} activated by {other.name} after {requiredContactTime}s");
        
        // 트리거 상태 업데이트
        hasDurationTriggered = true;
        lastTriggerTime = Time.time;
        
        // 매우 강한 햅틱 피드백 (시간 트리거 발동 시)
        SendHapticFeedback(hapticIntensity * 2f, hapticDuration * 2f);
        
        // 시간 트리거 이벤트 호출
        onDurationTrigger?.Invoke();
    }
    
    private void DetectHandFromCollider(Collider other)
    {
        // 초기화
        isLeftHand = false;
        isRightHand = false;
        
        // Hand 컴포넌트 찾기 (본인 오브젝트에서)
        Autohand.Hand hand = other.GetComponent<Autohand.Hand>();
        
        if (hand != null)
        {
            if (hand.left)
            {
                isLeftHand = true;
                Debug.Log($"감지: 왼손 접촉 (Hand Component: {hand.name})");
            }
            else
            {
                isRightHand = true;
                Debug.Log($"감지: 오른손 접촉 (Hand Component: {hand.name})");
            }
        }
        else
        {
            // Hand 컴포넌트를 찾지 못한 경우 경고
            Debug.LogWarning($"손 감지 실패: {other.name}. Hand 컴포넌트를 찾을 수 없습니다.");
        }
    }
    
    private void SendHapticFeedback(float intensity = -1f, float duration = -1f)
    {
        if (!enableHaptics || PlayerManager.Instance == null) return;
        
        // 기본값 사용
        if (intensity < 0) intensity = hapticIntensity;
        if (duration < 0) duration = hapticDuration;
        
        // PlayerManager를 통해 햅틱 전송
        if (autoDetectHand)
        {
            // 자동 감지 모드: 접촉한 손에만 전송
            if (isLeftHand)
            {
                PlayerManager.Instance.SendHapticToLeftHand(intensity, duration);
            }
            else if (isRightHand)
            {
                PlayerManager.Instance.SendHapticToRightHand(intensity, duration);
            }
        }
        else
        {
            // 수동 모드: 설정된 손에 전송
            if (sendToLeftHand && sendToRightHand)
            {
                PlayerManager.Instance.SendHapticToBothHands(intensity, duration);
            }
            else if (sendToLeftHand)
            {
                PlayerManager.Instance.SendHapticToLeftHand(intensity, duration);
            }
            else if (sendToRightHand)
            {
                PlayerManager.Instance.SendHapticToRightHand(intensity, duration);
            }
        }
        
        lastHapticTime = Time.time;
    }
    
    /// <summary>
    /// 트리거 상태 리셋 (다시 트리거할 수 있도록)
    /// </summary>
    public void ResetTriggerState()
    {
        hasImmediateTriggered = false;
        hasDurationTriggered = false;
        isInContact = false;
        contactStartTime = 0f;
        currentTriggerObject = null;
        isLeftHand = false;
        isRightHand = false;
        Debug.Log($"TriggerDetection {gameObject.name} trigger state reset");
    }
    
    /// <summary>
    /// 즉시 트리거만 리셋
    /// </summary>
    public void ResetImmediateTrigger()
    {
        hasImmediateTriggered = false;
        Debug.Log($"TriggerDetection {gameObject.name} immediate trigger reset");
    }
    
    /// <summary>
    /// 시간 트리거만 리셋
    /// </summary>
    public void ResetDurationTrigger()
    {
        hasDurationTriggered = false;
        Debug.Log($"TriggerDetection {gameObject.name} duration trigger reset");
    }
}
