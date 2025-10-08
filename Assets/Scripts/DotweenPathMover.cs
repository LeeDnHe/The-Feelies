using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening.Plugins.Core.PathCore;

public class DotweenPathMover : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("이동할 오브젝트 (비어있으면 현재 오브젝트)")]
    public Transform targetObject;
    
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
    public bool playOnStart = true;
    
    [Header("회전 설정")]
    [Tooltip("이동 방향으로 회전")]
    public bool lookAtDirection = false;
    
    [Tooltip("회전 각도 오프셋")]
    public Vector3 lookAheadOffset = Vector3.zero;
    
    [Header("카메라 회전 설정")]
    [Tooltip("각 웨이포인트마다 카메라 방향 회전")]
    public bool rotateCameraAtWaypoints = false;
    
    [Tooltip("회전시킬 카메라")]
    public Transform cameraTransform;
    
    [Tooltip("카메라 회전 시간")]
    public float cameraRotationDuration = 0.5f;
    
    [Tooltip("카메라 회전 이징")]
    public Ease cameraRotationEase = Ease.OutQuad;
    
    [Header("디버그")]
    [Tooltip("Scene 뷰에서 경로 표시")]
    public bool showPath = true;
    
    [Tooltip("경로 색상")]
    public Color pathColor = Color.green;
    
    [Header("이벤트")]
    [Tooltip("경로 이동이 완료되었을 때 호출되는 이벤트")]
    public UnityEvent onPathComplete;
    
    [Tooltip("경로 이동이 시작될 때 호출되는 이벤트")]
    public UnityEvent onPathStart;
    
    private TweenerCore<Vector3, Path, PathOptions> pathTweener;
    private Vector3[] pathPoints;
    private Tweener cameraRotationTweener;
    private int currentWaypointIndex = 0;
    
    void Start()
    {
        // 타겟 오브젝트가 설정되지 않았다면 현재 오브젝트 사용
        if (targetObject == null)
        {
            targetObject = transform;
        }
        
        if (playOnStart)
        {
            PlayPath();
        }
    }
    
    /// <summary>
    /// 경로를 따라 이동 시작
    /// </summary>
    [ContextMenu("경로 재생")]
    public void PlayPath()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning("최소 2개 이상의 웨이포인트가 필요합니다!");
            return;
        }
        
        // 기존 트윈 종료
        if (pathTweener != null)
        {
            pathTweener.Kill();
        }
        
        // Transform 배열을 Vector3 배열로 변환
        pathPoints = new Vector3[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                pathPoints[i] = waypoints[i].position;
            }
            else
            {
                Debug.LogWarning($"웨이포인트 {i}가 비어있습니다!");
            }
        }
        
        // DOPath를 사용하여 경로 이동
        pathTweener = targetObject.DOPath(pathPoints, duration, pathType, pathMode, resolution)
            .SetEase(ease)
            .SetLoops(loops, loopType)
            .OnStart(() => {
                // 시작 이벤트 호출
                currentWaypointIndex = 0;
                onPathStart?.Invoke();
                
                // 첫 번째 웨이포인트로 카메라 회전
                if (rotateCameraAtWaypoints && waypoints.Length > 0)
                {
                    RotateCameraToWaypoint(0);
                }
            })
            .OnWaypointChange((waypointIndex) => {
                // 웨이포인트 변경 시 카메라 회전
                if (rotateCameraAtWaypoints)
                {
                    RotateCameraToWaypoint(waypointIndex);
                }
            })
            .OnComplete(() => {
                // 완료 이벤트 호출
                onPathComplete?.Invoke();
            });
        
        // 이동 방향으로 회전
        if (lookAtDirection)
        {
            pathTweener.SetLookAt(0.01f, lookAheadOffset);
        }
    }
    
    /// <summary>
    /// 특정 웨이포인트 방향으로 카메라 회전 (XZ 평면만, Y축 회전만)
    /// </summary>
    private void RotateCameraToWaypoint(int waypointIndex)
    {
        if (cameraTransform == null || waypoints == null || waypoints.Length == 0)
            return;
        
        currentWaypointIndex = waypointIndex;
        
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
            
            Debug.Log($"카메라 회전: 웨이포인트 {waypointIndex}로 회전");
        }
    }
    
    /// <summary>
    /// 경로 이동 일시정지
    /// </summary>
    [ContextMenu("경로 일시정지")]
    public void PausePath()
    {
        if (pathTweener != null)
        {
            pathTweener.Pause();
        }
    }
    
    /// <summary>
    /// 경로 이동 재개
    /// </summary>
    [ContextMenu("경로 재개")]
    public void ResumePath()
    {
        if (pathTweener != null)
        {
            pathTweener.Play();
        }
    }
    
    /// <summary>
    /// 경로 이동 중지
    /// </summary>
    [ContextMenu("경로 중지")]
    public void StopPath()
    {
        if (pathTweener != null)
        {
            pathTweener.Kill();
            pathTweener = null;
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
        if (pathTweener != null)
        {
            pathTweener.Restart();
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
        if (pathTweener != null)
        {
            pathTweener.Goto(time);
        }
    }
    
    /// <summary>
    /// 특정 진행도로 이동 (0~1)
    /// </summary>
    public void GoToProgress(float progress)
    {
        if (pathTweener != null)
        {
            float time = duration * Mathf.Clamp01(progress);
            pathTweener.Goto(time);
        }
    }
    
    void OnDestroy()
    {
        // 오브젝트 파괴 시 트윈 정리
        if (pathTweener != null)
        {
            pathTweener.Kill();
        }
        
        if (cameraRotationTweener != null)
        {
            cameraRotationTweener.Kill();
        }
    }
    
    // Scene 뷰에서 경로 시각화
    void OnDrawGizmos()
    {
        if (!showPath || waypoints == null || waypoints.Length < 2)
            return;
        
        // 웨이포인트 포인트 그리기
        Gizmos.color = pathColor;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                Gizmos.DrawSphere(waypoints[i].position, 0.1f);
                
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
        if (!showPath || waypoints == null || waypoints.Length < 2)
            return;
        
        // 웨이포인트 번호 표시
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(waypoints[i].position, $"Point {i}");
#endif
            }
        }
    }
}

