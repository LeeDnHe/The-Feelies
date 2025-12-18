using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif
using StarterAssets;
using System.Collections.Generic;
using TheFeelies.Managers; // PlayerManager 참조
using TheFeelies.Core; // PlayerDotweenMover 참조
using TheFeelies.Events; // PlayerMovementEvents 참조

public class AutoPilotSystem : MonoBehaviour
{
    [Header("References")]
    [Tooltip("캐릭터의 StarterAssetsInputs 컴포넌트")]
    public StarterAssetsInputs starterAssetsInputs;
    
    [Tooltip("ThirdPersonController가 참조하는 메인 카메라 (보통 VR 카메라)")]
    public Transform cameraTransform;

    [Header("Path Settings")]
    [Tooltip("PlayerDotweenMover 컴포넌트나 이벤트에서 경로를 자동으로 가져올지 여부")]
    public bool useDotweenMoverPath = true;

    [Tooltip("캐릭터가 순서대로 지나갈 경로 지점들")]
    public List<Transform> waypoints;
    public float stopDistance = 0.5f;
    public bool loopPath = false;
    public bool sprint = false;

    private int _currentWaypointIndex = 0;
    private bool _isAutoPiloting = false;
    private PlayerDotweenMover _dotweenMover; // 현재 추적 중인 무버

    private void Start()
    {
        // StarterAssetsInputs 자동 검색
        if (starterAssetsInputs == null)
            starterAssetsInputs = GetComponent<StarterAssetsInputs>();

        // 카메라 자동 검색
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // PlayerDotweenMover 자동 검색 (초기값)
        _dotweenMover = GetComponent<PlayerDotweenMover>();
    }

    private void OnEnable()
    {
        // PlayerManager의 이벤트 구독 (활성화될 때마다 구독)
        if (PlayerManager.Instance != null && PlayerManager.Instance.MovementEvents != null)
        {
            PlayerManager.Instance.MovementEvents.OnDotweenMoveStarted += OnDotweenMoveStarted;
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제 (비활성화될 때마다 해제)
        if (PlayerManager.Instance != null && PlayerManager.Instance.MovementEvents != null)
        {
            PlayerManager.Instance.MovementEvents.OnDotweenMoveStarted -= OnDotweenMoveStarted;
        }
    }

    private void OnDestroy()
    {
        // OnDisable에서 이미 해제하지만 안전을 위해 한 번 더 체크
        if (PlayerManager.Instance != null && PlayerManager.Instance.MovementEvents != null)
        {
            PlayerManager.Instance.MovementEvents.OnDotweenMoveStarted -= OnDotweenMoveStarted;
        }
    }

    /// <summary>
    /// 새로운 PlayerDotweenMover가 실행되었을 때 호출됨 (이벤트 핸들러)
    /// </summary>
    private void OnDotweenMoveStarted(PlayerDotweenMover mover)
    {
        Debug.Log($"AutoPilotSystem: 새로운 경로 이벤트 수신 - {mover.name}");
        _dotweenMover = mover;

        // 만약 이미 오토파일럿 모드이고, 경로 동기화 옵션이 켜져있다면
        // 즉시 새 경로로 갱신하여 계속 이동하게 함
        if (_isAutoPiloting && useDotweenMoverPath)
        {
            Debug.Log("AutoPilotSystem: 실행 중인 경로를 새 경로로 교체합니다.");
            
            // StartAutoPilot을 다시 호출하면 내부에서 _dotweenMover의 경로를 다시 읽어와서 시작함
            StartAutoPilot(); 
        }
    }

    private void Update()
    {
        if (!_isAutoPiloting) return;
        
        // 웨이포인트가 없으면 작동 중지
        if (waypoints == null || waypoints.Count == 0) return;

        MoveTowardsWaypoint();
    }

    /// <summary>
    /// 오토파일럿 시작 (3인칭 전환 시 호출)
    /// </summary>
    public void StartAutoPilot()
    {
        if (starterAssetsInputs == null)
        {
            Debug.LogError("AutoPilotSystem: StarterAssetsInputs가 연결되지 않았습니다.");
            return;
        }

        // PlayerDotweenMover에서 경로 동기화
        if (useDotweenMoverPath)
        {
            // _dotweenMover가 null이면 GetComponent로 시도 (안전장치)
            if (_dotweenMover == null) 
                _dotweenMover = GetComponent<PlayerDotweenMover>();
            
            // 경로 가져오기
            if (_dotweenMover != null && _dotweenMover.waypoints != null && _dotweenMover.waypoints.Length > 0)
            {
                // 기존 리스트를 지우고 DotweenMover의 경로로 덮어쓰기
                waypoints = new List<Transform>();
                foreach (var wp in _dotweenMover.waypoints)
                {
                    if (wp != null) waypoints.Add(wp);
                }
                Debug.Log($"AutoPilotSystem: PlayerDotweenMover({_dotweenMover.name})에서 {waypoints.Count}개의 웨이포인트를 동기화했습니다.");
            }
        }

        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning("AutoPilotSystem: 이동할 웨이포인트가 없습니다.");
            return;
        }

        _isAutoPiloting = true;
        _currentWaypointIndex = 0; // 시작할 때 항상 첫 번째 지점부터
        
        // 달리기 설정 적용
        starterAssetsInputs.SprintInput(sprint);
        
        Debug.Log("AutoPilotSystem: 자동 이동 시작");
    }

