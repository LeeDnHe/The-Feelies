using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TheFeelies.Core.Editor
{
    [CustomEditor(typeof(Scene))]
    public class SceneEditor : UnityEditor.Editor
    {
        private SerializedProperty sceneName;
        private SerializedProperty description;
        private SerializedProperty useMultiSceneLoading;
        private SerializedProperty chapterSceneNames;
        private SerializedProperty chapters;
        private SerializedProperty onSceneStart;
        private SerializedProperty onSceneComplete;
        
        private void OnEnable()
        {
            sceneName = serializedObject.FindProperty("sceneName");
            description = serializedObject.FindProperty("description");
            useMultiSceneLoading = serializedObject.FindProperty("useMultiSceneLoading");
            chapterSceneNames = serializedObject.FindProperty("chapterSceneNames");
            chapters = serializedObject.FindProperty("chapters");
            onSceneStart = serializedObject.FindProperty("onSceneStart");
            onSceneComplete = serializedObject.FindProperty("onSceneComplete");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Scene Settings
            EditorGUILayout.LabelField("Scene Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sceneName);
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.PropertyField(useMultiSceneLoading, new GUIContent("Use Multi Scene Loading", 
                "체크 시: 챕터별 씬을 Additive로 로드 (권장)\n해제 시: 단일 씬에서 모든 챕터 관리"));
            
            EditorGUILayout.Space(10);
            
            // Chapters
            bool isMultiScene = useMultiSceneLoading.boolValue;
            
            if (isMultiScene)
            {
                DrawMultiSceneMode();
            }
            else
            {
                DrawSingleSceneMode();
            }
            
            EditorGUILayout.Space(10);
            
            // Events
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onSceneStart);
            EditorGUILayout.PropertyField(onSceneComplete);
            
            // Runtime Info
            if (Application.isPlaying)
            {
                EditorGUILayout.Space(10);
                DrawRuntimeInfo();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawMultiSceneMode()
        {
            EditorGUILayout.LabelField("Chapters (Multi Scene Mode)", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "멀티 씬 모드: 각 챕터의 씬 이름을 입력하세요.\n" +
                "씬은 Additive 방식으로 로드되며, 자동으로 Chapter 컴포넌트를 찾습니다.\n" +
                "모든 씬은 Build Settings에 추가되어 있어야 합니다.", 
                MessageType.Info);
            
            EditorGUILayout.Space(5);
            
            // 챕터 씬 이름 리스트
            for (int i = 0; i < chapterSceneNames.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField($"Chapter {i + 1}:", GUILayout.Width(80));
                
                SerializedProperty sceneNameProp = chapterSceneNames.GetArrayElementAtIndex(i);
                sceneNameProp.stringValue = EditorGUILayout.TextField(sceneNameProp.stringValue);
                
                // 삭제 버튼
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    chapterSceneNames.DeleteArrayElementAtIndex(i);
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space(5);
            
            // 추가 버튼
            if (GUILayout.Button("+ Add Chapter Scene"))
            {
                chapterSceneNames.arraySize++;
                SerializedProperty newElement = chapterSceneNames.GetArrayElementAtIndex(chapterSceneNames.arraySize - 1);
                newElement.stringValue = $"Chapter{chapterSceneNames.arraySize:D2}";
            }
            
            EditorGUILayout.Space(5);
            
            // 빠른 설정 버튼
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Quick Setup (10 Chapters)"))
            {
                chapterSceneNames.ClearArray();
                for (int i = 1; i <= 10; i++)
                {
                    chapterSceneNames.arraySize++;
                    SerializedProperty element = chapterSceneNames.GetArrayElementAtIndex(i - 1);
                    element.stringValue = $"Chapter{i:D2}";
                }
            }
            if (GUILayout.Button("Clear All"))
            {
                if (EditorUtility.DisplayDialog("Clear All Chapters", 
                    "모든 챕터를 삭제하시겠습니까?", "확인", "취소"))
                {
                    chapterSceneNames.ClearArray();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawSingleSceneMode()
        {
            EditorGUILayout.LabelField("Chapters (Single Scene Mode)", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "단일 씬 모드: 같은 씬 내에 있는 Chapter 컴포넌트들을 직접 참조합니다.\n" +
                "배경이 동적으로 변경되지 않는 경우에 적합합니다.", 
                MessageType.Info);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(chapters, true);
        }
        
        private void DrawRuntimeInfo()
        {
            EditorGUILayout.LabelField("Runtime Info", EditorStyles.boldLabel);
            
            Scene sceneComponent = (Scene)target;
            
            GUI.enabled = false;
            EditorGUILayout.Toggle("Is Playing", sceneComponent.IsPlaying);
            EditorGUILayout.IntField("Current Chapter Index", sceneComponent.CurrentChapterIndex);
            EditorGUILayout.IntField("Total Chapters", sceneComponent.TotalChapters);
            
            if (sceneComponent.CurrentChapter != null)
            {
                EditorGUILayout.TextField("Current Chapter Name", sceneComponent.CurrentChapter.ChapterName);
            }
            GUI.enabled = true;
            
            EditorGUILayout.Space(5);
            
            // 런타임 컨트롤
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = !sceneComponent.IsPlaying;
            if (GUILayout.Button("Start Scene"))
            {
                sceneComponent.StartScene();
            }
            GUI.enabled = sceneComponent.IsPlaying;
            if (GUILayout.Button("Stop Scene"))
            {
                sceneComponent.StopScene();
            }
            if (GUILayout.Button("Restart Scene"))
            {
                sceneComponent.RestartScene();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            if (sceneComponent.IsPlaying)
            {
                if (GUILayout.Button("Skip to Next Chapter"))
                {
                    sceneComponent.SkipToNextChapter();
                }
            }
        }
    }
}

