using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace TheFeelies.Managers
{
    public class AnimationManager : MonoBehaviour
    {
        [Header("Character Animators")]
        [SerializeField] private List<Animator> characterAnimators = new List<Animator>();
        [SerializeField] private List<string> characterNames = new List<string>();
        
        [Header("Animation Settings")]
        [SerializeField] private float crossFadeDuration = 0.25f;
        
        private Dictionary<string, Animator> characterAnimatorMap = new Dictionary<string, Animator>();
        
        private void Awake()
        {
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
                // 애니메이션 클립을 직접 재생
                animator.Play(animationClip.name, 0, 0f);
                
                Debug.Log($"캐릭터 '{characterName}' 애니메이션 재생: {animationClip.name}");
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