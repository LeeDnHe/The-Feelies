using UnityEngine;

namespace TheFeelies.Core
{
    [CreateAssetMenu(fileName = "New Story Data", menuName = "The Feelies/Story Data")]
    public class StoryDataAsset : ScriptableObject
    {
        [Header("Story Information")]
        public string storyID;
        public string storyName;
        public string description;
        
        [Header("Story Content")]
        public StoryData storyData;
        
        private void OnValidate()
        {
            if (storyData == null)
            {
                storyData = new StoryData();
            }
            
            // 에셋의 기본 정보를 스토리 데이터와 동기화
            if (storyData.storyID != storyID)
            {
                storyData.storyID = storyID;
            }
            
            if (storyData.storyName != storyName)
            {
                storyData.storyName = storyName;
            }
            
            if (storyData.description != description)
            {
                storyData.description = description;
            }
        }
    }
} 