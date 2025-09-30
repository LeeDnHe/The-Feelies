using UnityEngine;
using UnityEngine.Events;
using TheFeelies.Managers;

namespace TheFeelies.Core
{
    /// <summary>
    /// 컷 내에서 실행되는 개별 이벤트를 정의하는 클래스
    /// 매니저들과 연동하여 다양한 기능을 실행할 수 있음
    /// </summary>
    [System.Serializable]
    public class CutEvent
    {
        [Header("Event Settings")]
        [SerializeField] private string eventName;
        [SerializeField] private string description;
        [SerializeField] private float delay = 0f;
        [SerializeField] private float waitAfterExecution = 0f;
        
        [Header("Event Type")]
        [SerializeField] private CutEventType eventType = CutEventType.UnityEvent;
        
        [Header("Unity Event")]
        [SerializeField] private UnityEvent onEventExecute;
        
        [Header("Manager Events")]
        [SerializeField] private string managerMethodName;
        [SerializeField] private string[] parameters;
        
        [Header("Animation Events")]
        [SerializeField] private AnimationClip animationClip;
        [SerializeField] private string targetActorId;
        
        [Header("Audio Events")]
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private bool playAsBackgroundMusic = false;
        
        [Header("Controller Events")]
        [SerializeField] private bool enableController = true;
        
        [Header("Movement Events")]
        [SerializeField] private bool enableMovement = true;
        
        [Header("Teleport Events")]
        [SerializeField] private Vector3 teleportPosition = Vector3.zero;
        [SerializeField] private bool shouldTeleport = false;
        
        [Header("Auto Move Events")]
        [SerializeField] private Vector3[] waypoints = new Vector3[0];
        [SerializeField] private Vector3 singleDestination = Vector3.zero;
        [SerializeField] private bool useWaypoints = false;
        
        public string EventName => eventName;
        public float Delay => delay;
        public float WaitAfterExecution => waitAfterExecution;
        public CutEventType EventType => eventType;
        
        /// <summary>
        /// 이벤트 실행
        /// </summary>
        public void ExecuteEvent()
        {
            Debug.Log($"Executing CutEvent: {eventName} (Type: {eventType})");
            
            switch (eventType)
            {
                case CutEventType.UnityEvent:
                    ExecuteUnityEvent();
                    break;
                    
                case CutEventType.ManagerMethod:
                    ExecuteManagerMethod();
                    break;
                    
                case CutEventType.PlayAnimation:
                    ExecutePlayAnimation();
                    break;
                    
                case CutEventType.PlayAudio:
                    ExecutePlayAudio();
                    break;
                    
                case CutEventType.TeleportPlayer:
                    ExecuteTeleportPlayer();
                    break;
                    
                case CutEventType.ChangeBackgroundMusic:
                    ExecuteChangeBackgroundMusic();
                    break;
                    
                case CutEventType.ChangeScene:
                    ExecuteChangeScene();
                    break;
                    
                case CutEventType.ControllerControl:
                    ExecuteControllerControl();
                    break;
                    
                case CutEventType.MovementControl:
                    ExecuteMovementControl();
                    break;
                    
                case CutEventType.AutoMovePlayer:
                    ExecuteAutoMovePlayer();
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown event type: {eventType}");
                    break;
            }
        }
        
        private void ExecuteUnityEvent()
        {
            Debug.Log($"Executing UnityEvent for: {eventName}");
            Debug.Log($"UnityEvent has {onEventExecute?.GetPersistentEventCount() ?? 0} listeners");
            
            if (onEventExecute != null)
            {
                onEventExecute.Invoke();
                Debug.Log($"UnityEvent executed successfully for: {eventName}");
            }
            else
            {
                Debug.LogWarning($"UnityEvent is null for: {eventName}");
            }
        }
        
        private void ExecuteManagerMethod()
        {
            if (string.IsNullOrEmpty(managerMethodName))
            {
                Debug.LogError("Manager method name is not set!");
                return;
            }
            
            // 매니저 찾기 및 메서드 호출
            var managers = Object.FindObjectsOfType<MonoBehaviour>();
            foreach (var manager in managers)
            {
                var method = manager.GetType().GetMethod(managerMethodName);
                if (method != null)
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        method.Invoke(manager, new object[] { parameters });
                    }
                    else
                    {
                        method.Invoke(manager, null);
                    }
                    return;
                }
            }
            
