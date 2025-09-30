# 새로운 스토리 시스템 사용법

## 개요
기존의 복잡한 StoryData 구조를 대체하여 더 직관적이고 계층적인 스토리 시스템을 제공합니다.

## 구조
```
Scene (씬)
├── Chapter01 (챕터)
│   ├── Act01 (액트)
│   │   ├── Cut01 (컷)
│   │   ├── Cut02 (컷)
│   │   └── Cut03 (컷)
│   └── Act02 (액트)
└── Chapter02 (챕터)
    └── Act01 (액트)
```

## 컴포넌트 설명

### 1. Scene.cs
- **역할**: 전체 스토리의 최상위 관리자
- **기능**: 챕터들을 순차적으로 재생
- **사용법**: 
  - 씬에 Scene 컴포넌트 추가
  - Chapters 리스트에 Chapter 오브젝트들 할당
  - `StartScene()` 호출로 재생 시작

### 2. Chapter.cs
- **역할**: 챕터 단위 관리
- **기능**: 액트들을 순차적으로 재생
- **사용법**:
  - Chapter 오브젝트에 Chapter 컴포넌트 추가
  - Acts 리스트에 Act 오브젝트들 할당
  - Scene에서 자동으로 관리됨

### 3. Act.cs
- **역할**: 액트 단위 관리
- **기능**: 컷들을 순차적으로 재생
- **사용법**:
  - Act 오브젝트에 Act 컴포넌트 추가
  - Cuts 리스트에 Cut 오브젝트들 할당
  - Chapter에서 자동으로 관리됨

### 4. Cut.cs
- **역할**: 개별 컷 관리
- **기능**: 시작/중간/종료 이벤트 순차 실행
- **사용법**:
  - Cut 오브젝트에 Cut 컴포넌트 추가
  - Start Events, Middle Events, End Events에 CutEvent 할당
  - Act에서 자동으로 관리됨

### 5. CutEvent.cs
- **역할**: 개별 이벤트 정의
- **기능**: 매니저들과 연동하여 다양한 기능 실행
- **이벤트 타입**:
  - UnityEvent: UnityEvent 실행
  - ManagerMethod: 매니저 메서드 호출
  - PlayAnimation: 애니메이션 재생
  - PlayAudio: 오디오 재생
  - PlayerControl: 플레이어 제어
  - TeleportPlayer: 플레이어 텔레포트
  - ChangeBackgroundMusic: 배경음악 변경
  - ChangeScene: 씬 변경

## 설정 방법

### 1. 기본 구조 생성
1. 빈 GameObject 생성 → "Scene" 이름 변경 → Scene.cs 컴포넌트 추가
2. Scene 하위에 빈 GameObject 생성 → "Chapter01" 이름 변경 → Chapter.cs 컴포넌트 추가
3. Chapter01 하위에 빈 GameObject 생성 → "Act01" 이름 변경 → Act.cs 컴포넌트 추가
4. Act01 하위에 빈 GameObject 생성 → "Cut01" 이름 변경 → Cut.cs 컴포넌트 추가

### 2. 이벤트 설정
1. Cut01에 CutEvent 컴포넌트 추가
2. Event Type을 원하는 타입으로 설정
3. 해당 타입에 맞는 설정값 입력
4. Cut의 Start Events, Middle Events, End Events에 CutEvent 할당

### 3. 매니저 연동
1. 씬에 AudioManager, PlayerManager 등 매니저들 추가
2. CutEvent에서 ManagerMethod 타입 선택
3. Manager Method Name에 호출할 메서드명 입력
4. Parameters에 필요한 매개변수 입력

## 예시 설정

### 플레이어 이동 비활성화 이벤트
- **Event Type**: ManagerMethod
- **Manager Method Name**: SetPlayerMovementEnabled
- **Parameters**: ["false"]

### 배경음악 변경 이벤트
- **Event Type**: ChangeBackgroundMusic
- **Audio Clip**: 원하는 음악 파일

### 애니메이션 재생 이벤트
- **Event Type**: PlayAnimation
- **Animation Clip**: 재생할 애니메이션
- **Target Actor ID**: 애니메이션을 재생할 오브젝트 이름

## 자동 재생 흐름
1. Scene.StartScene() 호출
2. Chapter01 시작 → Act01 시작 → Cut01 시작
3. Cut01의 Start Events 실행
4. Cut01의 Middle Events 실행
5. Cut01의 End Events 실행
6. Cut01 완료 → Cut02 시작 (반복)
7. Act01의 모든 Cut 완료 → Act02 시작
8. Chapter01의 모든 Act 완료 → Chapter02 시작
9. 모든 Chapter 완료 → Scene 완료

## 장점
- **직관적**: 계층 구조로 스토리 구성이 명확
- **유연성**: 각 레벨에서 독립적인 제어 가능
- **확장성**: 새로운 이벤트 타입 쉽게 추가 가능
- **디버깅**: 각 단계별 로그로 문제 파악 용이
- **재사용성**: 컴포넌트 기반으로 재사용 가능

## 주의사항
- 각 컴포넌트는 해당 레벨에서만 제어됨
- 상위 레벨에서 하위 레벨을 중지할 수 있음
- 매니저들은 싱글톤 패턴으로 구현됨
- CutEvent의 매니저 메서드는 public이어야 함
