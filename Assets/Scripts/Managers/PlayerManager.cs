using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace TheFeelies.Managers
{
    /// <summary>
    /// 오토핸드 플레이어 관리를 담당하는 매니저
    /// CutEvent에서 호출될 수 있는 메서드들을 제공
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        [Header("Player References")]
        [SerializeField] private GameObject player; // 플레이어 오브젝트 (인스펙터에서 할당)
        [SerializeField] private Rigidbody playerRigidbody; // 플레이어 Rigidbody (인스펙터에서 할당)
        [SerializeField] private Autohand.AutoHandPlayer autoHandPlayer; // AutoHand 플레이어 (인스펙터에서 할당)
        
        [Header("Controller Settings")]
        [SerializeField] private GameObject[] controllerObjects; // 시각적 컨트롤러 모델들 (알파값 페이드 대상)
        [SerializeField] private Autohand.Hand[] handControllers; // AutoHand.Hand 컴포넌트들 (입력 처리)
        [SerializeField] private float controllerFadeDuration = 2f; // 컨트롤러 페이드 시간
        
        [Header("Movement Settings")]
        [SerializeField] private GameObject[] movementObjects; // 이동 관련 오브젝트들
        [SerializeField] private float walkSpeed = 2.0f; // 걸음걸이 속도
        [SerializeField] private float rotationSpeed = 5.0f; // 회전 속도
        
        [Header("Player State")]
        [SerializeField] private bool isMovementEnabled = true;
        [SerializeField] private bool isInteractionEnabled = true;
        [SerializeField] private bool isControllerEnabled = true;
        [SerializeField] private bool isAutoMoving = false;
        [SerializeField] private bool isControllerFading = false;
        
        private static PlayerManager instance;
        public static PlayerManager Instance => instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                }
                else
                {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // 인스펙터에서 직접 할당된 것들만 사용
            // 동적 할당 코드 제거
        }
        
        /// <summary>
        /// 컨트롤러 활성화/비활성화 (CutEvent에서 호출)
        /// </summary>
        public void SetControllerEnabled(bool enabled)
        {
            if (isControllerFading) return; // 이미 페이딩 중이면 무시
            
            isControllerEnabled = enabled;
            isControllerFading = true;
            
            if (controllerObjects != null)
            {
                foreach (var controllerObj in controllerObjects)
                {
                    if (controllerObj != null)
                    {
                        FadeController(controllerObj, enabled);
                    }
                }
            }
            
            Debug.Log($"Player controller {(enabled ? "enabling" : "disabling")} with fade");
        }
        
        /// <summary>
        /// 컨트롤러 페이드 효과
        /// </summary>
        private void FadeController(GameObject controllerObj, bool fadeIn)
        {
            // 컨트롤러가 비활성화되어 있으면 활성화
            if (!controllerObj.activeInHierarchy)
            {
                controllerObj.SetActive(true);
            }
            
            // 페이드 시작 시 입력 비활성화
            SetHandInputEnabled(!isControllerFading);
            
            // 모든 렌더러 컴포넌트 찾기
            Renderer[] renderers = controllerObj.GetComponentsInChildren<Renderer>();
            
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"No renderers found on controller: {controllerObj.name}");
                isControllerFading = false;
                SetHandInputEnabled(true); // 입력 다시 활성화
                return;
            }
            
            // 현재 알파값 확인
            float startAlpha = fadeIn ? 0f : 1f;
            float targetAlpha = fadeIn ? 1f : 0f;
            
            // 모든 렌더러의 알파값을 시작값으로 설정
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        Color color = material.color;
                        color.a = startAlpha;
                        material.color = color;
                    }
                }
            }
            
            // DOTween으로 알파값 변경
            DOTween.To(() => startAlpha, alpha => {
                foreach (var renderer in renderers)
                {
                    foreach (var material in renderer.materials)
                    {
                        if (material.HasProperty("_Color"))
                        {
                            Color color = material.color;
                            color.a = alpha;
                            material.color = color;
                        }
                    }
                }
            }, targetAlpha, controllerFadeDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                // 페이드 완료 시 입력 다시 활성화
                SetHandInputEnabled(true);
                
                // 페이드 아웃 완료 시 오브젝트 비활성화
                if (!fadeIn)
                {
                    controllerObj.SetActive(false);
                }
                
                isControllerFading = false;
                Debug.Log($"Controller {controllerObj.name} fade {(fadeIn ? "in" : "out")} completed");
            });
        }
        
        /// <summary>
        /// 핸드 컨트롤러 입력 활성화/비활성화
        /// </summary>
        private void SetHandInputEnabled(bool enabled)
        {
            if (handControllers != null)
            {
                foreach (var hand in handControllers)
                {
                    if (hand != null)
                    {
                        hand.enabled = enabled;
                        // 추가적인 입력 비활성화 로직이 필요하면 여기에 추가
                        Debug.Log($"Hand {hand.name} input {(enabled ? "enabled" : "disabled")}");
                    }
                }
            }
        }
        
        /// <summary>
        /// 즉시 컨트롤러 활성화/비활성화 (페이드 없이)
        /// </summary>
        public void SetControllerEnabledImmediate(bool enabled)
        {
            isControllerEnabled = enabled;
            
            if (controllerObjects != null)
            {
                foreach (var controllerObj in controllerObjects)
                {
                    if (controllerObj != null)
                    {
                        controllerObj.SetActive(enabled);
                    }
                }
            }
            
            Debug.Log($"Player controller {(enabled ? "enabled" : "disabled")} immediately");
        }
        
        /// <summary>
        /// 이동 기능만 활성화/비활성화 (조이스틱 이동) (CutEvent에서 호출)
        /// </summary>
        public void SetPlayerMovementEnabled(bool enabled)
        {
            isMovementEnabled = enabled;
            
            if (autoHandPlayer != null)
            {
                autoHandPlayer.useMovement = enabled;
            }
            
            Debug.Log($"Player movement {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// 플레이어 상호작용 활성화/비활성화 (CutEvent에서 호출)
        /// </summary>
        public void SetPlayerInteractionEnabled(bool enabled)
        {
            isInteractionEnabled = enabled;
            
            // 상호작용은 컨트롤러와 동일하게 처리
            if (controllerObjects != null)
            {
                foreach (var controllerObj in controllerObjects)
                {
                    if (controllerObj != null)
                    {
                        controllerObj.SetActive(enabled);
                    }
                }
            }
            
            Debug.Log($"Player interaction {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// 플레이어 텔레포트 (CutEvent에서 호출)
        /// </summary>
        public void TeleportPlayer(Vector3 position)
        {
            if (player == null)
            {
                Debug.LogError("Player reference is null!");
                return;
            }
            
            // Rigidbody 속도 초기화
            if (playerRigidbody != null)
            {
                playerRigidbody.velocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
            
            // 위치 설정
            player.transform.position = position;
            
            Debug.Log($"Player teleported to: {position}");
        }
        
        /// <summary>
        /// 플레이어 회전 (CutEvent에서 호출)
        /// </summary>
        public void RotatePlayer(Vector3 eulerAngles)
        {
            if (player == null)
            {
                Debug.LogError("Player reference is null!");
                return;
            }
            
            player.transform.rotation = Quaternion.Euler(eulerAngles);
            Debug.Log($"Player rotated to: {eulerAngles}");
        }
        
        /// <summary>
        /// 플레이어 위치 및 회전 설정 (CutEvent에서 호출)
        /// </summary>
        public void SetPlayerTransform(Vector3 position, Vector3 eulerAngles)
        {
            TeleportPlayer(position);
            RotatePlayer(eulerAngles);
        }
        
        /// <summary>
        /// 자동이동 - Transform 이동 (사람의 걸음걸이 정도의 속도) (CutEvent에서 호출)
        /// </summary>
        public void AutoMovePlayer(Vector3[] waypoints)
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogError("Waypoints array is null or empty!");
                return;
            }
            
            if (isAutoMoving)
            {
                Debug.LogWarning("Player is already auto moving!");
                return;
            }
            
            StartCoroutine(AutoMoveCoroutine(waypoints));
        }
        
        /// <summary>
        /// 자동이동 - 단일 목적지 (CutEvent에서 호출)
        /// </summary>
        public void AutoMovePlayer(Vector3 destination)
        {
            AutoMovePlayer(new Vector3[] { destination });
        }
        
        /// <summary>
        /// 자동이동 중지 (CutEvent에서 호출)
        /// </summary>
        public void StopAutoMove()
        {
            if (isAutoMoving)
            {
                StopAllCoroutines();
                isAutoMoving = false;
                Debug.Log("Auto move stopped");
            }
        }
        
        private IEnumerator AutoMoveCoroutine(Vector3[] waypoints)
        {
            isAutoMoving = true;
            
            Debug.Log($"Starting auto move with {waypoints.Length} waypoints");
            
            foreach (var waypoint in waypoints)
            {
                yield return StartCoroutine(MoveToPosition(waypoint));
            }
            
            isAutoMoving = false;
            
            Debug.Log("Auto move completed");
        }
        
        private IEnumerator MoveToPosition(Vector3 targetPosition)
        {
            if (player == null || playerRigidbody == null)
            {
                Debug.LogError("Player or Rigidbody is null!");
                yield break;
            }
            
            Vector3 startPosition = player.transform.position;
            Vector3 direction = (targetPosition - startPosition).normalized;
            float distance = Vector3.Distance(startPosition, targetPosition);
            
            // 목표 지점을 향해 회전
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                while (Quaternion.Angle(player.transform.rotation, targetRotation) > 1f && isAutoMoving)
                {
                    player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    yield return null;
                }
            }
            
            // Rigidbody로 이동
            while (Vector3.Distance(player.transform.position, targetPosition) > 0.1f && isAutoMoving)
            {
                Vector3 currentDirection = (targetPosition - player.transform.position).normalized;
                Vector3 targetVelocity = currentDirection * walkSpeed;
                
                // 현재 속도를 목표 속도로 부드럽게 변경
                Vector3 velocityChange = targetVelocity - new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
                playerRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
                
                yield return new WaitForFixedUpdate();
            }
            
            // 정확한 위치로 설정하고 속도 초기화
            player.transform.position = targetPosition;
            playerRigidbody.velocity = new Vector3(0, playerRigidbody.velocity.y, 0); // Y축 속도는 유지 (중력)
        }
        
        /// <summary>
        /// 플레이어 완전 제어 (이동 + 상호작용 + 컨트롤러) (CutEvent에서 호출)
        /// </summary>
        public void SetPlayerControl(bool movementEnabled, bool interactionEnabled, bool controllerEnabled)
        {
            SetPlayerMovementEnabled(movementEnabled);
            SetPlayerInteractionEnabled(interactionEnabled);
            SetControllerEnabled(controllerEnabled);
        }
        
        /// <summary>
        /// 현재 플레이어 상태 반환
        /// </summary>
        public bool IsMovementEnabled => isMovementEnabled;
        public bool IsInteractionEnabled => isInteractionEnabled;
        public bool IsControllerEnabled => isControllerEnabled;
        public bool IsAutoMoving => isAutoMoving;
        public bool IsControllerFading => isControllerFading;
        
    }
} 