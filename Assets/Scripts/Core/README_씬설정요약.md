# 멀티 씬 구조 설정 요약 ⚡

## 핵심 3단계

### 1️⃣ Persistent 씬 만들기

```
File > New Scene > Empty
저장: Scenes/Persistent.unity

필수 GameObject:
- SceneManager (Scene.cs)
- Player
- Main Camera
- Canvas + EventSystem
- 모든 매니저 싱글톤들
```

**Scene.cs 설정:**
```
☑ Use Multi Scene Loading
Chapter Scene Names: Chapter01, Chapter02, ...
```

---

### 2️⃣ Chapter 씬 만들기

```
File > New Scene > Empty  
저장: Scenes/Chapter01.unity

필수 GameObject:
- ChapterManager (Chapter.cs)
  └── Acts/ (Act들)
- Background/
- Directional Light
```

**Chapter.cs 설정:**
```
Chapter Name: Chapter 1
Acts: Act 컴포넌트들 할당
```

**Lighting:**
```
Window > Rendering > Lighting
☐ Auto Generate 해제
[Generate Lighting] 클릭
```

---

### 3️⃣ Build Settings

```
File > Build Settings

필수 순서:
[0] Persistent.unity    ← 첫 번째!
[1] Chapter01.unity
[2] Chapter02.unity
...
```

---

## 필수 체크리스트 ✅

### Persistent 씬
- [ ] SceneManager + Scene.cs
- [ ] Use Multi Scene Loading ☑
- [ ] Chapter Scene Names 입력
- [ ] Player (여기에만!)
- [ ] 모든 매니저들 (여기에만!)

### Chapter 씬
- [ ] ChapterManager + Chapter.cs
- [ ] Chapter Name 입력
- [ ] Acts 할당
- [ ] 라이팅 베이크

### Build Settings
- [ ] Persistent가 Index 0
- [ ] 모든 Chapter 씬 추가
- [ ] 씬 이름 철자 확인

---

## 주의사항 ⚠️

### ❌ 하지 마세요
- Player를 Chapter 씬에 넣기
- 매니저를 Chapter 씬에 넣기
- 씬 이름 오타 (`chapter01` ≠ `Chapter01`)

### ✅ 꼭 하세요
- Persistent가 Build Settings 첫 번째
- 각 Chapter 씬마다 라이팅 베이크
- Scene.cs의 씬 이름과 실제 파일명 일치

---

## 빠른 테스트

```
1. Persistent.unity 열기
2. Play 버튼
3. Console 확인:
   ✅ "Loading chapter scene: Chapter01"
   ✅ "Starting Chapter 1"
```

---

## 자세한 가이드

- 📋 **SceneSetupChecklist.md** - 완전한 체크리스트
- 🚀 **README_QuickStart.md** - 단계별 가이드
- 📚 **README_MultiSceneSetup.md** - 상세 설명

---

## 문제 해결

| 문제 | 해결 |
|------|------|
| Scene couldn't be loaded | Build Settings에 씬 추가 |
| No Chapter component found | Chapter 씬에 Chapter.cs 추가 |
| Player가 사라짐 | Player를 Persistent 씬으로 |
| 매니저 찾을 수 없음 | 매니저를 Persistent 씬으로 |

---

**완료 후:** 각 챕터의 배경 디자인 시작! 🎨

