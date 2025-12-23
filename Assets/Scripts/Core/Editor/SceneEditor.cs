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
                
                EditorGUILayout.Space(10);
                DrawSceneTester();
            }
            
            serializedObject.ApplyModifiedProperties();

            // 실시간 갱신을 위해 Repaint 호출 (플레이 중에만)
            if (Application.isPlaying)
            {
                Repaint();
            }
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

        private void DrawSceneTester()
        {
            Scene scene = (Scene)target;
            if (!scene.IsPlaying) return;

            EditorGUILayout.LabelField("Scene Tester", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // 1. 현재 상태 정보 표시
            var chapter = scene.CurrentChapter;
            var act = chapter != null ? chapter.CurrentAct : null;
            var cut = act != null ? act.CurrentCut : null;
            var currentEvent = cut != null ? cut.CurrentEvent : null;

            DrawStatusLabel("Current Chapter", chapter != null ? chapter.ChapterName : "None");
            DrawStatusLabel("Current Act", act != null ? act.ActName : "None");
            DrawStatusLabel("Current Cut", cut != null ? cut.CutName : "None");
            
            string eventName = currentEvent != null ? currentEvent.EventName : "None";
            if (cut != null && cut.IsWaitingBeforeStart) eventName = "Waiting (Before Start)";
            else if (cut != null && cut.IsWaitingBeforeEnd) eventName = "Waiting (Before End)";
            
            DrawStatusLabel("Current Event", eventName);

            string status = "Running";
            if (cut != null)
            {
                if (cut.IsWaitingBeforeStart) status = "Waiting for Input (Start)";
                else if (cut.IsWaitingBeforeEnd) status = "Waiting for Input (End)";
                else if (currentEvent != null && currentEvent.Delay > 0) status = "Waiting (Delay)";
                else if (currentEvent != null && currentEvent.WaitAfterExecution > 0) status = "Waiting (Post-Exec)";
            }
            DrawStatusLabel("Status", status);

            EditorGUILayout.Space(5);

            // 2. 컨트롤 (버튼 & 단축키)
            if (cut != null)
            {
                string buttonLabel = "Next Event (Space)";
                if (GUILayout.Button(buttonLabel, GUILayout.Height(30)))
                {
                    cut.SkipCurrentWait();
                }

                // 스페이스바 단축키 처리
                Event e = Event.current;
                if (e != null && e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
                {
                    cut.SkipCurrentWait();
                    e.Use(); // 이벤트 소비
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawStatusLabel(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(120));
            EditorGUILayout.LabelField(value, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }
    }
}

