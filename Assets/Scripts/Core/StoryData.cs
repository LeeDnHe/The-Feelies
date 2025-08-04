using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheFeelies.Core
{
    [Serializable]
    public class Cut
    {
        [Header("Cut Settings")]
        public string cutID;
        public string cutName;
        public string description;
        
        [Header("Content")]
        public string dialogueText;
        public AudioClip ttsAudio;
        public List<AnimationClip> characterAnimations = new List<AnimationClip>();
        public List<string> characterNames = new List<string>();
        
        [Header("Player Position")]
        public PlayerPositionType playerPositionType = PlayerPositionType.MaintainPosition;
        public Transform spawnPoint;
        public Vector3 customPosition = Vector3.zero;
        public Quaternion customRotation = Quaternion.identity;
        
        [Header("Conditions")]
        public CutStartCondition startCondition = CutStartCondition.Automatic;
        public string requiredButtonText = "다음";
        
        [Header("Timing")]
        public float cutDuration = 3f;
        public bool waitForPlayerInput = false;
    }
    
    [Serializable]
    public class Act
    {
        [Header("Act Settings")]
        public string actID;
        public string actName;
        public string description;
        
        [Header("Content")]
        public List<Cut> cuts = new List<Cut>();
        
        [Header("Act Settings")]
        public bool isSavePoint = true;
        public string backgroundScene;
    }
    
    [Serializable]
    public class Chapter
    {
        [Header("Chapter Settings")]
        public string chapterID;
        public string chapterName;
        public string description;
        
        [Header("Content")]
        public List<Act> acts = new List<Act>();
        
        [Header("Character")]
        public CharacterType characterType;
    }
    
    [Serializable]
    public class StoryData
    {
        [Header("Story Settings")]
        public string storyID;
        public string storyName;
        public string description;
        
        [Header("Content")]
        public List<Chapter> chapters = new List<Chapter>();
    }
    
    public enum CutStartCondition
    {
        Automatic,
        PlayerInput,
        Timer
    }
    
    public enum CharacterType
    {
        Sherlock,
        Inspector,
        Victim,
        Perpetrator
    }
    
    public enum PlayerPositionType
    {
        MaintainPosition,    // 이전 위치 유지
        TeleportToSpawnPoint, // SpawnPoint로 텔레포트
        TeleportToCustom     // 커스텀 위치로 텔레포트
    }
} 