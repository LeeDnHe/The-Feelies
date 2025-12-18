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
        [SerializeField] private AudioPlayType audioPlayType = AudioPlayType.Dialog;
        [SerializeField] private bool loopAudio = false;
        
        [Header("Player Control Events")]
        [SerializeField] private bool enablePlayerControl = true;
        
        [Header("Teleport Events")]
        [SerializeField] private Vector3 teleportPosition = Vector3.zero;
        [SerializeField] private bool shouldTeleport = false;
        
        [Header("DOTween Path Mover Events")]
        [SerializeField] private PlayerDotweenMover playerDotweenMover;
        [SerializeField] private DotweenPathMover dotweenPathMover;
        
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
                    
                case CutEventType.PlayerControl:
                    ExecutePlayerControl();
                    break;
                    
                case CutEventType.PlayPlayerDotweenPath:
                    ExecutePlayPlayerDotweenPath();
                    break;
                    
                case CutEventType.PlayDotweenPath:
                    ExecutePlayDotweenPath();
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
            
            // 싱글톤 매니저들에서 메서드 찾기 및 호출
            MonoBehaviour[] managers = new MonoBehaviour[]
            {
                AnimationManager.Instance,
                SoundManager.Instance,
                PlayerManager.Instance,
                AudioManager.Instance
            };
            
            foreach (var manager in managers)
            {
                if (manager == null) continue;
                
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
                    Debug.Log($"Manager method '{managerMethodName}' executed on {manager.GetType().Name}");
                    return;
                }
            }
            
            Debug.LogError($"Manager method '{managerMethodName}' not found in any manager!");
        }
        
        private void ExecutePlayAnimation()
        {
            if (animationClip == null)
            {
                Debug.LogError("Animation clip is not set!");
                return;
            }
            
            if (string.IsNullOrEmpty(targetActorId))
            {
                Debug.LogError("Target actor ID is not set!");
                return;
            }
            
            // AnimationManager를 통해 애니메이션 재생
            if (AnimationManager.Instance == null)
            {
                Debug.LogError("AnimationManager instance not found!");
                return;
            }
            
            AnimationManager.Instance.PlayCharacterAnimation(targetActorId, animationClip);
            Debug.Log($"Animation played via AnimationManager: {animationClip.name} for actor: {targetActorId}");
        }
        
        private void ExecutePlayAudio()
        {
            if (audioClip == null)
            {
                Debug.LogError("Audio clip is not set!");
                return;
            }
            
            if (SoundManager.Instance == null)
            {
                Debug.LogError("SoundManager instance not found!");
                return;
            }
            
            switch (audioPlayType)
            {
                case AudioPlayType.Dialog:
                    SoundManager.Instance.PlayDialog(audioClip);
                    Debug.Log($"대화 재생: {audioClip.name}");
                    break;
                    
                case AudioPlayType.BackgroundMusic:
                    SoundManager.Instance.PlayMusic(audioClip);
                    Debug.Log($"배경음악 재생: {audioClip.name}");
                    break;
                    
                case AudioPlayType.SFX:
                    SoundManager.Instance.PlaySFX(audioClip, loopAudio);
                    Debug.Log($"효과음 재생: {audioClip.name} (Loop: {loopAudio})");
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown audio play type: {audioPlayType}");
                    break;
            }
        }
        
        
        private void ExecuteTeleportPlayer()
        {
            if (!shouldTeleport) return;
            
            if (PlayerManager.Instance == null)
            {
                Debug.LogError("PlayerManager instance not found!");
                return;
            }
            
            PlayerManager.Instance.TeleportPlayer(teleportPosition);
        }
        
        private void ExecuteChangeBackgroundMusic()
        {
            if (audioClip == null)
            {
                Debug.LogError("Background music audio clip is not set!");
                return;
            }
            
            if (AudioManager.Instance == null)
            {
                Debug.LogError("AudioManager instance not found!");
                return;
            }
            
            AudioManager.Instance.ChangeBackgroundMusic(audioClip);
        }
        
        private void ExecuteChangeScene()
        {
            // 씬 변경 로직 (SceneManager 사용)
            if (!string.IsNullOrEmpty(managerMethodName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(managerMethodName);
            }
        }
        
        private void ExecutePlayerControl()
        {
            if (PlayerManager.Instance == null)
            {
                Debug.LogError("PlayerManager instance not found!");
                return;
            }
            
            PlayerManager.Instance.SetPlayerControlEnabled(enablePlayerControl);
        }
        
        private void ExecutePlayPlayerDotweenPath()
        {
            if (playerDotweenMover == null)
            {
                Debug.LogError("PlayerDotweenMover is not set!");
                return;
            }
            
            playerDotweenMover.PlayPath();
            Debug.Log($"Player DOTween path started: {eventName}");
        }
        
        private void ExecutePlayDotweenPath()
        {
            if (dotweenPathMover == null)
            {
                Debug.LogError("DotweenPathMover is not set!");
                return;
            }
            
            dotweenPathMover.PlayPath();
            Debug.Log($"DOTween path started: {eventName}");
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
        PlayerControl,          // 플레이어 제어 (컨트롤러 + 이동)
        PlayPlayerDotweenPath,  // 플레이어 DOTween 경로 이동
        PlayDotweenPath         // 일반 오브젝트 DOTween 경로 이동
    }
    
    /// <summary>
    /// 오디오 재생 타입 열거형
    /// </summary>
    public enum AudioPlayType
    {
        Dialog,                 // 대화 재생
        BackgroundMusic,        // 배경음악 재생
        SFX                     // 효과음 재생
    }
}
