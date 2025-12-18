# 멀티 씬 구조 빠른 시작 가이드

## 5분 안에 시작하기 🚀

### 1단계: Persistent 씬 생성 (2분)

```
File > New Scene > Empty
저장: Scenes/Persistent.unity
```

**Hierarchy 구성:**
```
Persistent
├── SceneManager ← Scene.cs 추가
├── Player
├── Main Camera (Tag: MainCamera)
│   ├── Camera
│   └── Audio Listener
└── Canvas
    ├── Canvas Scaler
    └── Graphic Raycaster
└── EventSystem
```

**Scene.cs 설정:**
- ✅ Scene Name: "Main Story"
- ✅ Use Multi Scene Loading 체크
- ✅ Chapter Scene Names: `Chapter01`, `Chapter02`, ... 입력

**Inspector에서:**
```
Chapter Scene Names
  Size: 10
  Element 0: Chapter01
  Element 1: Chapter02
  ...
  Element 9: Chapter10
```

---

### 2단계: Chapter01 씬 생성 (2분)

```
File > New Scene > Empty
저장: Scenes/Chapter01.unity
```

**Hierarchy 구성:**
```
Chapter01
├── ChapterManager ← Chapter.cs 추가
│   ├── Chapter Name: "Chapter 1"
│   └── Acts/
│       ├── Act01 (Act.cs 추가)
│       └── Act02 (Act.cs 추가)
│
├── Background/
│   ├── Sky
│   ├── Ground
│   └── Props
│
└── Directional Light
```

**Chapter.cs 설정:**
```
Chapter Name: Chapter 1
Description: 첫 번째 챕터
Acts: Act01, Act02 할당
```

**Lighting 설정:**
```
Window > Rendering > Lighting
- Environment > Skybox Material: 설정
- ☐ Auto Generate 체크 해제
- Generate Lighting 클릭 (수동 베이크)
```

---

### 3단계: Build Settings 설정 (1분)

```
File > Build Settings
Add Open Scenes 버튼 클릭 또는 씬 드래그

필수 순서:
[0] Scenes/Persistent.unity      ← 반드시 첫 번째!
[1] Scenes/Chapter01.unity
[2] Scenes/Chapter02.unity
...
```

**주의:** Persistent가 Index 0이 아니면 작동하지 않음!

---

### 4단계: 테스트 ▶️

```
1. Persistent.unity 씬 열기
2. Play 버튼 클릭
3. Console 확인:
   - "Starting Scene: Main Story"
   - "Loading chapter scene: Chapter01"
   - "Starting Chapter 1: Chapter 1"
4. ✅ Chapter01.unity가 Additive로 로드되고 재생됨!
```

---

## 구조 시각화

### 씬 로딩 흐름

```
🎮 게임 시작
  ↓
📂 Persistent.unity 로드
  ├── SceneManager
  ├── Player (DontDestroyOnLoad 아님!)
  └── Managers
  ↓
🎬 Scene.StartScene() 호출
  ↓
📂 Chapter01.unity Additive 로드
  ├── Chapter 컴포넌트 자동 찾기
  └── 배경/라이팅 로드
  ↓
▶️ Chapter01 재생
  ↓
✅ Chapter01 완료
  ↓
🗑️ Chapter01.unity 언로드
  ├── 메모리 정리 (Resources.UnloadUnusedAssets)
  └── Persistent 씬만 남음
  ↓
📂 Chapter02.unity Additive 로드
  ↓
  (반복...)
```

### 메모리 사용

```
시작:     Persistent (10MB)
Chapter1: Persistent + Chapter01 (10MB + 50MB = 60MB)
전환:     Persistent (10MB) ← Chapter01 언로드
Chapter2: Persistent + Chapter02 (10MB + 45MB = 55MB)
```

---

## Inspector 미리보기

### Scene 컴포넌트 (Persistent 씬)

```
┌─────────────────────────────────────┐
│ Scene (Script)                      │
├─────────────────────────────────────┤
│ Scene Settings                      │
│  Scene Name: Main Story             │
│  Description: 메인 스토리           │
│  ☑ Use Multi Scene Loading         │
├─────────────────────────────────────┤
│ Chapters (Multi Scene Mode)         │
│  [ℹ️ 멀티 씬 모드: 각 챕터의]       │
│  [씬 이름을 입력하세요]             │
│                                     │
│  Chapter 1: [Chapter01        ] [-] │
│  Chapter 2: [Chapter02        ] [-] │
│  Chapter 3: [Chapter03        ] [-] │
│  ...                                │
│                                     │
│  [+ Add Chapter Scene]              │
│  [Quick Setup (10)] [Clear All]     │
├─────────────────────────────────────┤
│ Events                              │
│  On Scene Start ()                  │
│  On Scene Complete ()               │
└─────────────────────────────────────┘
```

