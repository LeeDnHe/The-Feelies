using UnityEngine;
using UnityEditor;
using TheFeelies.Core;
using System.Collections.Generic;

namespace TheFeelies.Editor
{
    public class StoryEditorWindow : EditorWindow
    {
        private StoryDataAsset currentStoryAsset;
        private StoryData currentStoryData => currentStoryAsset?.storyData;
        private Vector2 scrollPosition;
        private int selectedChapterIndex = -1;
        private int selectedActIndex = -1;
        private int selectedCutIndex = -1;
        
        private bool showChapters = true;
        private bool showActs = true;
        private bool showCuts = true;
        
        [MenuItem("The Feelies/Story Editor")]
        public static void ShowWindow()
        {
            StoryEditorWindow window = GetWindow<StoryEditorWindow>("Story Editor");
            window.minSize = new Vector2(800, 600);
        }
        
        private void OnEnable()
        {
            // 키보드 단축키 등록
            EditorApplication.update += OnUpdate;
        }
        
        private void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
        }
        
        private void OnUpdate()
        {
            // 자동 저장 (5초마다)
            if (currentStoryAsset != null && EditorUtility.IsDirty(currentStoryAsset))
            {
                if (Time.realtimeSinceStartup - lastAutoSaveTime > 5f)
                {
                    SaveStoryAsset();
                    lastAutoSaveTime = Time.realtimeSinceStartup;
                }
            }
        }
        
        private float lastAutoSaveTime = 0f;
        
        private void OnGUI()
        {
            // Ctrl+S 단축키 처리
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.S)
            {
                if (currentStoryAsset != null)
                {
                    SaveStoryAsset();
                    e.Use();
                }
            }

