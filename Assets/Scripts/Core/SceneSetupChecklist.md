# 씬 설정 필수 체크리스트

## 📋 Persistent 씬 필수 설정

### 1. Hierarchy 구조
```
Persistent
├── SceneManager
│   └── Scene.cs 컴포넌트
├── Player
├── Main Camera
│   ├── Camera 컴포넌트
│   └── Audio Listener
├── Canvas
│   ├── Canvas (Render Mode: Screen Space - Overlay)
│   ├── Canvas Scaler
│   └── Graphic Raycaster
└── EventSystem
    ├── Event System
    └── Standalone Input Module
```

### 2. SceneManager (Scene.cs) 설정

**필수 체크:**
- ✅ `Scene Name`: 입력
- ✅ `Use Multi Scene Loading`: **체크**
- ✅ `Chapter Scene Names`: 챕터 씬 이름 입력
  - 예: `Chapter01`, `Chapter02`, ...

**Inspector 예시:**
```
Scene Name: Main Story
Description: (선택사항)
☑ Use Multi Scene Loading

Chapter Scene Names:
  Element 0: Chapter01
  Element 1: Chapter02
  Element 2: Chapter03
  ...
```

### 3. 매니저들 추가 (싱글톤)

**필수 매니저:** (프로젝트에 따라)
- AnimationManager
- SoundManager
- PlayerManager
- AudioManager
- 기타 필요한 매니저들

**주의:** 모두 Persistent 씬에 배치!

---

## 📋 Chapter 씬 필수 설정

### 1. Hierarchy 구조
```
Chapter01
├── ChapterManager
│   ├── Chapter.cs 컴포넌트
│   └── Acts/
│       ├── Act01 (Act.cs)
│       ├── Act02 (Act.cs)
│       └── ...
├── Background/
│   ├── Sky (스카이박스 관련)
│   ├── Ground (지형/바닥)
│   └── Props (소품/장식)
└── Directional Light
```

### 2. ChapterManager (Chapter.cs) 설정

**필수 체크:**
- ✅ `Chapter Name`: 입력
- ✅ `Acts`: Act 컴포넌트들 할당

**Inspector 예시:**
```
Chapter Name: Chapter 1
Description: 첫 번째 챕터
Character Type: (선택)

Acts:
  Size: 3
  Element 0: Act01 (Act)
  Element 1: Act02 (Act)
  Element 2: Act03 (Act)
```

### 3. Lighting 설정

**Window > Rendering > Lighting**

**필수 설정:**
- ✅ Environment > Skybox Material: 설정
- ✅ Environment Lighting: 설정
- ✅ **☐ Auto Generate 체크 해제** (권장)
- ✅ **Generate Lighting 클릭** (수동 베이크)

**이유:**
- 런타임에 라이팅 재계산 방지
- 챕터 로딩 속도 향상

### 4. Scene Settings

**Edit > Project Settings > Graphics**
- 필요시 Quality 설정 조정

---

## 📋 Build Settings 필수 설정

### File > Build Settings

**씬 추가 순서 (중요!):**
```
0: Persistent.unity      ← 반드시 첫 번째!
1: Chapter01.unity
2: Chapter02.unity
3: Chapter03.unity
...
```

**체크사항:**
- ✅ 모든 Chapter 씬이 추가되었는지 확인
- ✅ Persistent가 Index 0인지 확인
- ✅ 각 씬의 체크박스가 활성화되었는지 확인

---

## 🔍 검증 체크리스트

### Persistent 씬
- [ ] SceneManager 오브젝트 있음
- [ ] Scene.cs 컴포넌트 추가됨
- [ ] Use Multi Scene Loading 체크됨
- [ ] Chapter Scene Names 리스트에 모든 챕터 추가됨
- [ ] Player 오브젝트 있음
- [ ] Main Camera 있음 (Tag: MainCamera)
- [ ] Canvas + EventSystem 있음
- [ ] 모든 매니저 싱글톤들 있음

### Chapter 씬 (각각)
- [ ] ChapterManager 오브젝트 있음
- [ ] Chapter.cs 컴포넌트 추가됨
- [ ] Chapter Name 입력됨
- [ ] Acts 리스트에 Act들 추가됨
- [ ] Background 그룹 있음
- [ ] Directional Light 있음
- [ ] Skybox 설정됨
- [ ] Lighting 베이크됨

### Build Settings
- [ ] Persistent.unity가 Index 0
- [ ] 모든 Chapter 씬 추가됨
- [ ] 씬 이름 철자 확인 (대소문자 일치)

---

## ⚠️ 주의사항

### ❌ 하지 말아야 할 것

1. **Player를 Chapter 씬에 넣지 마세요**
   ```
   ❌ Chapter01/Player  (챕터 언로드 시 사라짐!)
   ✅ Persistent/Player  (항상 유지)
   ```

2. **매니저를 Chapter 씬에 넣지 마세요**
   ```
   ❌ Chapter01/AnimationManager
   ✅ Persistent/AnimationManager
   ```

3. **Chapter Scene Names 철자 확인**
   ```
   ❌ "chapter01"  (소문자, 실제 씬: Chapter01.unity)
   ✅ "Chapter01"  (대소문자 일치!)
   ```

4. **DontDestroyOnLoad 사용 금지**
   ```
   Persistent 씬이 유지되므로 불필요함
   ```

### ✅ 꼭 해야 할 것

1. **라이팅 베이크**
   - 각 Chapter 씬마다 개별적으로 베이크
   - Auto Generate 끄고 수동 Generate

2. **Build Settings 순서**
   - Persistent가 반드시 첫 번째

3. **씬 이름 규칙 통일**
   - Scene.cs: `Chapter01`
   - 파일명: `Chapter01.unity`
   - 정확히 일치해야 함!

---

## 🎯 빠른 설정 가이드

### 1분 체크리스트

**Persistent 씬:**
1. SceneManager + Scene.cs ✅
2. Use Multi Scene Loading ✅
3. Chapter Scene Names 입력 ✅
4. Player + Camera + Canvas ✅

**Chapter 씬:**
1. ChapterManager + Chapter.cs ✅
2. Chapter Name 입력 ✅
3. Acts 추가 ✅
4. Lighting 베이크 ✅

**Build Settings:**
1. Persistent (Index 0) ✅
2. 모든 Chapter 씬 추가 ✅

**테스트:**
1. Persistent 씬 열기 ✅
2. Play 버튼 ✅
3. Chapter 로딩 확인 ✅

---

## 🐛 문제 해결

### "Scene 'Chapter01' couldn't be loaded"
→ Build Settings에 Chapter01.unity 추가

### "No Chapter component found"
→ Chapter 씬에 Chapter.cs 추가

### "Player가 사라짐"
→ Player를 Persistent 씬으로 이동

### "매니저를 찾을 수 없음"
→ 매니저들을 Persistent 씬으로 이동

---

## 📁 폴더 구조 권장

```
Assets/
├── Scenes/
│   ├── Persistent.unity
│   ├── Chapter01.unity
│   ├── Chapter02.unity
│   └── ...
│
├── Scripts/
│   └── Core/
│       ├── Scene.cs
│       ├── Chapter.cs
│       ├── Act.cs
│       └── ...
│
└── Resources/
    ├── Skyboxes/
    ├── Materials/
    └── ...
```

---

## 완료! 🎉

이 체크리스트를 따라 설정하면 멀티 씬 구조가 완성됩니다.

**다음 단계:**
1. 각 챕터의 배경 디자인
2. Act와 Cut 구성
3. 컷 이벤트 설정
4. 테스트 및 디버깅

