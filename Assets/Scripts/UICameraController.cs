using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICameraController : MonoBehaviour
{
    [Header("추적 설정")]
    [SerializeField] private Transform mainCamera; // 메인 카메라 Transform
    [SerializeField] private float positionFollowSpeed = 2f; // 위치 따라가는 속도 (낮을수록 느림, 권장: 0.5~5)
    [SerializeField] private float rotationFollowSpeed = 2f; // 회전 따라가는 속도 (낮을수록 느림, 권장: 0.5~5)
    
    [Header("고급 옵션")]
    [SerializeField] private bool useDistanceScaling = true; // 거리에 따른 속도 조절
    [SerializeField] private float minDistance = 0.01f; // 이 거리 이하면 추적 중지
    
    [Header("옵션")]
    [SerializeField] private bool followPosition = true; // 위치 따라가기 활성화
    [SerializeField] private bool followRotation = true; // 회전 따라가기 활성화
    [SerializeField] private Vector3 positionOffset = Vector3.zero; // 위치 오프셋

    void Start()
    {
        // 메인 카메라가 할당되지 않았다면 자동으로 찾기
        if (mainCamera == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                mainCamera = cam.transform;
            }
            else
            {
                Debug.LogWarning("UICameraController: 메인 카메라를 찾을 수 없습니다.");
            }
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // 위치 부드럽게 따라가기
        if (followPosition)
        {
            Vector3 targetPosition = mainCamera.position + mainCamera.TransformDirection(positionOffset);
            float distance = Vector3.Distance(transform.position, targetPosition);
            
            // 최소 거리 이상일 때만 이동
            if (distance > minDistance)
            {
                float speed = positionFollowSpeed;
                
                // 거리에 비례하여 속도 조절 (선택적)
                if (useDistanceScaling)
                {
                    speed *= distance;
                }
                
                // 일정한 속도로 목표 지점을 향해 이동
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    targetPosition, 
                    speed * Time.deltaTime
                );
            }
        }

        // 회전 부드럽게 따라가기
        if (followRotation)
        {
            float rotationSpeed = rotationFollowSpeed * Time.deltaTime;
            
            // 각도 차이 계산
            float angleDifference = Quaternion.Angle(transform.rotation, mainCamera.rotation);
            
            // 최소 각도 이상일 때만 회전
            if (angleDifference > 0.1f)
            {
                if (useDistanceScaling)
                {
                    // 각도 차이에 비례하여 속도 조절
                    rotationSpeed *= (angleDifference / 180f);
                }
                
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, 
                    mainCamera.rotation, 
                    rotationSpeed * 100f // 각도 단위로 변환
                );
            }
        }
    }
}
