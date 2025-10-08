using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace TheFeelies.Managers
{
    public class AnimationManager : MonoBehaviour
    {
        private static AnimationManager instance;
        public static AnimationManager Instance => instance;
        
        [Header("Character Animators")]
        [SerializeField] private List<Animator> characterAnimators = new List<Animator>();
        [SerializeField] private List<string> characterNames = new List<string>();
        
        [Header("Animation Settings")]
        [SerializeField] private float crossFadeDuration = 0.25f;
        
        private Dictionary<string, Animator> characterAnimatorMap = new Dictionary<string, Animator>();
        
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
            
            InitializeCharacterMap();
        }
        
        private void InitializeCharacterMap()
        {
            characterAnimatorMap.Clear();
            
            for (int i = 0; i < characterNames.Count && i < characterAnimators.Count; i++)
            {
                if (!string.IsNullOrEmpty(characterNames[i]) && characterAnimators[i] != null)
                {
                    characterAnimatorMap[characterNames[i]] = characterAnimators[i];
                }
            }
        }
        
        public void PlayCharacterAnimations(List<AnimationClip> animations, List<string> characterNames)
        {
            if (animations == null || characterNames == null)
                return;
                
            for (int i = 0; i < animations.Count && i < characterNames.Count; i++)
            {
                PlayCharacterAnimation(characterNames[i], animations[i]);
            }
        }
        
        public void PlayCharacterAnimation(string characterName, AnimationClip animationClip)
        {
            if (string.IsNullOrEmpty(characterName) || animationClip == null)
                return;
                
            if (characterAnimatorMap.TryGetValue(characterName, out Animator animator))
            {
                if (animator == null || animator.runtimeAnimatorController == null)
                {
                    Debug.LogError($"캐릭터 '{characterName}'의 Animator 또는 RuntimeAnimatorController가 null입니다!");
                    return;
                }
                
                // 먼저 RuntimeAnimatorController의 AnimationClip 목록에서 찾기
                var clips = animator.runtimeAnimatorController.animationClips;
                bool clipFound = false;
                
                if (clips != null)
                {
                    foreach (var clip in clips)
                    {
                        if (clip.name == animationClip.name)
                        {
                            clipFound = true;
                            break;
                        }
                    }
                }
                
                if (clipFound)
                {
                    // AnimationClip이 존재하면 CrossFade로 재생 (State가 없어도 작동)
                    animator.CrossFade(animationClip.name, 0.1f, 0, 0f);
                    Debug.Log($"✓ 캐릭터 '{characterName}' 애니메이션 재생: {animationClip.name}");
                }
                else
                {
                    // AnimationClip을 찾을 수 없음 - 사용 가능한 Clip 목록 출력
                    Debug.LogError($"✗ 캐릭터 '{characterName}'의 Animator Controller에 AnimationClip '{animationClip.name}'가 존재하지 않습니다!");
                    Debug.LogWarning("=== 사용 가능한 Animation Clip 목록 ===");
                    
                    if (clips != null && clips.Length > 0)
                    {
                        for (int i = 0; i < clips.Length; i++)
                        {
                            Debug.Log($"  [{i}] {clips[i].name}");
                        }
                        Debug.LogWarning("=================================");
                        Debug.LogWarning($"Unity Inspector에서 CutEvent의 Animation Clip을 위 목록 중 하나로 설정해주세요.");
                    }
                    else
                    {
                        Debug.LogError("RuntimeAnimatorController에 AnimationClip이 없습니다!");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"캐릭터 '{characterName}'의 Animator를 찾을 수 없습니다.");
            }
        }
        
        public void StopCharacterAnimation(string characterName)
        {
            if (characterAnimatorMap.TryGetValue(characterName, out Animator animator))
            {
                animator.SetTrigger("Idle");
                Debug.Log($"캐릭터 '{characterName}' 애니메이션 정지");
            }
        }
        
        public void SetCharacterAnimator(string characterName, Animator animator)
        {
            if (!string.IsNullOrEmpty(characterName) && animator != null)
            {
                characterAnimatorMap[characterName] = animator;
                Debug.Log($"캐릭터 '{characterName}' Animator 설정됨");
            }
        }
        
        public void RemoveCharacterAnimator(string characterName)
        {
            if (characterAnimatorMap.ContainsKey(characterName))
            {
                characterAnimatorMap.Remove(characterName);
                Debug.Log($"캐릭터 '{characterName}' Animator 제거됨");
            }
        }
        
        public Transform GetCharacterTransform(string characterName)
        {
            if (characterAnimatorMap.TryGetValue(characterName, out Animator animator) && animator != null)
            {
                return animator.transform;
            }
            return null;
        }
        
        public bool IsCharacterPlayingAnimation(string characterName)
        {
            if (characterAnimatorMap.TryGetValue(characterName, out Animator animator))
            {
                return animator.GetCurrentAnimatorStateInfo(0).length > 0;
            }
            return false;
        }
        
        public void SetCrossFadeDuration(float duration)
        {
            crossFadeDuration = Mathf.Max(0, duration);
        }
        
        public void ResetAllAnimations()
        {
            foreach (var animator in characterAnimatorMap.Values)
            {
                if (animator != null)
                {
                    animator.SetTrigger("Idle");
                }
            }
            Debug.Log("모든 캐릭터 애니메이션 리셋");
        }
    }
} 