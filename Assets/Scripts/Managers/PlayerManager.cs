using UnityEngine;

namespace TheFeelies.Managers
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Player References")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private GameObject playerObject;
        [SerializeField] private Transform xrOrigin; // XR Origin Transform
        
        [Header("Player Settings")]
        [SerializeField] private float teleportDuration = 0.5f;
        [SerializeField] private bool useSmoothTeleport = true;
        
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        
        private void Awake()
        {
            InitializePlayer();
        }
        
        private void InitializePlayer()
        {
            if (playerTransform == null)
            {
                // XR Origin 찾기
                GameObject xrOriginGO = GameObject.Find("XR Origin");
                if (xrOriginGO != null)
                {
                    xrOrigin = xrOriginGO.transform;
                    playerTransform = xrOrigin;
                    playerObject = xrOriginGO;
                }
                else
                {
                    // 일반 플레이어 찾기
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        playerTransform = player.transform;
                        playerObject = player;
                    }
                }
            }
            
            if (playerTransform != null)
            {
                originalPosition = playerTransform.position;
                originalRotation = playerTransform.rotation;
            }
        }
        
        public void TeleportPlayer(Vector3 position, Quaternion rotation)
        {
            if (playerTransform == null)
            {
                Debug.LogError("플레이어 Transform이 설정되지 않았습니다!");
                return;
            }
            
            if (useSmoothTeleport)
            {
                StartCoroutine(SmoothTeleport(position, rotation));
            }
            else
            {
                playerTransform.position = position;
                playerTransform.rotation = rotation;
                Debug.Log($"플레이어 텔레포트: {position}");
            }
        }
        
        private System.Collections.IEnumerator SmoothTeleport(Vector3 targetPosition, Quaternion targetRotation)
        {
            Vector3 startPosition = playerTransform.position;
            Quaternion startRotation = playerTransform.rotation;
            float elapsedTime = 0f;
            
            while (elapsedTime < teleportDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / teleportDuration;
                
                // 부드러운 보간
                playerTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
                playerTransform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                
                yield return null;
            }
            
            // 정확한 위치로 설정
            playerTransform.position = targetPosition;
            playerTransform.rotation = targetRotation;
            
            Debug.Log($"플레이어 부드러운 텔레포트 완료: {targetPosition}");
        }
        
        public void ResetPlayerPosition()
        {
            if (playerTransform != null)
            {
                TeleportPlayer(originalPosition, originalRotation);
            }
        }
        
        public void SetPlayerPosition(Vector3 position)
        {
            if (playerTransform != null)
            {
                playerTransform.position = position;
            }
        }
        
        public void SetPlayerRotation(Quaternion rotation)
        {
            if (playerTransform != null)
            {
                playerTransform.rotation = rotation;
            }
        }
        
        public Vector3 GetPlayerPosition()
        {
            return playerTransform != null ? playerTransform.position : Vector3.zero;
        }
        
        public Quaternion GetPlayerRotation()
        {
            return playerTransform != null ? playerTransform.rotation : Quaternion.identity;
        }
        
        public void SetTeleportDuration(float duration)
        {
            teleportDuration = Mathf.Max(0.1f, duration);
        }
        
        public void SetUseSmoothTeleport(bool useSmooth)
        {
            useSmoothTeleport = useSmooth;
        }
        
        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
            if (player != null)
            {
                playerObject = player.gameObject;
                originalPosition = player.position;
                originalRotation = player.rotation;
            }
        }
        
        public void SetXROrigin(Transform origin)
        {
            xrOrigin = origin;
            if (origin != null)
            {
                playerTransform = origin;
                playerObject = origin.gameObject;
            }
        }
        
        public bool IsPlayerInitialized()
        {
            return playerTransform != null;
        }
        
        public Transform GetPlayerTransform()
        {
            return playerTransform;
        }
        
        public GameObject GetPlayerObject()
        {
            return playerObject;
        }
    }
} 