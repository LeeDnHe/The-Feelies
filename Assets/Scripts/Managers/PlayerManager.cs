using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.XR;

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
        
        [Header("Player State")]
        [SerializeField] private bool isMovementEnabled = true;
        [SerializeField] private bool isControllerEnabled = true;
        [SerializeField] private bool isControllerFading = false;
        
        [Header("Haptic Settings")]
        [SerializeField] private bool enableHaptics = true;
        
        private static PlayerManager instance;
        public static PlayerManager Instance => instance;
        
        // 햅틱용 디바이스 리스트
        private List<InputDevice> leftDevices = new List<InputDevice>();
        private List<InputDevice> rightDevices = new List<InputDevice>();
        
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
        /// 플레이어 제어 활성화/비활성화 (컨트롤러 입력만, DOTween 이동은 허용) (CutEvent에서 호출)
        /// </summary>
        public void SetPlayerControlEnabled(bool enabled)
        {
            if (isControllerFading) return; // 이미 페이딩 중이면 무시
            
            isControllerEnabled = enabled;
            isMovementEnabled = enabled;
            isControllerFading = true;
            
            // 조이스틱 입력만 제어 (DOTween을 통한 외부 이동은 허용)
            // autoHandPlayer.useMovement를 건드리지 않음
            
            // 컨트롤러 페이드
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
            
            Debug.Log($"Player control {(enabled ? "enabling" : "disabling")} (controller input only, DOTween movement allowed)");
        }
        
        /// <summary>
        /// 컨트롤러 활성화/비활성화 (하위 호환성을 위해 유지)
        /// </summary>
        public void SetControllerEnabled(bool enabled)
        {
            SetPlayerControlEnabled(enabled);
        }
        
        /// <summary>
        /// 컨트롤러 페이드 효과 - 렌더러를 켜고 끄는 방식으로 구현
        /// </summary>
        private void FadeController(GameObject controllerObj, bool fadeIn)
        {
            // 페이드 시작 시 입력 비활성화
            SetHandInputEnabled(false);
            
            // 모든 렌더러 컴포넌트 찾기 (MeshRenderer + SkinnedMeshRenderer)
            Renderer[] renderers = controllerObj.GetComponentsInChildren<Renderer>(true);
            
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"No renderers found on controller: {controllerObj.name}");
                isControllerFading = false;
                SetHandInputEnabled(true); // 입력 다시 활성화
                return;
            }
            
            Debug.Log($"Found {renderers.Length} renderers on {controllerObj.name}");
            
            if (fadeIn)
            {
                // 페이드 인: 렌더러를 켜고 딜레이 후 입력 활성화
                foreach (var renderer in renderers)
                {
                    renderer.enabled = true;
                }
                
                DOVirtual.DelayedCall(controllerFadeDuration, () => {
                    SetHandInputEnabled(true);
                    isControllerFading = false;
                    Debug.Log($"Controller {controllerObj.name} fade in completed");
                });
            }
            else
            {
                // 페이드 아웃: 딜레이 후 렌더러를 끄고 입력 활성화
                DOVirtual.DelayedCall(controllerFadeDuration, () => {
                    foreach (var renderer in renderers)
                    {
                        renderer.enabled = false;
                    }
                    
                    SetHandInputEnabled(true);
                    isControllerFading = false;
                    Debug.Log($"Controller {controllerObj.name} fade out completed");
                });
            }
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
        /// 즉시 플레이어 제어 활성화/비활성화 (컨트롤러 입력만, 페이드 없이, DOTween 이동은 허용)
        /// </summary>
        public void SetPlayerControlEnabledImmediate(bool enabled)
        {
            isControllerEnabled = enabled;
            isMovementEnabled = enabled;
            
            // 조이스틱 입력만 제어 (DOTween을 통한 외부 이동은 허용)
            // autoHandPlayer.useMovement를 건드리지 않음
            
            // 핸드 컨트롤러 입력 즉시 활성화/비활성화
            SetHandInputEnabled(enabled);
            
            // 컨트롤러 렌더러 즉시 활성화/비활성화
            if (controllerObjects != null)
            {
                foreach (var controllerObj in controllerObjects)
                {
                    if (controllerObj != null)
                    {
                        Renderer[] renderers = controllerObj.GetComponentsInChildren<Renderer>(true);
                        foreach (var renderer in renderers)
                        {
                            renderer.enabled = enabled;
                        }
                    }
                }
            }
            
            Debug.Log($"Player control {(enabled ? "enabled" : "disabled")} immediately (controller input only, DOTween movement allowed)");
        }
        
        /// <summary>
        /// 즉시 컨트롤러 활성화/비활성화 (하위 호환성을 위해 유지)
        /// </summary>
        public void SetControllerEnabledImmediate(bool enabled)
        {
            SetPlayerControlEnabledImmediate(enabled);
        }
        
        /// <summary>
        /// 이동 기능만 활성화/비활성화 (하위 호환성을 위해 유지)
        /// </summary>
        public void SetPlayerMovementEnabled(bool enabled)
        {
            SetPlayerControlEnabled(enabled);
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
        /// 현재 플레이어 상태 반환
        /// </summary>
        public bool IsMovementEnabled => isMovementEnabled;
        public bool IsControllerEnabled => isControllerEnabled;
        public bool IsControllerFading => isControllerFading;
        
        // ===== 햅틱 피드백 시스템 =====
        
        /// <summary>
        /// 왼손 컨트롤러에 햅틱 피드백 전송
        /// </summary>
        public void SendHapticToLeftHand(float intensity, float duration)
        {
            SendHapticToHand(XRNode.LeftHand, intensity, duration);
        }
        
        /// <summary>
        /// 오른손 컨트롤러에 햅틱 피드백 전송
        /// </summary>
        public void SendHapticToRightHand(float intensity, float duration)
        {
            SendHapticToHand(XRNode.RightHand, intensity, duration);
        }
        
        /// <summary>
        /// 양손 컨트롤러에 햅틱 피드백 전송
        /// </summary>
        public void SendHapticToBothHands(float intensity, float duration)
        {
            SendHapticToLeftHand(intensity, duration);
            SendHapticToRightHand(intensity, duration);
        }
        
        /// <summary>
        /// 특정 손에 햅틱 피드백 전송 (XRHandControllerLink 방식)
        /// </summary>
        private void SendHapticToHand(XRNode handNode, float intensity, float duration)
        {
            if (!enableHaptics)
            {
                return;
            }
            
            // 디바이스 목록 가져오기
            List<InputDevice> devices = (handNode == XRNode.LeftHand) ? leftDevices : rightDevices;
            InputDevices.GetDevicesAtXRNode(handNode, devices);
            
            if (devices.Count == 0)
            {
                Debug.LogWarning($"No XR devices found for {handNode}");
                return;
            }
            
            // 햅틱 전송
            foreach(var device in devices)
            {
                if(device.TryGetHapticCapabilities(out var capabilities) && capabilities.supportsImpulse)
                {
                    device.SendHapticImpulse(0u, intensity, duration);
                    Debug.Log($"Haptic feedback sent to {handNode}: intensity={intensity}, duration={duration}");
                }
            }
        }
        
        /// <summary>
        /// 햅틱 활성화/비활성화 설정
        /// </summary>
        public void SetHapticsEnabled(bool enabled)
        {
            enableHaptics = enabled;
            Debug.Log($"Haptics {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// 햅틱 활성화 상태 반환
        /// </summary>
        public bool IsHapticsEnabled => enableHaptics;
        
    }
} 