            Debug.LogError($"Manager method '{managerMethodName}' not found!");
        }
        
        private void ExecutePlayAnimation()
        {
            if (animationClip == null)
            {
                Debug.LogError("Animation clip is not set!");
                return;
            }
            
            // 타겟 액터 찾기
            GameObject targetActor = null;
            if (!string.IsNullOrEmpty(targetActorId))
            {
                targetActor = GameObject.Find(targetActorId);
            }
            
            if (targetActor == null)
            {
                Debug.LogError($"Target actor '{targetActorId}' not found!");
                return;
            }
            
            // 애니메이션 실행
            var animator = targetActor.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play(animationClip.name);
            }
            else
            {
                Debug.LogError($"Animator component not found on {targetActor.name}!");
            }
        }
        
        private void ExecutePlayAudio()
        {
            if (audioClip == null)
            {
                Debug.LogError("Audio clip is not set!");
                return;
            }
            
            if (playAsBackgroundMusic)
            {
                // 배경음악으로 재생
                var audioManager = Object.FindObjectOfType<AudioManager>();
                if (audioManager != null)
                {
                    audioManager.PlayBackgroundMusic(audioClip);
                }
                else
                {
                    Debug.LogError("AudioManager not found!");
                }
            }
            else
            {
                // 효과음으로 재생
                AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position);
            }
        }
        
        
        private void ExecuteTeleportPlayer()
        {
            if (!shouldTeleport) return;
            
            var playerManager = Object.FindObjectOfType<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.TeleportPlayer(teleportPosition);
            }
            else
            {
                Debug.LogError("PlayerManager not found!");
            }
        }
        
        private void ExecuteChangeBackgroundMusic()
        {
            if (audioClip == null)
            {
                Debug.LogError("Background music audio clip is not set!");
                return;
            }
            
            var audioManager = Object.FindObjectOfType<AudioManager>();
            if (audioManager != null)
            {
                audioManager.ChangeBackgroundMusic(audioClip);
            }
            else
            {
                Debug.LogError("AudioManager not found!");
            }
        }
        
        private void ExecuteChangeScene()
        {
            // 씬 변경 로직 (SceneManager 사용)
            if (!string.IsNullOrEmpty(managerMethodName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(managerMethodName);
            }
        }
        
        private void ExecuteControllerControl()
        {
            var playerManager = Object.FindObjectOfType<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.SetControllerEnabled(enableController);
            }
            else
            {
                Debug.LogError("PlayerManager not found!");
            }
        }
        
        private void ExecuteMovementControl()
        {
            var playerManager = Object.FindObjectOfType<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.SetPlayerMovementEnabled(enableMovement);
            }
            else
            {
                Debug.LogError("PlayerManager not found!");
            }
        }
        
        private void ExecuteAutoMovePlayer()
        {
            var playerManager = Object.FindObjectOfType<PlayerManager>();
            if (playerManager != null)
            {
                if (useWaypoints && waypoints != null && waypoints.Length > 0)
                {
                    playerManager.AutoMovePlayer(waypoints);
                }
                else if (singleDestination != Vector3.zero)
                {
                    playerManager.AutoMovePlayer(singleDestination);
                }
                else
                {
                    Debug.LogError("No valid destination set for auto move!");
                }
            }
            else
            {
                Debug.LogError("PlayerManager not found!");
            }
        }
    }
    
    /// <summary>
    /// 컷 이벤트 타입 열거형
    /// </summary>
    public enum CutEventType
    {
        UnityEvent,              // UnityEvent 실행
        ManagerMethod,           // 매니저 메서드 호출
        PlayAnimation,           // 애니메이션 재생
        PlayAudio,              // 오디오 재생
        TeleportPlayer,         // 플레이어 텔레포트
        ChangeBackgroundMusic,  // 배경음악 변경
        ChangeScene,            // 씬 변경
        ControllerControl,      // 컨트롤러 전체 (이동 + 상호작용)
        MovementControl,        // 이동 기능만
        AutoMovePlayer          // 자동이동 (경유지 포함)
    }
}
