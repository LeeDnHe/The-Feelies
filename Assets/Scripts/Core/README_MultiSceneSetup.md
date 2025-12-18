# 멀티 씬 챕터 구조 가이드

## 개요

각 챕터마다 다른 배경(스카이박스, 배경 오브젝트)을 사용하면서도 매니저와 플레이어를 공유하는 구조입니다.

## 씬 구조

### 1. Persistent.unity (메인 씬)
항상 로드되어 있는 영구 씬으로, 모든 공통 요소를 포함합니다.

**포함할 오브젝트:**
- Scene Manager (Scene.cs 컴포넌트)
- Player 오브젝트
- Main Camera
- UI Canvas
- Audio Manager
- Game Manager
- 기타 모든 공통 매니저들

### 2. Chapter씬들 (Chapter01.unity ~ Chapter10.unity)
각 챕터별 배경 전용 씬으로, Additive 방식으로 로드/언로드됩니다.

**포함할 오브젝트:**
- Chapter 컴포넌트 (Chapter.cs)
- 스카이박스 설정
- 배경 오브젝트들 (건물, 지형, 소품 등)
- 챕터 전용 라이팅
- 챕터 전용 이펙트
- Act 오브젝트들

**포함하지 말아야 할 것:**
- Player
- 매니저들
- UI Canvas (공통)

## 설정 방법

### Step 1: Persistent 씬 생성

1. 새 씬 생성: `File` > `New Scene`
2. 이름을 `Persistent.unity`로 저장
3. 다음 오브젝트들을 배치:
   ```
   Persistent
   ├── SceneManager (Scene.cs)
   ├── Player
   ├── MainCamera
   ├── UICanvas
   ├── AudioManager
   └── GameManager
   ```

### Step 2: 챕터 씬 생성

각 챕터마다:

1. 새 씬 생성: `Chapter01.unity`, `Chapter02.unity` 등
2. 챕터별 배경 요소 배치:
   ```
   Chapter01
   ├── ChapterRoot (Chapter.cs 컴포넌트 부착)
   ├── Background
   │   ├── Sky
   │   ├── Ground
   │   └── Buildings
   └── Acts
       ├── Act01
       └── Act02
   ```
3. Lighting 설정 (Window > Rendering > Lighting)
   - Environment > Skybox Material 설정
   - 각 챕터의 분위기에 맞는 라이팅 설정

### Step 3: Scene.cs 설정

`Persistent` 씬의 `SceneManager` 오브젝트에서:

1. Scene 컴포넌트의 `Use Multi Scene Loading` 체크 ✅
2. `Chapter Scene Names` 리스트 설정:

**Inspector에서 수동 입력:**
```
Chapter Scene Names
  Size: 10
  Element 0: Chapter01
  Element 1: Chapter02
  Element 2: Chapter03
  ...
  Element 9: Chapter10
```

**또는 Quick Setup 버튼 사용 (Inspector에 있음):**
```
1. "Quick Setup (10 Chapters)" 버튼 클릭
2. 자동으로 Chapter01~10 생성됨
3. 필요없는 챕터는 - 버튼으로 삭제
```

**중요:** 씬 이름만 입력하면 됩니다! Chapter 컴포넌트는 로드 시 자동으로 찾아집니다.

### Step 4: Chapter.cs 설정

각 챕터 씬의 `Chapter` 컴포넌트에서:

1. `Chapter Name`: 입력
2. `Description`: 입력 (선택)
3. `Acts`: Act 컴포넌트들 할당

### Step 5: Build Settings 설정

모든 씬을 Build Settings에 추가:

1. `File` > `Build Settings`
2. 다음 순서로 씬 추가:
   ```
   0: Persistent.unity
   1: Chapter01.unity
   2: Chapter02.unity
   ...
   10: Chapter10.unity
   ```

## 작동 방식

### 씬 로딩 흐름

```
1. 게임 시작 → Persistent.unity 로드
2. Scene.StartScene() 호출
3. Chapter01 시작:
   - Chapter01.unity를 Additive로 로드
   - Chapter01.StartChapter() 호출
   - Acts 재생
4. Chapter01 완료:
   - Chapter01.unity 언로드
   - Resources.UnloadUnusedAssets() 호출 (메모리 정리)
5. Chapter02 시작... (반복)
```

### 메모리 관리

- 각 챕터 완료 시 자동으로 씬 언로드
- `Resources.UnloadUnusedAssets()` 자동 호출로 메모리 정리
- 한 번에 하나의 챕터 씬만 로드되므로 메모리 효율적

## Scene.cs 주요 변경사항

```csharp
// 멀티 씬 로딩 활성화
[SerializeField] private bool useMultiSceneLoading = true;

// 챕터 씬 자동 로드/언로드
private IEnumerator LoadChapterScene(string chapterSceneName)
private IEnumerator UnloadChapterScene()
```

## Chapter.cs 주요 변경사항

```csharp
// 챕터 씬 이름 지정
[SerializeField] private string chapterSceneName;
public string ChapterSceneName => chapterSceneName;
```

## 대안 구조 (참고용)

### DontDestroyOnLoad 방식
멀티 씬이 불편하다면 이 방식도 가능:

```csharp
void Awake()
{
    DontDestroyOnLoad(gameObject);
}
```

**장점:**
- 구현이 간단함
- 씬 전환이 빠름

**단점:**
- 챕터 간 씬 전환 시 플레이어가 깜빡임
- 씬 간 상태 관리가 복잡해질 수 있음
- 메모리 관리가 덜 효율적

## 추가 팁

### 1. 에디터에서 테스트
Persistent 씬과 Chapter 씬을 동시에 열어서 작업 가능:
- Hierarchy에서 마우스 우클릭 > `Load Scene Additive`
- 여러 씬을 동시에 열어 배치 확인

### 2. 씬 간 라이팅 설정
각 챕터마다 다른 라이팅 설정을 사용하려면:
- Lighting Settings Asset 생성
- 각 챕터 씬마다 다른 Lighting Settings 할당

### 3. 챕터 프리뷰
특정 챕터만 테스트하려면:
```csharp
// Scene.cs의 Start()에서
void Start()
{
    // 에디터에서만 특정 챕터부터 시작
    #if UNITY_EDITOR
    StartFromChapter(0); // 원하는 챕터 인덱스
    #else
    StartScene();
    #endif
}
```

## 문제 해결

### Q: 챕터 씬이 로드되지 않아요
- Build Settings에 모든 씬이 추가되었는지 확인
- Chapter의 ChapterSceneName이 정확한지 확인 (대소문자 구분)

### Q: 플레이어가 챕터 씬 언로드 시 사라져요
- Player가 Persistent 씬에 있는지 확인
- Player가 챕터 씬에 있지 않은지 확인

### Q: 라이팅이 이상해요
- 각 챕터 씬에서 Generate Lighting 실행
- Lighting Settings를 챕터별로 분리

### Q: 메모리 사용량이 계속 증가해요
- Scene.cs의 UnloadChapterScene()에서 Resources.UnloadUnusedAssets() 호출 확인
- 챕터 씬에 static 오브젝트가 많은 경우 메모리 누수 가능

## 참고 자료

- [Unity - Multiple Scenes](https://docs.unity3d.com/Manual/MultiSceneEditing.html)
- [Unity - SceneManager API](https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.html)
- [Unity - Additive Scene Loading](https://docs.unity3d.com/Manual/LoadingScenes.html)