    /// <summary>
    /// 오토파일럿 종료 (1인칭 복귀 시 호출)
    /// </summary>
    public void StopAutoPilot()
    {
        _isAutoPiloting = false;
        
        if (starterAssetsInputs != null)
        {
            // 입력 초기화 (멈춤)
            starterAssetsInputs.MoveInput(Vector2.zero);
            starterAssetsInputs.SprintInput(false);
        }
        
        Debug.Log("AutoPilotSystem: 자동 이동 종료");
    }

    private void MoveTowardsWaypoint()
    {
        if (_currentWaypointIndex >= waypoints.Count) return;

        Transform target = waypoints[_currentWaypointIndex];
        if (target == null) return;

        // 1. 방향 계산
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0; // 높이는 무시 (지형 처리는 컨트롤러가 함)

        // 2. 거리 체크 및 웨이포인트 전환
        if (directionToTarget.magnitude < stopDistance)
        {
            // 다음 웨이포인트로
            _currentWaypointIndex++;
            
            // 끝에 도달했는지 확인
            if (_currentWaypointIndex >= waypoints.Count)
            {
                if (loopPath)
                {
                    _currentWaypointIndex = 0;
                }
                else
                {
                    // 도착 완료 - 멈춤
                    starterAssetsInputs.MoveInput(Vector2.zero);
                    return; 
                }
            }
            
            // 웨이포인트가 바뀌었으므로 타겟 갱신
            target = waypoints[_currentWaypointIndex];
            directionToTarget = target.position - transform.position;
            directionToTarget.y = 0;
        }

        // 3. 입력값 변환 (World Space -> Camera Space)
        // ThirdPersonController는 입력(W/A/S/D)을 카메라가 보는 방향 기준으로 해석합니다.
        // 따라서 AI가 월드 좌표 절대값으로 이동하려면, 카메라 회전만큼 반대로 돌려줘야 합니다.
        
        Vector3 normalizedDir = directionToTarget.normalized;
        
        // 카메라가 없으면 월드 좌표 그대로 사용
        if (cameraTransform != null)
        {
            float cameraYRotation = cameraTransform.eulerAngles.y;
            // 목표 방향 벡터를 카메라 회전의 반대만큼 회전시킴
            Quaternion rotation = Quaternion.Euler(0, -cameraYRotation, 0);
            normalizedDir = rotation * normalizedDir;
        }

        // 4. 입력 주입
        // MoveInput은 Vector2(x, y)를 받으며, x는 좌우(A/D), y는 전후(W/S)입니다.
        starterAssetsInputs.MoveInput(new Vector2(normalizedDir.x, normalizedDir.z));
    }
}
