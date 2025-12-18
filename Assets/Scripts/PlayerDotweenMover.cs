using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening.Plugins.Core.PathCore;
using Autohand;
using TheFeelies.Managers;

namespace TheFeelies.Core
{
    /// <summary>
    /// PlayerManager를 통해 AutoHandPlayer를 DOTween으로 이동시키는 스크립트
    /// 다중 씬 환경에서 안전하게 플레이어를 이동시킬 수 있습니다
    /// </summary>
    public class PlayerDotweenMover : MonoBehaviour
    {
        [Header("경로 설정")]
        [Tooltip("이동 경로의 웨이포인트들")]
        public Transform[] waypoints;
        
        [Header("애니메이션 설정")]
        [Tooltip("전체 경로를 이동하는데 걸리는 시간")]
        public float duration = 5f;
        
        [Tooltip("경로 타입")]
        public PathType pathType = PathType.CatmullRom;
        
        [Tooltip("경로 모드")]
        public PathMode pathMode = PathMode.Full3D;
        
        [Tooltip("경로 해상도 (부드러움의 정도)")]
        [Range(1, 20)]
        public int resolution = 10;
        
        [Header("루프 설정")]
        [Tooltip("반복 횟수 (-1은 무한 반복)")]
        public int loops = 1;
        
        [Tooltip("루프 타입")]
        public LoopType loopType = LoopType.Restart;
        
        [Header("이징 설정")]
        [Tooltip("이동 이징")]
        public Ease ease = Ease.Linear;
        
        [Header("자동 재생")]
        [Tooltip("시작 시 자동으로 재생")]
        public bool playOnStart = false;
        
        [Header("회전 설정")]
        [Tooltip("이동 방향으로 플레이어 회전")]
        public bool rotatePlayerDirection = false;
        
        [Header("카메라 회전 설정")]
        [Tooltip("각 웨이포인트마다 카메라 방향 회전")]
        public bool rotateCameraAtWaypoints = false;
        
        [Tooltip("카메라 회전 시간")]
        public float cameraRotationDuration = 0.5f;
        
        [Tooltip("카메라 회전 이징")]
        public Ease cameraRotationEase = Ease.OutQuad;
        
        [Header("플레이어 제어 설정")]
        [Tooltip("이동 중 플레이어 제어 비활성화")]
        public bool disablePlayerControlDuringMove = true;
        
        [Header("디버그")]
        [Tooltip("Scene 뷰에서 경로 표시")]
        public bool showPath = true;
        
        [Tooltip("경로 색상")]
        public Color pathColor = Color.cyan;
        
        [Header("이벤트")]
        [Tooltip("경로 이동이 완료되었을 때 호출되는 이벤트")]
        public UnityEvent onPathComplete;
        
        [Tooltip("경로 이동이 시작될 때 호출되는 이벤트")]
        public UnityEvent onPathStart;
        
        [Tooltip("웨이포인트 변경 시 호출되는 이벤트")]
        public UnityEvent<int> onWaypointChanged;
        
        private Tweener moveTweener;
        private Tweener cameraRotationTweener;
        private Vector3[] pathPoints;
        private int currentWaypointIndex = 0;
        private bool wasPlayerControlEnabled = true;
        
        // 물리 충돌 방지를 위한 Rigidbody 제어
        private Rigidbody playerRigidbody;
        private bool wasKinematic;

        // 가상의 이동 오브젝트 (DOTween이 직접 제어)
        private GameObject virtualMover;
        
        void Start()
        {
            if (playOnStart)
            {
                PlayPath();
            }
        }
        
        /// <summary>
        /// 경로를 따라 플레이어 이동 시작
        /// </summary>
        [ContextMenu("경로 재생")]
        public void PlayPath()
        {
            if (waypoints == null || waypoints.Length < 1)
            {
                Debug.LogWarning("PlayerDotweenMover: 최소 1개 이상의 웨이포인트가 필요합니다!");
                return;
            }
            
            if (PlayerManager.Instance == null)
            {
                Debug.LogError("PlayerDotweenMover: PlayerManager를 찾을 수 없습니다!");
                return;
            }
            
            AutoHandPlayer player = PlayerManager.Instance.GetAutoHandPlayer();
            if (player == null)
            {
                Debug.LogError("PlayerDotweenMover: AutoHandPlayer를 찾을 수 없습니다!");
                return;
            }
            
            // 기존 트윈 종료
            StopPath();
            
            // 가상 이동 오브젝트 생성
            if (virtualMover == null)
            {
                virtualMover = new GameObject("PlayerVirtualMover");
                virtualMover.transform.position = player.transform.position;
            }
            else
            {
                virtualMover.transform.position = player.transform.position;
            }
            
            // Transform 배열을 Vector3 배열로 변환
            List<Vector3> validPathPoints = new List<Vector3>();

            // 이벤트 알림: 이 무버가 시작됨을 PlayerManager에 알림
            if (PlayerManager.Instance != null && PlayerManager.Instance.MovementEvents != null)
            {
                PlayerManager.Instance.MovementEvents.NotifyDotweenMoveStarted(this);
            }

            // 현재 위치를 시작점으로 추가 (사용자 요청: 시작점을 포함하여 경로 구성)
            if (virtualMover != null)
            {
                validPathPoints.Add(virtualMover.transform.position);
            }
            
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    validPathPoints.Add(waypoints[i].position);
                }
                else
                {
                    Debug.LogWarning($"PlayerDotweenMover: 웨이포인트 {i}가 비어있습니다!");
                }
            }

