using UnityEngine;
using Autohand;

/// <summary>
/// VR 환경에서 1인칭/3인칭 뷰를 전환하는 시스템
/// HMD 회전은 유지하면서 카메라 위치만 변경합니다.
/// </summary>
[DefaultExecutionOrder(100)]  // AutoHandPlayer(1) 이후에 실행되도록 설정
public class VRCameraViewSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AutoHandPlayer player;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Camera vrCamera;
    [SerializeField] private GameObject playerModel; // 3인칭 시 활성화할 플레이어 모델
    [SerializeField] private AutoPilotSystem autoPilot; // 자동 이동 시스템 연결
    
    [Header("Third Person Settings")]
    [Tooltip("플레이어 기준 3인칭 카메라 오프셋 (로컬 좌표)")]
    [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0f, 2f, -3f);
    [Tooltip("카메라 전환 속도")]
    [SerializeField] private float transitionSpeed = 5f;
    [Tooltip("부드러운 전환 활성화")]
    [SerializeField] private bool smoothTransition = true;
    
    [Header("View State")]
    [SerializeField] private bool isThirdPerson = false;
    
    // Internal
    private Transform trackingContainer;
    private bool hasLookedAtPlayer = false; // 플레이어를 바라봤는지 추적

    void Start()
    {
        InitializeReferences();
    }

    void InitializeReferences()
    {
        // AutoHandPlayer 자동 검색
        if (player == null)
            player = FindObjectOfType<AutoHandPlayer>();
        
        if (player == null)
        {
            Debug.LogError("VRCameraViewSwitcher: AutoHandPlayer를 찾을 수 없습니다!");
            enabled = false;
            return;
        }
        
        // VR 카메라 참조
        if (vrCamera == null)
            vrCamera = player.headCamera;
        
        // 플레이어 바디 참조
        if (playerBody == null)
            playerBody = player.transform;
        
        // TrackingContainer 참조 (중요! VR 카메라는 이것을 통해 제어됨)
        trackingContainer = player.trackingContainer;

        // 시작 시 모델 상태 초기화 (현재 시점에 맞게)
        if (playerModel != null)
        {
            playerModel.SetActive(isThirdPerson);
        }

        // AutoPilotSystem 자동 검색 (할당되지 않았을 경우)
        if (autoPilot == null && playerBody != null)
        {
            autoPilot = playerBody.GetComponent<AutoPilotSystem>();
            if (autoPilot == null && playerModel != null)
                autoPilot = playerModel.GetComponent<AutoPilotSystem>();
        }
        
        Debug.Log("VRCameraViewSwitcher: 초기화 완료");
        Debug.Log($"Player Body: {playerBody.name}, Tracking Container: {trackingContainer.name}");
    }

    void LateUpdate()
    {
        // AutoHandPlayer의 UpdateTrackedObjects 이후에 실행되도록 LateUpdate에서만 처리
        if (isThirdPerson)
        {
            UpdateThirdPersonCamera();
        }
    }

    void UpdateThirdPersonCamera()
    {
        // VR에서는 카메라를 직접 움직일 수 없고, trackingContainer를 움직여야 함
        Vector3 targetPosition = playerBody.position + playerBody.TransformDirection(thirdPersonOffset);
        
        // trackingContainer를 목표 위치로 이동 (카메라는 자식이므로 자동으로 따라감)
        Vector3 offsetFromCamera = targetPosition - vrCamera.transform.position;
        trackingContainer.position += offsetFromCamera;
        
        // 디버그 로그로 추적 확인 (삭제)
        // if (Time.frameCount % 60 == 0) // 1초에 한 번씩만 출력
        // {
        //    Debug.Log($"[3인칭] 플레이어: {playerBody.position}, 목표: {targetPosition}, 카메라: {vrCamera.transform.position}, TrackingContainer: {trackingContainer.position}");
        // }
        
        // 플레이어를 바라보도록 (한 번만)
        if (!hasLookedAtPlayer)
        {
            Vector3 lookDirection = playerBody.position + Vector3.up * 1.5f - vrCamera.transform.position;
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                
                // Z축(Roll)과 X축(Pitch) 회전을 모두 제거하여 멀미 방지 및 수평 유지
                Vector3 euler = targetRotation.eulerAngles;
                euler.z = 0f; // Roll 제거 (기울어짐 방지)
                euler.x = 0f; // Pitch 제거 (위아래 끄덕임 방지 - 항상 수평 유지)
                targetRotation = Quaternion.Euler(euler);
                
                // trackingContainer를 회전시켜서 카메라가 플레이어를 바라보도록
                Quaternion deltaRotation = targetRotation * Quaternion.Inverse(vrCamera.transform.rotation);
                
                // deltaRotation의 X, Z축도 제거하여 순수하게 Y축(Yaw) 회전만 적용
                Vector3 deltaEuler = deltaRotation.eulerAngles;
                deltaEuler.z = 0f;
                deltaEuler.x = 0f;
                deltaRotation = Quaternion.Euler(deltaEuler);
                
                trackingContainer.rotation = deltaRotation * trackingContainer.rotation;
                
                hasLookedAtPlayer = true;
                Debug.Log($"3인칭 카메라 정렬 완료 (X/Z축 회전 제거). 이후 HMD 자유 회전 가능.");
            }
        }
    }

    /// <summary>
    /// 뷰 전환 토글 - 오른손 PrimaryButton에 이 함수를 매핑하세요
    /// </summary>
    public void ToggleView()
    {
        if (isThirdPerson)
        {
            SetFirstPersonView();
        }
        else
        {
            SetThirdPersonView();
        }
    }

    /// <summary>
    /// 3인칭 뷰로 전환
    /// </summary>
    public void SetThirdPersonView()
    {
        if (isThirdPerson) return;
        
        isThirdPerson = true;
        hasLookedAtPlayer = false; // 플래그 리셋
        
        // VR에서는 카메라를 부모에서 분리하지 않음 - trackingContainer를 통해 제어
        // 즉시 3인칭 위치로 이동
        Vector3 targetPosition = playerBody.position + playerBody.TransformDirection(thirdPersonOffset);
        Vector3 offsetFromCamera = targetPosition - vrCamera.transform.position;
        trackingContainer.position += offsetFromCamera;
        
        // 3인칭 전환 시 플레이어 모델 활성화
        if (playerModel != null)
        {
            playerModel.SetActive(true);
        }

        // 오토파일럿 시작
        if (autoPilot != null)
        {
            autoPilot.StartAutoPilot();
        }
        
        Debug.Log($"3인칭 뷰로 전환 완료 - 플레이어: {playerBody.position}, 카메라: {vrCamera.transform.position}, TrackingContainer: {trackingContainer.position}");
    }

    /// <summary>
    /// 1인칭 뷰로 전환
    /// </summary>
    public void SetFirstPersonView()
    {
        if (!isThirdPerson) return;
        
        isThirdPerson = false;
        hasLookedAtPlayer = false; // 플래그 리셋
        
        // trackingContainer는 AutoHandPlayer가 관리하므로 추가 작업 불필요
        // 자동으로 1인칭 뷰로 복귀됨
        
        // 1인칭 전환 시 플레이어 모델 비활성화
        if (playerModel != null)
        {
            playerModel.SetActive(false);
        }

        // 오토파일럿 종료
        if (autoPilot != null)
        {
            autoPilot.StopAutoPilot();
        }
        
        Debug.Log($"1인칭 뷰로 전환 - 카메라 위치: {vrCamera.transform.position}");
    }

    /// <summary>
    /// 현재 3인칭 모드인지 확인
    /// </summary>
    public bool IsThirdPerson()
    {
        return isThirdPerson;
    }

    /// <summary>
    /// 3인칭 오프셋 변경
    /// </summary>
    public void SetThirdPersonOffset(Vector3 offset)
    {
        thirdPersonOffset = offset;
    }

    // 에디터에서 오프셋 확인용 기즈모
    void OnDrawGizmos()
    {
        if (playerBody != null)
        {
            Gizmos.color = isThirdPerson ? Color.green : Color.yellow;
            Vector3 thirdPersonPos = playerBody.position + playerBody.TransformDirection(thirdPersonOffset);
            Gizmos.DrawWireSphere(thirdPersonPos, 0.3f);
            Gizmos.DrawLine(playerBody.position, thirdPersonPos);
            
            // 플레이어 위치 표시
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(playerBody.position + Vector3.up * 1f, Vector3.one * 0.5f);
        }
    }

}

