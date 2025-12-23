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

    [Header("Movement Settings")]
    [Tooltip("카메라 회전을 고려하여 입력을 보정할지 여부. VR이나 고정 카메라에서는 끄는 것이 좋을 수 있습니다.")]
    public bool useCameraRelativeInput = false;

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
        
        Debug.Log($"AutoPilotSystem: 자동 이동 시작. Camera: {(cameraTransform != null ? cameraTransform.name : "NULL")}");
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
        Vector3 originalWorldDir = normalizedDir; // 디버그용 원본 저장

        float cameraYRotation = 0f;
        
        // 카메라 기준 입력 사용 시에만 회전 보정
        if (useCameraRelativeInput && cameraTransform != null)
        {
            cameraYRotation = cameraTransform.eulerAngles.y;
            // 목표 방향 벡터를 카메라 회전의 반대만큼 회전시킴
            Quaternion rotation = Quaternion.Euler(0, -cameraYRotation, 0);
            normalizedDir = rotation * normalizedDir;
        }
        // 카메라 기준 입력을 안 쓸 경우 (월드 좌표 이동)
        // 하지만 ThirdPersonController는 내부적으로 카메라 회전을 더해서 계산하므로,
        // 오히려 "카메라 회전을 빼주는" 연산이 필요함.
        // 만약 useCameraRelativeInput이 false라면, ThirdPersonController가 카메라 회전을 
        // 무시하고 입력 그대로 이동하게 만들어야 하는데, 컨트롤러 수정 없이 하려면
        // 여기서 "현재 카메라 회전만큼 반대로 돌려서" 컨트롤러가 다시 더했을 때 원본이 되게 해야 함.
        else if (!useCameraRelativeInput && cameraTransform != null)
        {
             // ThirdPersonController 로직:
             // targetRotation = atan2(input) + camera.y
             // 우리가 원하는 것: targetRotation = atan2(worldDir)
             // 따라서 input 각도 = worldDir 각도 - camera.y 가 되어야 함.
             // 즉, 여기서도 결국 카메라 회전만큼 빼줘야 월드 좌표대로 감.
             
             // 결론: ThirdPersonController를 쓰는 한, 월드 좌표로 가려면 카메라 회전을 빼줘야 함.
             // 문제는 지금 'cameraTransform'이 실제 뷰와 다른 엉뚱한 값을 가지고 있을 가능성임.
             // 사용자가 "오른쪽으로 간다"고 한 것은, 카메라가 270도(서쪽/왼쪽)를 보고 있다고 생각해서
             // 앞으로 가기 위해 오른쪽(동쪽)으로 입력을 줬는데,
             // 실제 화면은 0도(북쪽/앞)를 보고 있어서 캐릭터가 오른쪽으로 가는 것처럼 보이는 것임.
             
             // 해결책: 오토파일럿 중에는 카메라 회전값을 0으로 간주하고(무시하고)
             // 컨트롤러가 카메라 회전을 더하지 않도록 하거나,
             // 여기서 그냥 월드 방향 그대로 찔러넣고 컨트롤러가 엉뚱하게 해석하지 않길 바라는 수밖에 없음.
             
             // 하지만 ThirdPersonController 코드를 보면 무조건 Camera.eulerAngles.y를 더함.
             // 따라서 여기서 카메라 회전을 역보정해주는게 맞음.
             // 그런데 279도로 고정되어 있다는 건, VR 헤드셋이 아니라 뭔가 고정된 다른 카메라를 잡고 있는 것 같음.
             
             // 일단 useCameraRelativeInput = false일 때는 카메라 회전을 아예 무시하고(0 취급)
             // 그냥 월드 방향을 넣도록 해보겠습니다.
             // 단, 이 경우 ThirdPersonController가 카메라 회전(279도)을 더해서 엉뚱한 곳으로 갈 위험이 있음.
             
             // 따라서 가장 확실한 수정: 
             // "카메라가 279도라고 주장하지만, 나는 그냥 월드 Z축으로 가고 싶다"
             // -> 입력값: 월드 방향(Z)을 카메라 회전(-279)만큼 돌려서 줌.
             
             // 지금 로그 상황: 
             // WorldDir: (0, 0, 1) [앞]
             // CamRot: 279 [왼쪽]
             // Input: (0.99, 0, 0.17) [오른쪽] -> "왼쪽을 보는 카메라 기준으로 오른쪽으로 가라" = "앞으로 가라"
             // 의도는 맞음.
             // 근데 캐릭터가 "오른쪽으로 걸어간다"는 건, 실제 화면(유저가 보는 뷰)은 "앞(0도)"을 보고 있다는 뜻.
             // 즉, cameraTransform 변수에 할당된 카메라(279도)와 실제 유저 눈(0도)이 서로 다름!
             
             // 해결책: 올바른 카메라(유저가 보고 있는 그 카메라)를 할당해야 함.
             // 지금 cameraTransform이 MainCamera(VR Camera)가 맞는지 확인 필요.
        }
        
        // 다시 로직 정리:
        // useCameraRelativeInput = false (기본값) -> 카메라 회전 무시하고 월드 방향 그대로 입력
        // (단, ThirdPersonController가 카메라 회전을 더해버리는 문제가 있으므로
        //  이 옵션을 쓰려면 ThirdPersonController가 MainCamera를 참조하지 않거나,
        //  MainCamera의 회전이 0이어야 함)
        
        // 임시 조치: 
        // 로그를 보니 CamRotY가 279도로 고정됨 -> VR 카메라가 아니라 뭔가 엉뚱한 걸 잡았거나
        // VR 헤드셋이 책상에 놓여서 돌아가 있는 상태일 수 있음.
        
        // 일단 "카메라 보정 로직"을 그대로 유지하되, cameraTransform을 MainCamera로 강제 갱신해보는 코드를 추가.
        
        if (cameraTransform != null)
        {
            cameraYRotation = cameraTransform.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, -cameraYRotation, 0);
            normalizedDir = rotation * normalizedDir;
        }

        // 디버그: 60프레임마다 한 번씩 로그 출력 (스팸 방지)
        if (Time.frameCount % 60 == 0)
        {
             Debug.Log($"AutoPilot [DirCheck]: \n" +
                       $"Target: {target.position}, Me: {transform.position}\n" +
                       $"WorldDir: {originalWorldDir} (Mag: {directionToTarget.magnitude})\n" +
                       $"CamRotY: {cameraYRotation} (Obj: {cameraTransform?.name})\n" +
                       $"Calculated Input: {normalizedDir} (X->Move.x, Z->Move.y)");
        }

        // 4. 입력 주입
        // MoveInput은 Vector2(x, y)를 받으며, x는 좌우(A/D), y는 전후(W/S)입니다.
        starterAssetsInputs.MoveInput(new Vector2(normalizedDir.x, normalizedDir.z));
    }
}
