using UnityEngine;
using UnityEngine.Events;

public class TriggerDetection : MonoBehaviour
{
    [Header("Trigger Detection")]
    [SerializeField] private string[] allowedTriggerTags = { "GameController"};
    [SerializeField] private bool preventMultipleTriggers = true;
    [SerializeField] private float triggerCooldown = 0.5f;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onTriggerActivated;
    
    private bool hasBeenTriggered = false;
    private float lastTriggerTime = 0f;
    
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
        
        // 중복 트리거 방지
        if (preventMultipleTriggers && hasBeenTriggered) return;
        
        // 쿨다운 체크
        if (Time.time - lastTriggerTime < triggerCooldown) return;
        
        // 허용된 태그인지 확인
        bool isValidTrigger = false;
        foreach (string tag in allowedTriggerTags)
        {
            if (!string.IsNullOrEmpty(tag) && other.CompareTag(tag))
            {
                isValidTrigger = true;
                break;
            }
        }
        
        if (isValidTrigger)
        {
            Debug.Log($"TriggerDetection {gameObject.name} triggered by {other.name}");
            
            // 트리거 상태 업데이트
            hasBeenTriggered = true;
            lastTriggerTime = Time.time;
            
            // 이벤트 호출
            onTriggerActivated?.Invoke();
        }
    }
    
    /// <summary>
    /// 트리거 상태 리셋 (다시 트리거할 수 있도록)
    /// </summary>
    public void ResetTriggerState()
    {
        hasBeenTriggered = false;
        Debug.Log($"TriggerDetection {gameObject.name} trigger state reset");
    }
}