            // 시작점만 있고 목적지가 없는 경우 (즉, 웨이포인트가 하나도 유효하지 않은 경우)
            if (validPathPoints.Count <= 1)
            {
                Debug.LogWarning("PlayerDotweenMover: 유효한 웨이포인트가 없습니다!");
                StopPath();
                return;
            }

            pathPoints = validPathPoints.ToArray();
            
            // 플레이어 제어 비활성화
            if (disablePlayerControlDuringMove)
            {
                wasPlayerControlEnabled = PlayerManager.Instance.IsPlayerControlEnabled();
                PlayerManager.Instance.SetPlayerControlEnabled(false);
            }
            
            // DOPath를 사용하여 가상 오브젝트 이동
            var pathTweener = virtualMover.transform.DOPath(pathPoints, duration, pathType, pathMode, resolution);
            
            // 이동 방향으로 회전 (DOPath 직후에 호출)
            if (rotatePlayerDirection)
            {
                pathTweener.SetLookAt(0.01f, Vector3.zero);
            }
            
            moveTweener = pathTweener
                .SetEase(ease)
                .SetLoops(loops, loopType)
                .OnStart(() => {
                    currentWaypointIndex = 0;
                    
                    // 물리 간섭 제거: Rigidbody Kinematic 설정
                    playerRigidbody = player.GetComponent<Rigidbody>();
                    if (playerRigidbody != null)
                    {
                        wasKinematic = playerRigidbody.isKinematic;
                        playerRigidbody.isKinematic = true;
                    }

                    onPathStart?.Invoke();
                    
                    Debug.Log("PlayerDotweenMover: 플레이어 경로 이동 시작 (Physics Disabled)");
                    
                    // 첫 번째 웨이포인트로 카메라 회전
                    if (rotateCameraAtWaypoints && waypoints.Length > 0)
                    {
                        RotateCameraToWaypoint(0);
                    }
                })
                .OnUpdate(() => {
                    // 매 프레임 플레이어 위치를 가상 오브젝트 위치로 동기화
                    UpdatePlayerPosition();
                })
                .OnWaypointChange((waypointIndex) => {
                    currentWaypointIndex = waypointIndex;
                    
                    // 웨이포인트 변경 이벤트
                    onWaypointChanged?.Invoke(waypointIndex);
                    
                    // 웨이포인트 변경 시 카메라 회전
                    if (rotateCameraAtWaypoints)
                    {
                        RotateCameraToWaypoint(waypointIndex);
                    }
                    
                    Debug.Log($"PlayerDotweenMover: 웨이포인트 {waypointIndex} 도달");
                })
                .OnComplete(() => {
                    Debug.Log("PlayerDotweenMover: 플레이어 경로 이동 완료");
                    
                    // 물리 상태 복원
                    if (playerRigidbody != null)
                    {
                        playerRigidbody.isKinematic = wasKinematic;
                    }

                    // 플레이어 제어 복원
                    if (disablePlayerControlDuringMove)
                    {
                        PlayerManager.Instance.SetPlayerControlEnabled(wasPlayerControlEnabled);
                    }
                    
                    onPathComplete?.Invoke();
                })
                .OnKill(() => {
                    // 트윈이 중단되어도 물리 상태 및 플레이어 제어 복원
                    if (playerRigidbody != null)
                    {
                        playerRigidbody.isKinematic = wasKinematic;
                    }

                    if (disablePlayerControlDuringMove && PlayerManager.Instance != null)
                    {
                        PlayerManager.Instance.SetPlayerControlEnabled(wasPlayerControlEnabled);
                    }
                });
        }
        
        /// <summary>
        /// 플레이어 위치를 가상 오브젝트 위치로 업데이트
        /// </summary>
        private void UpdatePlayerPosition()
        {
            if (PlayerManager.Instance == null || virtualMover == null)
                return;
            
            AutoHandPlayer player = PlayerManager.Instance.GetAutoHandPlayer();
            if (player != null)
            {
                // [FIX] AutoHandPlayer.SetPosition 대신 Transform 직접 제어
                // 물리(Rigidbody)가 Kinematic 상태이므로 Transform을 직접 옮겨도 됨
                
                // 위치만 동기화하고 회전은 적용하지 않음 (사용자 요청)
                player.transform.position = virtualMover.transform.position;
            }
        }
        
        /// <summary>
        /// 특정 웨이포인트 방향으로 카메라 회전 (XZ 평면만, Y축 회전만)
        /// </summary>
        private void RotateCameraToWaypoint(int waypointIndex)
        {
            if (waypoints == null || waypoints.Length == 0 || waypointIndex >= waypoints.Length)
                return;
            
            if (PlayerManager.Instance == null)
                return;
            
            AutoHandPlayer player = PlayerManager.Instance.GetAutoHandPlayer();
            if (player == null || player.headCamera == null)
                return;
            
            Transform cameraTransform = player.headCamera.transform;
            
            // 현재 웨이포인트 위치
            Vector3 currentWaypoint = waypoints[waypointIndex].position;
            
            // 카메라 위치
            Vector3 cameraPosition = cameraTransform.position;
            
            // 방향 벡터 계산 (Y축 무시)
            Vector3 direction = currentWaypoint - cameraPosition;
            direction.y = 0; // Y축 무시
            
            // 방향이 있는 경우에만 회전
            if (direction.sqrMagnitude > 0.001f)
            {
                // 기존 카메라 회전 트윈 중지
                if (cameraRotationTweener != null)
                {
                    cameraRotationTweener.Kill();
                }
                
                // 목표 회전 계산
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                
                // 현재 X축 회전 유지 (상하 각도 유지)
                Vector3 currentEuler = cameraTransform.eulerAngles;
                Vector3 targetEuler = targetRotation.eulerAngles;
                targetEuler.x = currentEuler.x;
                targetEuler.z = currentEuler.z;
                
                // 부드럽게 회전
                cameraRotationTweener = cameraTransform.DORotate(targetEuler, cameraRotationDuration)
                    .SetEase(cameraRotationEase);
                
                Debug.Log($"PlayerDotweenMover: 카메라 회전 - 웨이포인트 {waypointIndex}");
            }
        }
        
        /// <summary>
        /// 경로 이동 일시정지
        /// </summary>
        [ContextMenu("경로 일시정지")]
        public void PausePath()
        {
            if (moveTweener != null)
            {
                moveTweener.Pause();
            }
        }
        
        /// <summary>
        /// 경로 이동 재개
        /// </summary>
        [ContextMenu("경로 재개")]
        public void ResumePath()
        {
            if (moveTweener != null)
            {
                moveTweener.Play();
            }
        }
        
        /// <summary>
        /// 경로 이동 중지
        /// </summary>
        [ContextMenu("경로 중지")]
        public void StopPath()
        {
            if (moveTweener != null)
            {
                moveTweener.Kill();
                moveTweener = null;
            }
            
            if (cameraRotationTweener != null)
            {
                cameraRotationTweener.Kill();
                cameraRotationTweener = null;
            }
        }
        
        /// <summary>
        /// 경로 이동 재시작
        /// </summary>
        [ContextMenu("경로 재시작")]
        public void RestartPath()
        {
            if (moveTweener != null)
            {
                moveTweener.Restart();
            }
            else
            {
                PlayPath();
            }
        }
        
        /// <summary>
        /// 특정 시간으로 이동
        /// </summary>
        public void GoToTime(float time)
        {
            if (moveTweener != null)
            {
                moveTweener.Goto(time);
            }
        }
        
        /// <summary>
        /// 특정 진행도로 이동 (0~1)
        /// </summary>
        public void GoToProgress(float progress)
        {
            if (moveTweener != null)
            {
                float time = duration * Mathf.Clamp01(progress);
                moveTweener.Goto(time);
            }
        }
        
        void OnDestroy()
        {
            // 오브젝트 파괴 시 트윈 정리
            StopPath();
            
            if (virtualMover != null)
            {
                Destroy(virtualMover);
            }
        }
        
        // Scene 뷰에서 경로 시각화
        void OnDrawGizmos()
        {
            if (!showPath || waypoints == null || waypoints.Length < 1)
                return;
            
            // 웨이포인트 포인트 그리기
            Gizmos.color = pathColor;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, 0.15f);
                    
                    // 다음 포인트로 선 그리기
                    if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                }
            }
            
            // 루프가 Yoyo가 아닌 경우 마지막 점에서 첫 점으로 선 그리기
            if (loops != 0 && loopType != LoopType.Yoyo && waypoints.Length > 0)
            {
                if (waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
                {
                    Gizmos.color = new Color(pathColor.r, pathColor.g, pathColor.b, 0.5f);
                    Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
                }
            }
        }
        
        void OnDrawGizmosSelected()
        {
            if (!showPath || waypoints == null || waypoints.Length < 1)
                return;
            
            // 웨이포인트 번호 표시
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(waypoints[i].position, $"Player Point {i}");
#endif
                }
            }
        }
    }
}

