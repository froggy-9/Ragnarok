# Dead Letter Office 작업 흐름

## 1. 기본 데이터 만들기

Unity 상단 메뉴에서 `DLO > Content > Create Chapter 1 Starter Content`를 누른다.

그러면 `Assets/ScriptableObjectes` 아래에 챕터 1용 편지, 플래그, NPC 데이터, 추리보드 카드, 챕터 비트가 자동 생성된다.

## 2. 배치용 프리팹 만들기

`DLO > Setup > Create Placement Prefabs`를 누른다.

그러면 `Assets/Prefabs/DLO` 아래에 플레이어, 카메라, NPC, 편지 픽업, 조사 오브젝트, 스토리 카메라 트리거, 최소 UI 프리팹이 생성된다.

## 3. DevScene 메인 HUD 만들기

1. `Assets/Scenes/DevScenes/DevScene_UI.unity`를 연다.
2. `DLO > Setup > Rebuild DevScene Main HUD`를 누른다.
3. 씬의 Canvas 아래에 `DLO_MainHUD`가 생성된다.

생성되는 UI는 좌상단 지도, 원신식 임무 목표/거리 표시, 우상단 추리보드 버튼이다.

## 4. 맵에 배치하기

씬에 `PF_Player`, `PF_PlayerCamera`, `PF_MinimalUI`, `PF_PersistentSystems`를 배치한다.

편지는 `PF_LetterPickup`을 복사해서 맵에 놓고, Inspector에서 `GameState`, `Letter`, `Set Flag On Found`를 연결한다.

NPC는 `PF_NPC`를 복사해서 배치하고, `NPC Data`에 원하는 NPC 데이터를 연결한다.

조사 오브젝트는 `PF_InteractableObject`를 배치하고, `Narration`에 조사 문구를 입력한다.

## 5. 스토리 중 임무 바꾸기

임무 문구와 목표 위치는 `QuestObjectiveChangedEvent`로 바꾼다.

```csharp
GameEventBus.Publish(new QuestObjectiveChangedEvent("우체국 뒤편 작업대로 가기", targetTransform));
```

목표 위치 없이 고정 거리만 보여주고 싶으면 이렇게 쓴다.

```csharp
GameEventBus.Publish(new QuestObjectiveChangedEvent("관리자가 알려준 작업대로 가기", null, 49));
```

## 6. 스토리 카메라 연출

`PF_StoryCameraTrigger`를 원하는 위치에 놓는다.

`Required Flags`에 조건 플래그를 넣으면, 그 플래그가 켜진 상태에서 플레이어가 트리거에 들어올 때만 카메라가 `ShotTransform` 위치로 이동한다.

## 7. 데이터 검사

`DLO > Content > Validate Content`를 누르면 비어 있는 편지 본문, 누락된 NPC 대화, 잘못된 추리보드 연결 같은 기본 실수를 확인할 수 있다.
