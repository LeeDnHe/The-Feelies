using UnityEngine;
using System.Collections.Generic;

namespace TheFeelies.Managers
{
    public class VFXManager : MonoBehaviour
    {
        private static VFXManager instance;
        public static VFXManager Instance => instance;
        
        [Header("VFX Prefabs")]
        [SerializeField] private List<GameObject> vfxPrefabs = new List<GameObject>();
        [SerializeField] private List<string> vfxNames = new List<string>();
        
        [Header("VFX Settings")]
        [SerializeField] private Transform vfxParent;
        [SerializeField] private bool autoDestroyVFX = true;
        [SerializeField] private float defaultVFXDuration = 3f;
        
        private Dictionary<string, GameObject> vfxPrefabMap = new Dictionary<string, GameObject>();
        private List<GameObject> activeVFX = new List<GameObject>();
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeVFXMap();
            
            if (vfxParent == null)
            {
                GameObject vfxParentGO = new GameObject("VFXParent");
                vfxParentGO.transform.SetParent(transform);
                vfxParent = vfxParentGO.transform;
            }
        }
        
        private void InitializeVFXMap()
        {
            vfxPrefabMap.Clear();
            
            for (int i = 0; i < vfxNames.Count && i < vfxPrefabs.Count; i++)
            {
                if (!string.IsNullOrEmpty(vfxNames[i]) && vfxPrefabs[i] != null)
                {
                    vfxPrefabMap[vfxNames[i]] = vfxPrefabs[i];
                }
            }
        }
        
        public void PlayVFX(string vfxName, Vector3 position, Quaternion rotation = default)
        {
            if (vfxPrefabMap.TryGetValue(vfxName, out GameObject vfxPrefab))
            {
                GameObject vfxInstance = Instantiate(vfxPrefab, position, rotation, vfxParent);
                activeVFX.Add(vfxInstance);
                
                if (autoDestroyVFX)
                {
                    Destroy(vfxInstance, defaultVFXDuration);
                }
                
                Debug.Log($"VFX 재생: {vfxName} at {position}");
            }
            else
            {
                Debug.LogWarning($"VFX '{vfxName}'를 찾을 수 없습니다.");
            }
        }
        
        public void PlayVFX(string vfxName, Transform target)
        {
            if (target != null)
            {
                PlayVFX(vfxName, target.position, target.rotation);
            }
        }
        
        public void StopVFX(string vfxName)
        {
            for (int i = activeVFX.Count - 1; i >= 0; i--)
            {
                if (activeVFX[i] != null && activeVFX[i].name.Contains(vfxName))
                {
                    Destroy(activeVFX[i]);
                    activeVFX.RemoveAt(i);
                }
            }
        }
        
        public void StopAllVFX()
        {
            foreach (var vfx in activeVFX)
            {
                if (vfx != null)
                {
                    Destroy(vfx);
                }
            }
            activeVFX.Clear();
            Debug.Log("모든 VFX 정지");
        }
        
        public void SetVFXDuration(float duration)
        {
            defaultVFXDuration = Mathf.Max(0, duration);
        }
        
        public void SetAutoDestroyVFX(bool autoDestroy)
        {
            autoDestroyVFX = autoDestroy;
        }
        
        public void AddVFXPrefab(string name, GameObject prefab)
        {
            if (!string.IsNullOrEmpty(name) && prefab != null)
            {
                vfxPrefabMap[name] = prefab;
                Debug.Log($"VFX 프리팹 추가: {name}");
            }
        }
        
        public void RemoveVFXPrefab(string name)
        {
            if (vfxPrefabMap.ContainsKey(name))
            {
                vfxPrefabMap.Remove(name);
                Debug.Log($"VFX 프리팹 제거: {name}");
            }
        }
        
        public bool IsVFXPlaying(string vfxName)
        {
            foreach (var vfx in activeVFX)
            {
                if (vfx != null && vfx.name.Contains(vfxName))
                {
                    return true;
                }
            }
            return false;
        }
        
        public int GetActiveVFXCount()
        {
            return activeVFX.Count;
        }
        
        private void Update()
        {
            // 파괴된 VFX 정리
            for (int i = activeVFX.Count - 1; i >= 0; i--)
            {
                if (activeVFX[i] == null)
                {
                    activeVFX.RemoveAt(i);
                }
            }
        }
    }
} 