### Chapter 컴포넌트 (Chapter01 씬)

```
┌─────────────────────────────────────┐
│ Chapter (Script)                    │
├─────────────────────────────────────┤
│ Chapter Settings                    │
│  Chapter Name: Chapter 1            │
│  Description: 인트로 챕터           │
│  Character Type: Bear               │
├─────────────────────────────────────┤
│ Acts                                │
│  Size: 3                            │
│  Element 0: Act01 (Act)             │
│  Element 1: Act02 (Act)             │
│  Element 2: Act03 (Act)             │
├─────────────────────────────────────┤
│ Events                              │
│  On Chapter Start ()                │
│  On Chapter Complete ()             │
└─────────────────────────────────────┘
```

---

## 자주 하는 실수와 해결

### ❌ 실수 1: Player를 챕터 씬에 넣음

```
Chapter01.unity
└── Player ← 여기 넣으면 안됨!
```

**해결:**
```
Persistent.unity
└── Player ← 여기 있어야 함!
```

---

### ❌ 실수 2: Build Settings에 씬 안 넣음

**증상:** `Scene 'Chapter01' couldn't be loaded` 에러

**해결:**
```
File > Build Settings
모든 씬 추가!
```

---

### ❌ 실수 3: 씬 이름 오타

Scene 컴포넌트에서:
```
Chapter Scene Names
  0: "chapter01" ← 소문자! 실제 씬은 "Chapter01.unity"
```

**해결:** 대소문자 정확히 맞추기
```
Chapter Scene Names
  0: "Chapter01" ✅
```

---

### ❌ 실수 4: Chapter 컴포넌트를 챕터 씬에 안 넣음

**증상:** `No Chapter component found in scene: Chapter01` 에러

**해결:**
```
Chapter01.unity 씬의 Hierarchy에
GameObject 생성 → Chapter.cs 컴포넌트 추가
```

---

## 에디터에서 특정 챕터 테스트하기

### 방법 1: Start() 수정

```csharp
// Scene.cs
private void Start()
{
    #if UNITY_EDITOR
    // 에디터에서는 Chapter 3부터 시작
    StartFromChapter(2); // 0-based index
    #else
    // 빌드에서는 처음부터
    StartScene();
    #endif
}
```

### 방법 2: Inspector에서 직접 제어

```
Play 모드 진입
  ↓
Scene Inspector > Runtime Info
  ↓
[Start from Chapter] 입력 필드에 2 입력
[Go] 버튼 클릭
```

---

## 성능 최적화 팁

### 1. 라이팅 베이크

각 챕터 씬에서:
```
Window > Rendering > Lighting
☑ Auto Generate 해제
수동으로 Generate Lighting
```

이유: 런타임 라이팅 계산 방지

---

### 2. 오클루전 컬링

배경이 복잡한 챕터:
```
Window > Rendering > Occlusion Culling
Bake 클릭
```

이유: 안 보이는 오브젝트 자동 숨김

---

### 3. 메모리 프로파일링

```
Window > Analysis > Profiler
Memory 탭 선택
챕터 전환 시 메모리 사용량 확인
```

목표: 각 챕터 언로드 후 메모리가 Persistent 수준으로 돌아가야 함

---

## 다음 단계

✅ Persistent 씬 생성  
✅ Chapter01 씬 생성  
✅ Scene.cs 설정  
✅ 테스트 성공!  

**이제 할 것:**

1. ⬜ Chapter02~10 씬 생성
2. ⬜ 각 챕터마다 다른 스카이박스 설정
3. ⬜ 각 챕터 Acts 구성
4. ⬜ 전환 이펙트 추가 (선택)
5. ⬜ 로딩 화면 추가 (선택)

---

## 도움이 필요하면

더 자세한 내용:
- `README_MultiSceneSetup.md` - 전체 구조 설명
- `README_NewStorySystem.md` - 스토리 시스템 설명

문제 발생:
- Console 창에서 에러 메시지 확인
- `Debug.Log` 출력 확인
- Scene/Chapter 컴포넌트 설정 다시 확인