            DrawToolbar();
            DrawMainContent();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("New Story", EditorStyles.toolbarButton))
            {
                CreateNewStory();
            }
            
            if (GUILayout.Button("Load Story Asset", EditorStyles.toolbarButton))
            {
                LoadStoryAsset();
            }
            
            if (GUILayout.Button("Save Asset", EditorStyles.toolbarButton))
            {
                SaveStoryAsset();
            }
            
            EditorGUILayout.Space();
            
            if (currentStoryAsset != null)
            {
                string status = "Current: " + currentStoryAsset.storyName;
                if (EditorUtility.IsDirty(currentStoryAsset))
                {
                    status += " (Modified)";
                }
                EditorGUILayout.LabelField(status, EditorStyles.toolbarButton);
            }
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Help", EditorStyles.toolbarButton))
            {
                ShowHelp();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void ShowHelp()
        {
            EditorUtility.DisplayDialog("Story Editor Help", 
                "The Feelies Story Editor 사용법:\n\n" +
                "기본 기능:\n" +
                "1. New Story: 새로운 스토리 에셋을 생성합니다.\n" +
                "2. Load Story Asset: 기존 스토리 에셋을 로드합니다.\n" +
                "3. Save Asset: 현재 스토리를 저장합니다.\n" +
                "4. Ctrl+S: 빠른 저장\n\n" +
                "스토리 구조:\n" +
                "- Chapter: 캐릭터별 스토리 라인 (셜록, 경감, 피해자, 가해자)\n" +
                "- Act: 챕터 내 세이브 포인트 단위\n" +
                "- Cut: 실제 콘텐츠 (대사, 애니메이션, TTS)\n\n" +
                "편집 기능:\n" +
                "- 왼쪽 패널: 스토리 구조 탐색 및 편집\n" +
                "- 오른쪽 패널: 선택된 항목 상세 편집\n" +
                "- 자동 저장: 5초마다 자동 저장\n\n" +
                "단축키:\n" +
                "- Ctrl+S: 저장\n\n" +
                "유효성 검사:\n" +
                "- ID 중복 검사\n" +
                "- 필수 필드 검사\n" +
                "- 콘텐츠 유효성 검사", 
                "확인");
        }
        
        private void DrawMainContent()
        {
            if (currentStoryAsset == null)
            {
                EditorGUILayout.HelpBox("스토리 에셋을 로드하거나 새로 생성해주세요.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            
            // 왼쪽 패널 - 스토리 구조
            DrawStoryStructure();
            
            // 오른쪽 패널 - 선택된 항목 편집
            DrawSelectedItemEditor();
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawStoryStructure()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(400));
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // 스토리 정보
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Story Information", EditorStyles.boldLabel);
            currentStoryData.storyID = EditorGUILayout.TextField("Story ID", currentStoryData.storyID);
            currentStoryData.storyName = EditorGUILayout.TextField("Story Name", currentStoryData.storyName);
            currentStoryData.description = EditorGUILayout.TextField("Description", currentStoryData.description);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 챕터 목록
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            showChapters = EditorGUILayout.Foldout(showChapters, "Chapters (" + currentStoryData.chapters.Count + ")");
            if (showChapters)
            {
                DrawChapterList();
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawChapterList()
        {
            for (int i = 0; i < currentStoryData.chapters.Count; i++)
            {
                var chapter = currentStoryData.chapters[i];
                bool isSelected = selectedChapterIndex == i;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button(isSelected ? "▼" : "▶", GUILayout.Width(20)))
                {
                    selectedChapterIndex = isSelected ? -1 : i;
                    selectedActIndex = -1;
                    selectedCutIndex = -1;
                }
                
                if (GUILayout.Button(chapter.chapterName, isSelected ? EditorStyles.boldLabel : EditorStyles.label))
                {
                    selectedChapterIndex = i;
                    selectedActIndex = -1;
                    selectedCutIndex = -1;
                }
                
                EditorGUILayout.LabelField("(" + chapter.acts.Count + " acts)", EditorStyles.miniLabel);
                
                bool shouldDelete = false;
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog("Delete Chapter", "정말로 이 챕터를 삭제하시겠습니까?", "삭제", "취소"))
                    {
                        shouldDelete = true;
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                if (isSelected)
                {
                    EditorGUI.indentLevel++;
                    DrawActList(chapter, i);
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.EndVertical();
                
                if (shouldDelete)
                {
                    currentStoryData.chapters.RemoveAt(i);
                    break;
                }
            }
            
            if (GUILayout.Button("Add Chapter", EditorStyles.miniButton))
            {
                AddNewChapter();
            }
        }
        
        private void DrawActList(Chapter chapter, int chapterIndex)
        {
            for (int i = 0; i < chapter.acts.Count; i++)
            {
                var act = chapter.acts[i];
                bool isSelected = selectedChapterIndex == chapterIndex && selectedActIndex == i;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button(isSelected ? "▼" : "▶", GUILayout.Width(20)))
                {
                    selectedActIndex = isSelected ? -1 : i;
                    selectedCutIndex = -1;
                }
                
                if (GUILayout.Button(act.actName, isSelected ? EditorStyles.boldLabel : EditorStyles.label))
                {
                    selectedActIndex = i;
                    selectedCutIndex = -1;
                }
                
                EditorGUILayout.LabelField("(" + act.cuts.Count + " cuts)", EditorStyles.miniLabel);
                
                bool shouldDelete = false;
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog("Delete Act", "정말로 이 액트를 삭제하시겠습니까?", "삭제", "취소"))
                    {
                        shouldDelete = true;
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                if (isSelected)
                {
                    EditorGUI.indentLevel++;
                    DrawCutList(act, chapterIndex, i);
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.EndVertical();
                
                if (shouldDelete)
                {
                    chapter.acts.RemoveAt(i);
                    break;
                }
            }
            
            if (GUILayout.Button("Add Act", EditorStyles.miniButton))
            {
                AddNewAct(chapter);
            }
        }
        
        private void DrawCutList(Act act, int chapterIndex, int actIndex)
        {
            for (int i = 0; i < act.cuts.Count; i++)
            {
                var cut = act.cuts[i];
                bool isSelected = selectedChapterIndex == chapterIndex && selectedActIndex == actIndex && selectedCutIndex == i;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button(cut.cutName, isSelected ? EditorStyles.boldLabel : EditorStyles.label))
                {
                    selectedCutIndex = i;
                }
                
                string cutInfo = "";
                if (!string.IsNullOrEmpty(cut.dialogueText))
                    cutInfo += "T";
                if (cut.ttsAudio != null)
                    cutInfo += "A";
                if (cut.characterAnimations.Count > 0)
                    cutInfo += "C";
                if (cut.playerPositionType != PlayerPositionType.MaintainPosition)
                    cutInfo += "P";
                
                if (!string.IsNullOrEmpty(cutInfo))
                    EditorGUILayout.LabelField("(" + cutInfo + ")", EditorStyles.miniLabel);
                
                bool shouldDelete = false;
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog("Delete Cut", "정말로 이 컷을 삭제하시겠습니까?", "삭제", "취소"))
                    {
                        shouldDelete = true;
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                
                if (shouldDelete)
                {
                    act.cuts.RemoveAt(i);
                    break;
                }
            }
            
            if (GUILayout.Button("Add Cut", EditorStyles.miniButton))
            {
                AddNewCut(act);
            }
        }
        
        private void DrawSelectedItemEditor()
        {
            EditorGUILayout.BeginVertical();
            
            try
            {
                if (selectedCutIndex >= 0 && selectedChapterIndex >= 0 && selectedActIndex >= 0 && 
                    selectedChapterIndex < currentStoryData.chapters.Count && 
                    selectedActIndex < currentStoryData.chapters[selectedChapterIndex].acts.Count &&
                    selectedCutIndex < currentStoryData.chapters[selectedChapterIndex].acts[selectedActIndex].cuts.Count)
                {
                    DrawCutEditor(currentStoryData.chapters[selectedChapterIndex].acts[selectedActIndex].cuts[selectedCutIndex]);
                }
                else if (selectedActIndex >= 0 && selectedChapterIndex >= 0 && 
                         selectedChapterIndex < currentStoryData.chapters.Count && 
                         selectedActIndex < currentStoryData.chapters[selectedChapterIndex].acts.Count)
                {
                    DrawActEditor(currentStoryData.chapters[selectedChapterIndex].acts[selectedActIndex]);
                }
                else if (selectedChapterIndex >= 0 && selectedChapterIndex < currentStoryData.chapters.Count)
                {
                    DrawChapterEditor(currentStoryData.chapters[selectedChapterIndex]);
                }
                else
                {
                    DrawStoryOverview();
                }
            }
            catch (System.Exception e)
            {
                EditorGUILayout.HelpBox("에러가 발생했습니다: " + e.Message, MessageType.Error);
                Debug.LogError("Story Editor Error: " + e.Message + "\n" + e.StackTrace);
                selectedChapterIndex = -1;
                selectedActIndex = -1;
                selectedCutIndex = -1;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawStoryOverview()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Story Editor", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("편집할 항목을 선택해주세요.", MessageType.Info);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Story Statistics", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Total Chapters: " + currentStoryData.chapters.Count);
            
            int totalActs = 0;
            int totalCuts = 0;
            int emptyCuts = 0;
            
            foreach (var chapter in currentStoryData.chapters)
            {
                totalActs += chapter.acts.Count;
                foreach (var act in chapter.acts)
                {
                    totalCuts += act.cuts.Count;
                    foreach (var cut in act.cuts)
                    {
                        if (string.IsNullOrEmpty(cut.dialogueText) && cut.ttsAudio == null && cut.characterAnimations.Count == 0)
                        {
                            emptyCuts++;
                        }
                    }
                }
            }
            
            EditorGUILayout.LabelField("Total Acts: " + totalActs);
            EditorGUILayout.LabelField("Total Cuts: " + totalCuts);
            
            if (emptyCuts > 0)
            {
                EditorGUILayout.HelpBox("빈 컷이 " + emptyCuts + "개 있습니다.", MessageType.Warning);
            }
            
            // 유효성 검사 결과 표시
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            
            bool hasErrors = false;
            foreach (var chapter in currentStoryData.chapters)
            {
                if (string.IsNullOrEmpty(chapter.chapterID))
                {
                    EditorGUILayout.HelpBox("Chapter ID가 비어있습니다: " + chapter.chapterName, MessageType.Error);
                    hasErrors = true;
                }
                
                foreach (var act in chapter.acts)
                {
                    if (string.IsNullOrEmpty(act.actID))
                    {
                        EditorGUILayout.HelpBox("Act ID가 비어있습니다: " + act.actName, MessageType.Error);
                        hasErrors = true;
                    }
                    
                    foreach (var cut in act.cuts)
                    {
                        if (string.IsNullOrEmpty(cut.cutID))
                        {
                            EditorGUILayout.HelpBox("Cut ID가 비어있습니다: " + cut.cutName, MessageType.Error);
                            hasErrors = true;
                        }
                    }
                }
            }
            
            if (!hasErrors)
            {
                EditorGUILayout.HelpBox("모든 ID가 유효합니다.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawChapterEditor(Chapter chapter)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Chapter Editor", EditorStyles.boldLabel);
            
            chapter.chapterID = EditorGUILayout.TextField("Chapter ID", chapter.chapterID);
            chapter.chapterName = EditorGUILayout.TextField("Chapter Name", chapter.chapterName);
            chapter.description = EditorGUILayout.TextField("Description", chapter.description);
            chapter.characterType = (CharacterType)EditorGUILayout.EnumPopup("Character Type", chapter.characterType);
            
            // 유효성 검사
            if (string.IsNullOrEmpty(chapter.chapterID))
            {
                EditorGUILayout.HelpBox("Chapter ID는 필수입니다.", MessageType.Warning);
            }
            
            // 중복 ID 검사
            int duplicateCount = 0;
            foreach (var otherChapter in currentStoryData.chapters)
            {
                if (otherChapter != chapter && otherChapter.chapterID == chapter.chapterID)
                {
                    duplicateCount++;
                }
            }
            
            if (duplicateCount > 0)
            {
                EditorGUILayout.HelpBox("중복된 Chapter ID가 있습니다.", MessageType.Error);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Chapter Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Acts: " + chapter.acts.Count);
            EditorGUILayout.EndVertical();
        }
        
        private void DrawActEditor(Act act)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Act Editor", EditorStyles.boldLabel);
            
            act.actID = EditorGUILayout.TextField("Act ID", act.actID);
            act.actName = EditorGUILayout.TextField("Act Name", act.actName);
            act.description = EditorGUILayout.TextField("Description", act.description);
            act.isSavePoint = EditorGUILayout.Toggle("Is Save Point", act.isSavePoint);
            act.backgroundScene = EditorGUILayout.TextField("Background Scene", act.backgroundScene);
            
            // 유효성 검사
            if (string.IsNullOrEmpty(act.actID))
            {
                EditorGUILayout.HelpBox("Act ID는 필수입니다.", MessageType.Warning);
            }
            
            // 중복 ID 검사
            int duplicateCount = 0;
            var currentChapter = currentStoryData.chapters[selectedChapterIndex];
            foreach (var otherAct in currentChapter.acts)
            {
                if (otherAct != act && otherAct.actID == act.actID)
                {
                    duplicateCount++;
                }
            }
            
            if (duplicateCount > 0)
            {
                EditorGUILayout.HelpBox("중복된 Act ID가 있습니다.", MessageType.Error);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Act Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Cuts: " + act.cuts.Count);
            EditorGUILayout.EndVertical();
        }
        
        private void DrawCutEditor(Cut cut)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Cut Editor", EditorStyles.boldLabel);
            
            cut.cutID = EditorGUILayout.TextField("Cut ID", cut.cutID);
            cut.cutName = EditorGUILayout.TextField("Cut Name", cut.cutName);
            cut.description = EditorGUILayout.TextField("Description", cut.description);
            
            // 유효성 검사
            if (string.IsNullOrEmpty(cut.cutID))
            {
                EditorGUILayout.HelpBox("Cut ID는 필수입니다.", MessageType.Warning);
            }
            
            // 중복 ID 검사
            int duplicateCount = 0;
            var currentAct = currentStoryData.chapters[selectedChapterIndex].acts[selectedActIndex];
            foreach (var otherCut in currentAct.cuts)
            {
                if (otherCut != cut && otherCut.cutID == cut.cutID)
                {
                    duplicateCount++;
                }
            }
            
            if (duplicateCount > 0)
            {
                EditorGUILayout.HelpBox("중복된 Cut ID가 있습니다.", MessageType.Error);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Content", EditorStyles.boldLabel);
            
            cut.dialogueText = EditorGUILayout.TextArea(cut.dialogueText, GUILayout.Height(100));
            cut.ttsAudio = (AudioClip)EditorGUILayout.ObjectField("TTS Audio", cut.ttsAudio, typeof(AudioClip), false);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Character Animations", EditorStyles.boldLabel);
            
            DrawCharacterAnimations(cut);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Player Position", EditorStyles.boldLabel);
            
            DrawPlayerPositionSettings(cut);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);
            
            cut.startCondition = (CutStartCondition)EditorGUILayout.EnumPopup("Start Condition", cut.startCondition);
            cut.requiredButtonText = EditorGUILayout.TextField("Button Text", cut.requiredButtonText);
            cut.cutDuration = EditorGUILayout.FloatField("Duration", cut.cutDuration);
            cut.waitForPlayerInput = EditorGUILayout.Toggle("Wait for Input", cut.waitForPlayerInput);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Cut Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Characters: " + cut.characterAnimations.Count);
            EditorGUILayout.LabelField("Has TTS: " + (cut.ttsAudio != null));
            EditorGUILayout.LabelField("Has Dialogue: " + !string.IsNullOrEmpty(cut.dialogueText));
            EditorGUILayout.LabelField("Player Position: " + cut.playerPositionType.ToString());
            
            // 경고 메시지
            if (string.IsNullOrEmpty(cut.dialogueText) && cut.ttsAudio == null && cut.characterAnimations.Count == 0)
            {
                EditorGUILayout.HelpBox("이 컷에는 콘텐츠가 없습니다. 대사, TTS, 또는 애니메이션을 추가해주세요.", MessageType.Warning);
            }
            
            // 플레이어 위치 관련 경고
            if (cut.playerPositionType == PlayerPositionType.TeleportToSpawnPoint && cut.spawnPoint == null)
            {
                EditorGUILayout.HelpBox("Spawn Point가 설정되지 않았습니다.", MessageType.Warning);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawCharacterAnimations(Cut cut)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            for (int i = 0; i < cut.characterAnimations.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                
                cut.characterNames[i] = EditorGUILayout.TextField("Character", cut.characterNames[i]);
                cut.characterAnimations[i] = (AnimationClip)EditorGUILayout.ObjectField("Animation", cut.characterAnimations[i], typeof(AnimationClip), false);
                
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog("Delete Animation", "이 애니메이션을 삭제하시겠습니까?", "삭제", "취소"))
                    {
                        cut.characterAnimations.RemoveAt(i);
                        cut.characterNames.RemoveAt(i);
                        break;
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                // 애니메이션 정보 표시
                if (cut.characterAnimations[i] != null)
                {
                    EditorGUILayout.LabelField("Duration: " + cut.characterAnimations[i].length + "s", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            if (GUILayout.Button("Add Character Animation", EditorStyles.miniButton))
            {
                cut.characterAnimations.Add(null);
                cut.characterNames.Add("Character");
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawPlayerPositionSettings(Cut cut)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            cut.playerPositionType = (PlayerPositionType)EditorGUILayout.EnumPopup("Position Type", cut.playerPositionType);
            
            switch (cut.playerPositionType)
            {
                case PlayerPositionType.MaintainPosition:
                    EditorGUILayout.HelpBox("플레이어의 현재 위치를 유지합니다.", MessageType.Info);
                    break;
                    
                case PlayerPositionType.TeleportToSpawnPoint:
                    cut.spawnPoint = (Transform)EditorGUILayout.ObjectField("Spawn Point", cut.spawnPoint, typeof(Transform), true);
                    if (cut.spawnPoint == null)
                    {
                        EditorGUILayout.HelpBox("Spawn Point를 설정해주세요.", MessageType.Warning);
                    }
                    break;
                    
                case PlayerPositionType.TeleportToCustom:
                    cut.customPosition = EditorGUILayout.Vector3Field("Custom Position", cut.customPosition);
                    cut.customRotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Custom Rotation (Euler)", cut.customRotation.eulerAngles));
                    break;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void CreateNewStory()
        {
            if (currentStoryAsset != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(currentStoryAsset)))
            {
                bool saveCurrent = EditorUtility.DisplayDialog("Save Current", 
                    "현재 스토리를 저장하시겠습니까?", "저장", "저장 안함");
                
                if (saveCurrent)
                {
                    SaveStoryAsset();
                }
            }
            
            currentStoryAsset = CreateInstance<StoryDataAsset>();
            currentStoryAsset.storyID = "new_story";
            currentStoryAsset.storyName = "New Story";
            currentStoryAsset.description = "새로운 스토리";
            currentStoryAsset.storyData = new StoryData
            {
                storyID = "new_story",
                storyName = "New Story",
                description = "새로운 스토리"
            };
            
            // 선택 상태 초기화
            selectedChapterIndex = -1;
            selectedActIndex = -1;
            selectedCutIndex = -1;
            
            Debug.Log("New Story created");
        }
        
        private void LoadStoryAsset()
        {
            string path = EditorUtility.OpenFilePanel("Load Story Asset", "Assets", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                string relativePath = path.Replace(Application.dataPath, "Assets");
                currentStoryAsset = AssetDatabase.LoadAssetAtPath<StoryDataAsset>(relativePath);
                
                if (currentStoryAsset != null)
                {
                    Debug.Log("Story Asset loaded: " + relativePath);
                    // 선택 상태 초기화
                    selectedChapterIndex = -1;
                    selectedActIndex = -1;
                    selectedCutIndex = -1;
                }
                else
                {
                    EditorUtility.DisplayDialog("Load Error", "스토리 에셋을 로드할 수 없습니다.", "확인");
                }
            }
        }
        
        private void SaveStoryAsset()
        {
            if (currentStoryAsset == null) return;
            
            // 이미 저장된 에셋인지 확인
            string assetPath = AssetDatabase.GetAssetPath(currentStoryAsset);
            if (!string.IsNullOrEmpty(assetPath))
            {
                // 기존 에셋 업데이트
                EditorUtility.SetDirty(currentStoryAsset);
                AssetDatabase.SaveAssets();
                Debug.Log("Story Asset saved: " + assetPath);
            }
            else
            {
                // 새 에셋으로 저장
                string path = EditorUtility.SaveFilePanel("Save Story Asset", "Assets", currentStoryAsset.storyID, "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    string relativePath = path.Replace(Application.dataPath, "Assets");
                    AssetDatabase.CreateAsset(currentStoryAsset, relativePath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("Story Asset created: " + relativePath);
                }
            }
        }
        
        private void AddNewChapter()
        {
            var newChapter = new Chapter
            {
                chapterID = "chapter_" + currentStoryData.chapters.Count,
                chapterName = "New Chapter",
                description = "새로운 챕터"
            };
            
            currentStoryData.chapters.Add(newChapter);
            
            // 자동으로 새 챕터 선택
            selectedChapterIndex = currentStoryData.chapters.Count - 1;
            selectedActIndex = -1;
            selectedCutIndex = -1;
            
            // 변경사항 표시
            EditorUtility.SetDirty(currentStoryAsset);
        }
        
        private void AddNewAct(Chapter chapter)
        {
            var newAct = new Act
            {
                actID = "act_" + chapter.acts.Count,
                actName = "New Act",
                description = "새로운 액트"
            };
            
            chapter.acts.Add(newAct);
            
            // 자동으로 새 액트 선택
            selectedActIndex = chapter.acts.Count - 1;
            selectedCutIndex = -1;
            
            // 변경사항 표시
            EditorUtility.SetDirty(currentStoryAsset);
        }
        
        private void AddNewCut(Act act)
        {
            var newCut = new Cut
            {
                cutID = "cut_" + act.cuts.Count,
                cutName = "New Cut",
                description = "새로운 컷"
            };
            
            act.cuts.Add(newCut);
            
            // 자동으로 새 컷 선택
            selectedCutIndex = act.cuts.Count - 1;
            
            // 변경사항 표시
            EditorUtility.SetDirty(currentStoryAsset);
        }
    }
} 