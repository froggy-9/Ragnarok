using System.IO;
using DeadLetterOffice.Core;
using DeadLetterOffice.Interaction;
using DeadLetterOffice.State;
using DeadLetterOffice.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DeadLetterOffice.Editor
{
    public static class DLOSceneSetupTools
    {
        private const string DevScenePath = "Assets/Scenes/DevScenes/DevScen.unity";
        private const string HudRootName = "DLO_MainHUD";
        private const string LegacyInteractionCanvasName = "DLO_InteractionCanvas";
        private const string CircleSpritePath = "Assets/Art/UI/dlo_ui_soft_circle.png";
        private const string DefaultFontAssetPath = "Assets/Art/Fonts/GowunBatang-Regular SDF.asset";
        private const string GameStatePath = "Assets/ScriptableObjectes/GameState/GameState.asset";
        private static bool s_buildInCurrentScene;

        [MenuItem("DLO/Setup/Rebuild DevScene Main HUD")]
        public static void BuildDevSceneMainHud()
        {
            if (!s_buildInCurrentScene)
            {
                OpenSceneIfExists(DevScenePath);
            }

            Canvas canvas = FindOrCreateCanvas();
            ClearLegacyInteractionCanvas();
            ClearPreviousHud(canvas.transform);

            GameObject hudRoot = CreateRect(canvas.transform, HudRootName, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform hudRect = hudRoot.GetComponent<RectTransform>();
            hudRect.offsetMin = Vector2.zero;
            hudRect.offsetMax = Vector2.zero;
            hudRoot.AddComponent<UIAudioPlayer>();
            EnsureGameModeManager();
            QuestSystem questSystem = EnsureQuestSystem();

            Sprite circleSprite = GetOrCreateCircleSprite();
            GameObject mapPanel = CreateMapPanel(hudRoot.transform);
            GameObject questPanel = CreateQuestPanel(hudRoot.transform, questSystem);
            GameObject helpPanel = CreateHelpPanel(hudRoot.transform);
            GameObject archivePanel = CreateArchivePanel(hudRoot.transform);
            GameObject boardPanel = CreateBoardPanel(hudRoot.transform);
            GameObject settingsPanel = CreateSettingsPanelV2(hudRoot.transform);
            CreateMinimap(hudRoot.transform, circleSprite, mapPanel);
            CreateHelpPrompt(hudRoot.transform, helpPanel);
            CreateQuestTracker(hudRoot.transform, circleSprite, questPanel);
            CreateArchiveButtonV2(hudRoot.transform, archivePanel);
            GameObject inferenceBoardButton = CreateInferenceBoardButtonV2(hudRoot.transform, boardPanel);
            CreateSettingsButtonV2(hudRoot.transform, settingsPanel);
            CreateActionSlotDock(hudRoot.transform);
            CreateInteractionPrompt(hudRoot.transform);
            CreateExplorationCrosshair(hudRoot);
            CreateQuestUnlockToast(hudRoot.transform);
            GameObject unlockOverlay = CreateUnlockOverlay(hudRoot.transform);
            DLOHudAutoBinder autoBinder = hudRoot.AddComponent<DLOHudAutoBinder>();
            Set(autoBinder, "_miniMap", hudRoot.GetComponentInChildren<MiniMapUI>(true));
            Set(autoBinder, "_mapView", hudRoot.GetComponentInChildren<MapViewUI>(true));
            Set(autoBinder, "_questObjective", hudRoot.GetComponentInChildren<QuestObjectiveHUD>(true));
            UIPanelHotkeyManager hotkeys = hudRoot.AddComponent<UIPanelHotkeyManager>();
            Set(hotkeys, "_mapPanel", mapPanel);
            Set(hotkeys, "_questPanel", questPanel);
            Set(hotkeys, "_helpPanel", helpPanel);
            Set(hotkeys, "_archivePanel", archivePanel);
            Set(hotkeys, "_boardPanel", boardPanel);
            Set(hotkeys, "_settingsPanel", settingsPanel);
            UIFeatureUnlockPresenter unlockPresenter = hudRoot.AddComponent<UIFeatureUnlockPresenter>();
            Set(unlockPresenter, "_unlockOverlay", unlockOverlay);
            Set(unlockPresenter, "_titleText", unlockOverlay.transform.Find("UnlockTitle").GetComponent<TextMeshProUGUI>());
            Set(unlockPresenter, "_descriptionText", unlockOverlay.transform.Find("UnlockDescription").GetComponent<TextMeshProUGUI>());
            Set(unlockPresenter, "_inferenceBoardButton", inferenceBoardButton.GetComponent<UnlockableHudButton>());
            Set(unlockPresenter, "_hotkeyManager", hotkeys);
            mapPanel.transform.SetAsLastSibling();
            questPanel.transform.SetAsLastSibling();
            helpPanel.transform.SetAsLastSibling();
            archivePanel.transform.SetAsLastSibling();
            boardPanel.transform.SetAsLastSibling();
            settingsPanel.transform.SetAsLastSibling();
            unlockOverlay.transform.SetAsLastSibling();
            EnsureEventSystem();

            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DLOSceneSetupTools] DevScene_UI main HUD rebuilt.");
        }

        [MenuItem("DLO/Setup/Rebuild Current Scene Main HUD")]
        public static void BuildCurrentSceneMainHud()
        {
            s_buildInCurrentScene = true;
            try
            {
                BuildDevSceneMainHud();
            }
            finally
            {
                s_buildInCurrentScene = false;
            }
        }

        private static void OpenSceneIfExists(string scenePath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                Debug.LogWarning($"[DLOSceneSetupTools] Scene not found at {scenePath}. Building in the currently open scene instead.");
                return;
            }

            EditorSceneManager.OpenScene(scenePath);
        }

        private static Canvas FindOrCreateCanvas()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                return canvas;
            }

            GameObject canvasObject = new("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void ClearPreviousHud(Transform canvas)
        {
            Transform previous = canvas.Find(HudRootName);
            if (previous != null)
            {
                Object.DestroyImmediate(previous.gameObject);
            }
        }

        private static void ClearLegacyInteractionCanvas()
        {
            GameObject previous = GameObject.Find(LegacyInteractionCanvasName);
            if (previous != null)
            {
                Object.DestroyImmediate(previous);
            }
        }

        private static void EnsureGameModeManager()
        {
            if (Object.FindFirstObjectByType<GameModeManager>() != null)
            {
                return;
            }

            GameObject manager = new("DLO_GameModeManager");
            manager.AddComponent<GameModeManager>();
        }

        private static QuestSystem EnsureQuestSystem()
        {
            QuestSystem questSystem = Object.FindFirstObjectByType<QuestSystem>();
            if (questSystem != null)
            {
                return questSystem;
            }

            GameObject manager = new("DLO_QuestSystem");
            return manager.AddComponent<QuestSystem>();
        }

        private static void CreateMinimap(Transform parent, Sprite circleSprite, GameObject mapPanel)
        {
            GameObject minimap = CreateImage(parent, "Minimap", Anchor.TopLeft, new Vector2(68f, -28f), new Vector2(150f, 150f), circleSprite, new Color(0.05f, 0.11f, 0.12f, 0.62f));
            RectTransform rect = minimap.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0f, 1f);

            CreateImage(minimap.transform, "MapTint", Anchor.Stretch, Vector2.zero, Vector2.zero, circleSprite, new Color(0.52f, 0.72f, 0.57f, 0.28f));

            GameObject playerMarker = CreateImage(minimap.transform, "PlayerMarker", Anchor.Center, Vector2.zero, new Vector2(16f, 16f), circleSprite, new Color(0.5f, 0.95f, 1f, 1f));

            TextMeshProUGUI label = CreateText(minimap.transform, "MapLabel", "지도", 22, TextAlignmentOptions.Center, Color.white);
            Stretch(label.rectTransform, new Vector2(16f, 16f), new Vector2(-16f, -16f));

            MiniMapUI miniMapUI = minimap.AddComponent<MiniMapUI>();
            Set(miniMapUI, "_imageMap", minimap.GetComponent<Image>());
            Set(miniMapUI, "_mapArea", rect);
            Set(miniMapUI, "_playerMarker", playerMarker.GetComponent<RectTransform>());

            Image clickImage = minimap.GetComponent<Image>();
            clickImage.raycastTarget = true;
            Button mapButton = minimap.AddComponent<Button>();
            AddButtonAudio(mapButton);
            mapButton.targetGraphic = clickImage;
            UIPanelToggle mapToggle = minimap.AddComponent<UIPanelToggle>();
            Set(mapToggle, "_panel", mapPanel);
            Set(mapToggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(mapButton.onClick, mapToggle.Show);
        }

        private static GameObject CreateMapPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "MapPanel", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.02f, 0.02f, 0.025f, 0.92f));
            Stretch(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            AddPanelMotion(panel, UIPanelMotionPreset.MapPanel);

            GameObject mapFrame = CreatePanel(panel.transform, "MapFrame", Anchor.Stretch, Vector2.zero, Vector2.zero, Color.white);
            Stretch(mapFrame.GetComponent<RectTransform>(), new Vector2(52f, 86f), new Vector2(-52f, -76f));

            GameObject mapArea = CreatePanel(mapFrame.transform, "RenderedMapArea", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.76f, 0.78f, 0.74f, 1f));
            Stretch(mapArea.GetComponent<RectTransform>(), new Vector2(18f, 18f), new Vector2(-18f, -18f));

            GameObject playerMarker = CreatePanel(mapArea.transform, "PlayerMarker", Anchor.Center, Vector2.zero, new Vector2(24f, 24f), new Color(0.25f, 0.8f, 1f, 1f));

            TextMeshProUGUI title = CreateText(panel.transform, "MapTitle", "Map", 24, TextAlignmentOptions.Left, new Color(0.82f, 0.82f, 0.82f, 1f));
            SetRect(title.rectTransform, Anchor.TopLeft, new Vector2(18f, -26f), new Vector2(220f, 40f));

            Button closeButton = CreateTextButton(panel.transform, "CloseButton", "X", Anchor.TopRight, new Vector2(-62f, -32f), new Vector2(62f, 62f));
            UIPanelToggle toggle = closeButton.gameObject.AddComponent<UIPanelToggle>();
            Set(toggle, "_panel", panel);
            Set(toggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(closeButton.onClick, toggle.Hide);

            MapViewUI mapViewUI = panel.AddComponent<MapViewUI>();
            Set(mapViewUI, "_root", panel);
            Set(mapViewUI, "_titleText", title);
            Set(mapViewUI, "_imageMap", mapArea.GetComponent<Image>());
            Set(mapViewUI, "_mapArea", mapArea.GetComponent<RectTransform>());
            Set(mapViewUI, "_playerMarker", playerMarker.GetComponent<RectTransform>());

            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateHelpPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "HelpPanel", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.07f, 0.92f));
            Stretch(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            AddPanelMotion(panel);

            TextMeshProUGUI title = CreateText(panel.transform, "HelpTitle", "여정 기록", 34, TextAlignmentOptions.Left, new Color(0.95f, 0.88f, 0.7f, 1f));
            SetRect(title.rectTransform, Anchor.TopLeft, new Vector2(190f, -96f), new Vector2(420f, 56f));

            TextMeshProUGUI subtitle = CreateText(panel.transform, "HelpSubtitle", "기본 조작과 화면 기능", 20, TextAlignmentOptions.Left, new Color(0.78f, 0.78f, 0.86f, 1f));
            SetRect(subtitle.rectTransform, Anchor.TopLeft, new Vector2(190f, -146f), new Vector2(360f, 34f));

            GameObject leftList = CreatePanel(panel.transform, "HelpListPanel", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.07f, 0.09f, 0.14f, 0.58f));
            RectTransform leftRect = leftList.GetComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0f, 0f);
            leftRect.anchorMax = new Vector2(0f, 1f);
            leftRect.pivot = new Vector2(0f, 0.5f);
            leftRect.anchoredPosition = new Vector2(190f, -40f);
            leftRect.sizeDelta = new Vector2(410f, -300f);

            Transform leftListContent = CreateVerticalScrollContent(leftList.transform, "HelpScroll", Vector2.zero, Vector2.zero, 12f, out _);

            Button helpEntryTemplate = CreateTextButton(leftList.transform, "HelpEntryButtonTemplate", "이동과 카메라", Anchor.TopLeft, Vector2.zero, new Vector2(370f, 58f));
            helpEntryTemplate.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.25f, 0.72f);
            TextMeshProUGUI helpEntryLabel = helpEntryTemplate.GetComponentInChildren<TextMeshProUGUI>();
            helpEntryLabel.alignment = TextAlignmentOptions.Left;
            helpEntryLabel.color = new Color(0.9f, 0.82f, 0.62f, 1f);
            Stretch(helpEntryLabel.rectTransform, new Vector2(22f, 8f), new Vector2(-12f, -8f));
            helpEntryTemplate.gameObject.SetActive(false);

            GameObject detail = CreateRect(panel.transform, "HelpDetail", Anchor.Stretch, Vector2.zero, Vector2.zero);
            Stretch(detail.GetComponent<RectTransform>(), new Vector2(680f, 160f), new Vector2(-180f, -130f));

            TextMeshProUGUI detailTitle = CreateText(detail.transform, "HelpDetailTitle", "이동과 카메라", 32, TextAlignmentOptions.Left, Color.white);
            SetRect(detailTitle.rectTransform, Anchor.TopLeft, Vector2.zero, new Vector2(460f, 50f));

            TextMeshProUGUI detailText = CreateText(
                detail.transform,
                "HelpDetailText",
                "플레이어는 하나의 캐릭터를 조작합니다.\n\n- 이동: 방향 입력\n- 달리기: Sprint 입력\n- 시점 조절: 카메라 입력\n- 미니맵 클릭: 큰 지도 열기\n- 임무 클릭: 현재 진행 중인 스토리 확인\n\n이 화면은 나중에 실제 조작키와 시스템 설명으로 교체하면 됩니다.",
                22,
                TextAlignmentOptions.Left,
                new Color(0.86f, 0.86f, 0.9f, 1f));
            detailText.textWrappingMode = TextWrappingModes.Normal;
            SetRect(detailText.rectTransform, Anchor.TopLeft, new Vector2(0f, -74f), new Vector2(900f, 440f));

            HelpArchiveUI helpArchiveUI = panel.AddComponent<HelpArchiveUI>();
            Set(helpArchiveUI, "_entryList", leftListContent);
            Set(helpArchiveUI, "_entryButtonTemplate", helpEntryTemplate);
            Set(helpArchiveUI, "_titleText", detailTitle);
            Set(helpArchiveUI, "_subtitleText", subtitle);
            Set(helpArchiveUI, "_bodyText", detailText);

            Button closeButton = CreateTextButton(panel.transform, "CloseButton", "X", Anchor.TopRight, new Vector2(-62f, -32f), new Vector2(62f, 62f));
            UIPanelToggle toggle = closeButton.gameObject.AddComponent<UIPanelToggle>();
            Set(toggle, "_panel", panel);
            Set(toggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(closeButton.onClick, toggle.Hide);

            panel.SetActive(false);
            return panel;
        }

        private static void CreateHelpPrompt(Transform parent, GameObject helpPanel)
        {
            GameObject prompt = CreatePanel(parent, "HelpPrompt", Anchor.TopLeft, new Vector2(226f, -32f), new Vector2(64f, 64f), new Color(0.68f, 0.93f, 1f, 0.34f));
            prompt.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);

            TextMeshProUGUI text = CreateText(prompt.transform, "HelpText", "?", 24, TextAlignmentOptions.Center, new Color(0.08f, 0.18f, 0.22f, 1f));
            Stretch(text.rectTransform, new Vector2(6f, 6f), new Vector2(-6f, -6f));

            Button helpButton = prompt.AddComponent<Button>();
            AddButtonAudio(helpButton);
            helpButton.targetGraphic = prompt.GetComponent<Image>();
            UIPanelToggle helpToggle = prompt.AddComponent<UIPanelToggle>();
            Set(helpToggle, "_panel", helpPanel);
            Set(helpToggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(helpButton.onClick, helpToggle.Show);
        }

        private static void CreateQuestTracker(Transform parent, Sprite circleSprite, GameObject questPanel)
        {
            GameObject tracker = CreateRect(parent, "QuestTracker", Anchor.TopLeft, new Vector2(90f, -205f), new Vector2(430f, 84f));
            tracker.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);

            CreatePanel(tracker.transform, "QuestRibbon", Anchor.Left, new Vector2(0f, 4f), new Vector2(34f, 76f), new Color(0.97f, 0.97f, 0.91f, 0.92f));
            TextMeshProUGUI bang = CreateText(tracker.transform, "QuestBang", "!", 26, TextAlignmentOptions.Center, new Color(0.42f, 0.48f, 0.36f, 1f));
            SetRect(bang.rectTransform, Anchor.Left, new Vector2(0f, 4f), new Vector2(34f, 76f));

            GameObject diamond = CreateImage(tracker.transform, "QuestMarker", Anchor.Left, new Vector2(48f, -2f), new Vector2(28f, 28f), circleSprite, new Color(1f, 0.67f, 0.12f, 0.95f));
            diamond.transform.localEulerAngles = new Vector3(0f, 0f, 45f);

            TextMeshProUGUI objective = CreateText(tracker.transform, "ObjectiveText", "관리자가 알려준 작업대로 가기", 25, TextAlignmentOptions.Left, Color.white);
            objective.enableAutoSizing = true;
            objective.fontSizeMin = 16f;
            objective.fontSizeMax = 25f;
            SetRect(objective.rectTransform, Anchor.TopLeft, new Vector2(84f, -2f), new Vector2(330f, 38f));
            AddShadow(objective.gameObject);

            TextMeshProUGUI distance = CreateText(tracker.transform, "DistanceText", "49m", 17, TextAlignmentOptions.Left, new Color(1f, 0.78f, 0.25f, 1f));
            SetRect(distance.rectTransform, Anchor.TopLeft, new Vector2(84f, -42f), new Vector2(120f, 24f));
            AddShadow(distance.gameObject);

            QuestObjectiveHUD hud = tracker.AddComponent<QuestObjectiveHUD>();
            Set(hud, "_root", tracker);
            Set(hud, "_objectiveText", objective);
            Set(hud, "_distanceText", distance);
            Set(hud, "_markerIcon", diamond.GetComponent<Image>());

            Image clickImage = tracker.AddComponent<Image>();
            clickImage.color = new Color(1f, 1f, 1f, 0.01f);
            clickImage.raycastTarget = true;
            Button questButton = tracker.AddComponent<Button>();
            AddButtonAudio(questButton);
            questButton.targetGraphic = clickImage;
            UIPanelToggle questToggle = tracker.AddComponent<UIPanelToggle>();
            Set(questToggle, "_panel", questPanel);
            Set(questToggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(questButton.onClick, questToggle.Show);
        }

        private static GameObject CreateQuestPanel(Transform parent, QuestSystem questSystem)
        {
            GameObject panel = CreatePanel(parent, "QuestPanel", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.02f, 0.02f, 0.025f, 0.88f));
            Stretch(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            AddPanelMotion(panel);

            GameObject left = CreatePanel(panel.transform, "QuestListPanel", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.08f, 0.075f, 0.07f, 0.72f));
            RectTransform leftRect = left.GetComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0f, 0f);
            leftRect.anchorMax = new Vector2(0f, 1f);
            leftRect.pivot = new Vector2(0f, 0.5f);
            leftRect.anchoredPosition = new Vector2(42f, 0f);
            leftRect.sizeDelta = new Vector2(360f, -120f);

            TextMeshProUGUI listTitle = CreateText(left.transform, "QuestListTitle", "임무", 22, TextAlignmentOptions.Left, Color.white);
            SetRect(listTitle.rectTransform, Anchor.TopLeft, new Vector2(18f, -16f), new Vector2(220f, 36f));

            Button mainTabButton = CreateTextButton(left.transform, "MainQuestTabButton", "메인", Anchor.TopLeft, new Vector2(18f, -58f), new Vector2(150f, 40f));
            mainTabButton.GetComponent<Image>().color = new Color(0.95f, 0.87f, 0.62f, 0.92f);
            AddButtonAudio(mainTabButton);

            Button subTabButton = CreateTextButton(left.transform, "SubQuestTabButton", "서브", Anchor.TopLeft, new Vector2(184f, -58f), new Vector2(150f, 40f));
            subTabButton.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.18f);
            AddButtonAudio(subTabButton);

            Transform questListContent = CreateVerticalScrollContent(left.transform, "QuestScroll", new Vector2(16f, 116f), new Vector2(-16f, -18f), 8f, out _);
            Transform questList = questListContent;

            Button questButtonTemplate = CreateTextButton(questList.transform, "QuestButtonTemplate", "개척 임무\n심연으로 추락한 자들", Anchor.TopLeft, Vector2.zero, new Vector2(320f, 64f));
            questButtonTemplate.gameObject.SetActive(false);

            GameObject detail = CreateRect(panel.transform, "QuestDetailPanel", Anchor.Stretch, Vector2.zero, Vector2.zero);
            Stretch(detail.GetComponent<RectTransform>(), new Vector2(450f, 130f), new Vector2(-120f, -130f));

            TextMeshProUGUI title = CreateText(detail.transform, "QuestTitle", "심연으로 추락한 자들", 28, TextAlignmentOptions.Left, Color.white);
            SetRect(title.rectTransform, Anchor.TopLeft, Vector2.zero, new Vector2(620f, 42f));

            TextMeshProUGUI area = CreateText(detail.transform, "QuestArea", "아벨른-VI 큰 황금구역", 16, TextAlignmentOptions.Left, new Color(1f, 0.78f, 0.42f, 1f));
            SetRect(area.rectTransform, Anchor.TopLeft, new Vector2(0f, -44f), new Vector2(520f, 26f));

            TextMeshProUGUI objective = CreateText(detail.transform, "QuestObjective", "가정용 탐지기 로봇 부품 찾기", 20, TextAlignmentOptions.Left, new Color(0.9f, 0.9f, 0.9f, 1f));
            SetRect(objective.rectTransform, Anchor.TopLeft, new Vector2(0f, -84f), new Vector2(760f, 34f));

            TextMeshProUGUI description = CreateText(detail.transform, "QuestDescription", "의뢰 내용을 확인하고 다음 단서를 추적한다.", 18, TextAlignmentOptions.Left, new Color(0.78f, 0.78f, 0.78f, 1f));
            description.textWrappingMode = TextWrappingModes.Normal;
            SetRect(description.rectTransform, Anchor.TopLeft, new Vector2(0f, -136f), new Vector2(860f, 190f));

            GameObject rewardRoot = CreateRect(detail.transform, "RewardRoot", Anchor.BottomLeft, new Vector2(0f, 32f), new Vector2(620f, 96f));
            TextMeshProUGUI rewardTitle = CreateText(rewardRoot.transform, "RewardTitle", "보상 미리보기", 16, TextAlignmentOptions.Left, new Color(0.72f, 0.72f, 0.72f, 1f));
            SetRect(rewardTitle.rectTransform, Anchor.TopLeft, Vector2.zero, new Vector2(180f, 24f));

            GameObject rewardList = CreateRect(rewardRoot.transform, "RewardList", Anchor.Stretch, Vector2.zero, Vector2.zero);
            Stretch(rewardList.GetComponent<RectTransform>(), new Vector2(0f, 30f), Vector2.zero);
            HorizontalLayoutGroup rewardLayout = rewardList.AddComponent<HorizontalLayoutGroup>();
            rewardLayout.spacing = 8f;
            rewardLayout.childForceExpandHeight = false;
            rewardLayout.childForceExpandWidth = false;

            GameObject rewardTemplate = CreatePanel(rewardList.transform, "RewardItemTemplate", Anchor.Center, Vector2.zero, new Vector2(78f, 64f), new Color(0.2f, 0.22f, 0.28f, 0.82f));
            CreateImage(rewardTemplate.transform, "RewardIcon", Anchor.Center, new Vector2(0f, 8f), new Vector2(40f, 40f), null, new Color(1f, 1f, 1f, 0.18f));
            TextMeshProUGUI rewardAmount = CreateText(rewardTemplate.transform, "RewardLabel", "x1", 13, TextAlignmentOptions.BottomRight, Color.white);
            Stretch(rewardAmount.rectTransform, new Vector2(5f, 4f), new Vector2(-5f, -4f));
            rewardTemplate.SetActive(false);
            rewardRoot.SetActive(false);

            Button closeButton = CreateTextButton(panel.transform, "CloseButton", "X", Anchor.TopRight, new Vector2(-62f, -32f), new Vector2(62f, 62f));
            UIPanelToggle toggle = closeButton.gameObject.AddComponent<UIPanelToggle>();
            Set(toggle, "_panel", panel);
            Set(toggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(closeButton.onClick, toggle.Hide);

            QuestLogUI questLogUI = panel.AddComponent<QuestLogUI>();
            Set(questLogUI, "_root", panel);
            Set(questLogUI, "_questList", questList.transform);
            Set(questLogUI, "_questButtonTemplate", questButtonTemplate);
            Set(questLogUI, "_mainTabButton", mainTabButton);
            Set(questLogUI, "_subTabButton", subTabButton);
            Set(questLogUI, "_titleText", title);
            Set(questLogUI, "_areaText", area);
            Set(questLogUI, "_objectiveText", objective);
            Set(questLogUI, "_descriptionText", description);
            Set(questLogUI, "_rewardRoot", rewardRoot);
            Set(questLogUI, "_rewardList", rewardList.transform);
            Set(questLogUI, "_rewardItemTemplate", rewardTemplate);
            Set(questLogUI, "_questSystem", questSystem);

            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateArchivePanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "ArchivePanel", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.02f, 0.025f, 0.035f, 0.9f));
            Stretch(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            AddPanelMotion(panel);

            TextMeshProUGUI title = CreateText(panel.transform, "ArchiveTitle", "아카이브", 32, TextAlignmentOptions.Left, Color.white);
            SetRect(title.rectTransform, Anchor.TopLeft, new Vector2(80f, -60f), new Vector2(360f, 50f));

            TextMeshProUGUI description = CreateText(panel.transform, "ArchiveDescription", "획득한 편지, 인물 정보, 단서 기록을 확인하는 화면입니다.\n실제 아카이브 목록 UI는 이후 디자인 단계에서 이 영역에 배치하면 됩니다.", 20, TextAlignmentOptions.Left, new Color(0.82f, 0.84f, 0.88f, 1f));
            description.textWrappingMode = TextWrappingModes.Normal;
            SetRect(description.rectTransform, Anchor.TopLeft, new Vector2(80f, -130f), new Vector2(760f, 130f));

            Button closeButton = CreateTextButton(panel.transform, "CloseButton", "X", Anchor.TopRight, new Vector2(-62f, -32f), new Vector2(62f, 62f));
            UIPanelToggle toggle = closeButton.gameObject.AddComponent<UIPanelToggle>();
            Set(toggle, "_panel", panel);
            Set(toggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(closeButton.onClick, toggle.Hide);

            panel.SetActive(false);
            return panel;
        }

        private static void CreateArchiveInventoryBody(GameObject panel)
        {
            TextMeshProUGUI title = panel.transform.Find("ArchiveTitle")?.GetComponent<TextMeshProUGUI>();
            if (title != null)
            {
                title.text = "아카이브 / 인벤토리";
            }

            Transform oldDescription = panel.transform.Find("ArchiveDescription");
            if (oldDescription != null)
            {
                oldDescription.gameObject.SetActive(false);
            }

            TextMeshProUGUI capacity = CreateText(panel.transform, "CapacityText", "0/120", 18, TextAlignmentOptions.Right, new Color(0.86f, 0.86f, 0.78f, 1f));
            SetRect(capacity.rectTransform, Anchor.TopRight, new Vector2(-140f, -66f), new Vector2(160f, 34f));

            GameObject tabRoot = CreateRect(panel.transform, "TabRoot", Anchor.Top, new Vector2(0f, -96f), new Vector2(760f, 46f));
            HorizontalLayoutGroup tabLayout = tabRoot.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 10f;
            tabLayout.childForceExpandWidth = false;
            tabLayout.childForceExpandHeight = true;
            Button tabTemplate = CreateTextButton(tabRoot.transform, "TabButtonTemplate", "증거", Anchor.Center, Vector2.zero, new Vector2(106f, 40f));

            GameObject gridFrame = CreatePanel(panel.transform, "InventoryGridFrame", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.05f, 0.07f, 0.09f, 0.42f));
            Stretch(gridFrame.GetComponent<RectTransform>(), new Vector2(70f, 156f), new Vector2(-470f, -96f));

            Transform gridContent = CreateVerticalScrollContent(gridFrame.transform, "InventoryScroll", new Vector2(8f, 8f), new Vector2(-8f, -8f), 8f, out _);
            GridLayoutGroup grid = gridContent.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(92f, 104f);
            grid.spacing = new Vector2(10f, 10f);
            grid.childAlignment = TextAnchor.UpperLeft;

            GameObject slotTemplate = CreatePanel(gridContent, "ItemSlotTemplate", Anchor.TopLeft, Vector2.zero, new Vector2(92f, 104f), new Color(0.22f, 0.25f, 0.3f, 0.76f));
            Button slotButton = slotTemplate.AddComponent<Button>();
            AddButtonAudio(slotButton);
            slotButton.targetGraphic = slotTemplate.GetComponent<Image>();
            CreateImage(slotTemplate.transform, "Icon", Anchor.Top, new Vector2(0f, -8f), new Vector2(56f, 56f), null, new Color(1f, 1f, 1f, 0.2f));
            TextMeshProUGUI slotName = CreateText(slotTemplate.transform, "NameText", "Item", 13, TextAlignmentOptions.Center, Color.white);
            Stretch(slotName.rectTransform, new Vector2(5f, 62f), new Vector2(-5f, -18f));
            TextMeshProUGUI slotCount = CreateText(slotTemplate.transform, "CountText", "x1", 13, TextAlignmentOptions.BottomRight, new Color(1f, 0.94f, 0.72f, 1f));
            Stretch(slotCount.rectTransform, new Vector2(6f, 4f), new Vector2(-6f, -4f));

            GameObject detail = CreatePanel(panel.transform, "ItemDetailPanel", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.88f, 0.83f, 0.73f, 0.92f));
            RectTransform detailRect = detail.GetComponent<RectTransform>();
            detailRect.anchorMin = new Vector2(1f, 0f);
            detailRect.anchorMax = new Vector2(1f, 1f);
            detailRect.pivot = new Vector2(1f, 0.5f);
            detailRect.anchoredPosition = new Vector2(-72f, 30f);
            detailRect.sizeDelta = new Vector2(390f, -252f);

            Image detailIcon = CreateImage(detail.transform, "DetailIcon", Anchor.Top, new Vector2(0f, -54f), new Vector2(96f, 96f), null, new Color(1f, 1f, 1f, 0.18f)).GetComponent<Image>();
            TextMeshProUGUI detailName = CreateText(detail.transform, "DetailNameText", "아이템 없음", 26, TextAlignmentOptions.Center, new Color(0.18f, 0.14f, 0.1f, 1f));
            SetRect(detailName.rectTransform, Anchor.Top, new Vector2(0f, -158f), new Vector2(340f, 42f));
            TextMeshProUGUI detailCategory = CreateText(detail.transform, "DetailCategoryText", "", 16, TextAlignmentOptions.Center, new Color(0.47f, 0.38f, 0.25f, 1f));
            SetRect(detailCategory.rectTransform, Anchor.Top, new Vector2(0f, -202f), new Vector2(340f, 28f));
            TextMeshProUGUI detailDescription = CreateText(detail.transform, "DetailDescriptionText", "이 탭에 저장된 아이템이 없습니다.", 18, TextAlignmentOptions.TopLeft, new Color(0.22f, 0.19f, 0.15f, 1f));
            detailDescription.textWrappingMode = TextWrappingModes.Normal;
            Stretch(detailDescription.rectTransform, new Vector2(28f, 250f), new Vector2(-28f, -96f));
            Button removeButton = CreateTextButton(detail.transform, "RemoveButton", "삭제", Anchor.BottomRight, new Vector2(-32f, 32f), new Vector2(118f, 42f));

            InventoryArchiveUI inventoryUI = panel.AddComponent<InventoryArchiveUI>();
            Set(inventoryUI, "_gameState", AssetDatabase.LoadAssetAtPath<GameStateSO>(GameStatePath));
            Set(inventoryUI, "_tabRoot", tabRoot.transform);
            Set(inventoryUI, "_tabButtonTemplate", tabTemplate);
            Set(inventoryUI, "_gridRoot", gridContent);
            Set(inventoryUI, "_slotTemplate", slotTemplate);
            Set(inventoryUI, "_capacityText", capacity);
            Set(inventoryUI, "_detailNameText", detailName);
            Set(inventoryUI, "_detailCategoryText", detailCategory);
            Set(inventoryUI, "_detailDescriptionText", detailDescription);
            Set(inventoryUI, "_detailIcon", detailIcon);
            Set(inventoryUI, "_removeButton", removeButton);
        }

        private static GameObject CreateBoardPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "InferenceBoardPanel", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.025f, 0.022f, 0.02f, 0.9f));
            Stretch(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            AddPanelMotion(panel);

            TextMeshProUGUI title = CreateText(panel.transform, "BoardTitle", "추리보드", 32, TextAlignmentOptions.Left, Color.white);
            SetRect(title.rectTransform, Anchor.TopLeft, new Vector2(80f, -60f), new Vector2(360f, 50f));

            GameObject boardArea = CreatePanel(panel.transform, "BoardArea", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.12f, 0.1f, 0.085f, 0.82f));
            Stretch(boardArea.GetComponent<RectTransform>(), new Vector2(80f, 130f), new Vector2(-80f, -80f));

            TextMeshProUGUI description = CreateText(boardArea.transform, "BoardPlaceholder", "스토리 진행 중 추리보드가 해금되면 이 화면에서 단서 카드를 연결합니다.", 22, TextAlignmentOptions.Center, new Color(0.88f, 0.82f, 0.68f, 1f));
            Stretch(description.rectTransform, new Vector2(40f, 40f), new Vector2(-40f, -40f));

            Button closeButton = CreateTextButton(panel.transform, "CloseButton", "X", Anchor.TopRight, new Vector2(-62f, -32f), new Vector2(62f, 62f));
            UIPanelToggle toggle = closeButton.gameObject.AddComponent<UIPanelToggle>();
            Set(toggle, "_panel", panel);
            Set(toggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(closeButton.onClick, toggle.Hide);

            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateSettingsPanelV2(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "SettingsPanel", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.35f, 0.52f, 0.68f, 0.84f));
            Stretch(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            AddPanelMotion(panel);

            GameObject topBar = CreatePanel(panel.transform, "TopBar", Anchor.Top, Vector2.zero, new Vector2(0f, 92f), new Color(0.12f, 0.18f, 0.26f, 0.72f));
            RectTransform topRect = topBar.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.offsetMin = new Vector2(0f, -92f);
            topRect.offsetMax = Vector2.zero;

            TextMeshProUGUI title = CreateText(topBar.transform, "SettingsTitle", "Settings / Controls", 26, TextAlignmentOptions.Left, new Color(1f, 0.92f, 0.68f, 1f));
            SetRect(title.rectTransform, Anchor.Left, new Vector2(150f, 0f), new Vector2(440f, 52f));

            Button closeButton = CreateTextButton(panel.transform, "CloseButton", "X", Anchor.TopRight, new Vector2(-62f, -32f), new Vector2(62f, 62f));
            UIPanelToggle toggle = closeButton.gameObject.AddComponent<UIPanelToggle>();
            Set(toggle, "_panel", panel);
            Set(toggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(closeButton.onClick, toggle.Hide);

            GameObject leftNav = CreateRect(panel.transform, "SettingsNav", Anchor.Stretch, Vector2.zero, Vector2.zero);
            RectTransform navRect = leftNav.GetComponent<RectTransform>();
            navRect.anchorMin = new Vector2(0f, 0f);
            navRect.anchorMax = new Vector2(0f, 1f);
            navRect.pivot = new Vector2(0f, 0.5f);
            navRect.anchoredPosition = new Vector2(120f, -20f);
            navRect.sizeDelta = new Vector2(280f, -210f);
            VerticalLayoutGroup navLayout = leftNav.AddComponent<VerticalLayoutGroup>();
            navLayout.spacing = 22f;
            navLayout.childForceExpandHeight = false;
            navLayout.childForceExpandWidth = true;

            Button controlsTab = CreateSettingsNavItem(leftNav.transform, "Controls", true);
            Button keyboardTab = CreateSettingsNavItem(leftNav.transform, "Keyboard Info", false);
            Button audioTab = CreateSettingsNavItem(leftNav.transform, "Audio", false);
            Button saveTab = CreateSettingsNavItem(leftNav.transform, "Save / Load", false);
            Button resetTab = CreateSettingsNavItem(leftNav.transform, "Reset", false);

            GameObject content = CreateRect(panel.transform, "SettingsContent", Anchor.Stretch, Vector2.zero, Vector2.zero);
            Stretch(content.GetComponent<RectTransform>(), new Vector2(420f, 130f), new Vector2(-120f, -110f));

            GameObject controlsSection = CreateSettingsSection(content.transform, "ControlsSection", true);
            GameObject keyboardSection = CreateSettingsSection(content.transform, "KeyboardSection", false);
            GameObject audioSection = CreateSettingsSection(content.transform, "AudioSection", false);
            GameObject saveSection = CreateSettingsSection(content.transform, "SaveSection", false);
            GameObject resetSection = CreateSettingsSection(content.transform, "ResetSection", false);

            CreateSettingsTitle(controlsSection.transform, "Controls");
            GameObject controlsList = CreateRect(controlsSection.transform, "ControlInfoList", Anchor.TopLeft, new Vector2(0f, -84f), new Vector2(720f, 360f));
            VerticalLayoutGroup controlsLayout = controlsList.AddComponent<VerticalLayoutGroup>();
            controlsLayout.spacing = 10f;
            controlsLayout.childForceExpandHeight = false;
            controlsLayout.childForceExpandWidth = true;
            CreateInfoRow(controlsList.transform, "WASD", "이동");
            CreateInfoRow(controlsList.transform, "Mouse", "카메라 조작 / 관찰");
            CreateInfoRow(controlsList.transform, "Shift", "달리기");
            CreateInfoRow(controlsList.transform, "M", "맵 열기");
            CreateInfoRow(controlsList.transform, "Q", "임무 보드 열기");
            CreateInfoRow(controlsList.transform, "E", "편지 아카이브 열기");
            CreateInfoRow(controlsList.transform, "R", "추리보드 열기");
            CreateInfoRow(controlsList.transform, "Esc", "설정 열기 / 열린 UI 닫기");

            CreateSettingsTitle(keyboardSection.transform, "Keyboard Info");
            GameObject keyboardBox = CreatePanel(keyboardSection.transform, "KeyboardDiagram", Anchor.TopLeft, new Vector2(0f, -86f), new Vector2(560f, 230f), new Color(1f, 1f, 1f, 0.18f));
            CreateKeyboardKey(keyboardBox.transform, "W", new Vector2(0f, 54f));
            CreateKeyboardKey(keyboardBox.transform, "A", new Vector2(-54f, 0f));
            CreateKeyboardKey(keyboardBox.transform, "S", new Vector2(0f, 0f));
            CreateKeyboardKey(keyboardBox.transform, "D", new Vector2(54f, 0f));
            CreateKeyboardKey(keyboardBox.transform, "Shift", new Vector2(-132f, -66f), new Vector2(100f, 42f));
            CreateKeyboardKey(keyboardBox.transform, "M", new Vector2(132f, 54f));
            CreateKeyboardKey(keyboardBox.transform, "Q", new Vector2(186f, 54f));
            CreateKeyboardKey(keyboardBox.transform, "E", new Vector2(240f, 54f));
            CreateKeyboardKey(keyboardBox.transform, "R", new Vector2(294f, 54f));
            CreateKeyboardKey(keyboardBox.transform, "Esc", new Vector2(180f, -66f), new Vector2(76f, 42f));

            CreateSettingsTitle(audioSection.transform, "Audio");
            TextMeshProUGUI masterLabel = CreateText(audioSection.transform, "MasterVolumeLabel", "전체", 18, TextAlignmentOptions.Left, Color.white);
            SetRect(masterLabel.rectTransform, Anchor.TopLeft, new Vector2(0f, -92f), new Vector2(140f, 30f));
            Slider volumeSlider = CreateSlider(audioSection.transform, "MasterVolumeSlider", new Vector2(150f, -86f), new Vector2(420f, 32f));
            TextMeshProUGUI bgmLabel = CreateText(audioSection.transform, "BgmVolumeLabel", "배경음", 18, TextAlignmentOptions.Left, Color.white);
            SetRect(bgmLabel.rectTransform, Anchor.TopLeft, new Vector2(0f, -146f), new Vector2(140f, 30f));
            Slider bgmSlider = CreateSlider(audioSection.transform, "BgmVolumeSlider", new Vector2(150f, -140f), new Vector2(420f, 32f));
            TextMeshProUGUI sfxLabel = CreateText(audioSection.transform, "SfxVolumeLabel", "효과음", 18, TextAlignmentOptions.Left, Color.white);
            SetRect(sfxLabel.rectTransform, Anchor.TopLeft, new Vector2(0f, -200f), new Vector2(140f, 30f));
            Slider sfxSlider = CreateSlider(audioSection.transform, "SfxVolumeSlider", new Vector2(150f, -194f), new Vector2(420f, 32f));

            CreateSettingsTitle(saveSection.transform, "Save / Load");
            Button loadButton = CreateTextButton(saveSection.transform, "LoadButton", "저장된 게임 플레이 불러오기", Anchor.TopLeft, new Vector2(0f, -94f), new Vector2(330f, 52f));
            Button saveButton = CreateTextButton(saveSection.transform, "SaveButton", "게임 플레이 현황 저장하기", Anchor.TopLeft, new Vector2(350f, -94f), new Vector2(330f, 52f));

            CreateSettingsTitle(resetSection.transform, "Reset");
            Button resetButton = CreateTextButton(resetSection.transform, "ResetButton", "초기화", Anchor.TopLeft, new Vector2(0f, -94f), new Vector2(160f, 52f));

            TextMeshProUGUI status = CreateText(content.transform, "SaveStatusText", "", 18, TextAlignmentOptions.Left, new Color(0.95f, 0.95f, 0.86f, 1f));
            SetRect(status.rectTransform, Anchor.BottomLeft, new Vector2(0f, 20f), new Vector2(780f, 36f));

            SettingsPanelUI settings = panel.AddComponent<SettingsPanelUI>();
            Set(settings, "_masterVolumeSlider", volumeSlider);
            Set(settings, "_bgmVolumeSlider", bgmSlider);
            Set(settings, "_sfxVolumeSlider", sfxSlider);
            Set(settings, "_statusText", status);
            SetObjectArray(settings, "_tabButtons", new Object[] { controlsTab, keyboardTab, audioTab, saveTab, resetTab });
            SetObjectArray(settings, "_tabSections", new Object[] { controlsSection, keyboardSection, audioSection, saveSection, resetSection });
            UnityEventTools.AddPersistentListener(saveButton.onClick, settings.SaveGameplay);
            UnityEventTools.AddPersistentListener(loadButton.onClick, settings.LoadGameplay);
            UnityEventTools.AddPersistentListener(resetButton.onClick, settings.ResetGameplay);

            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateSettingsPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "SettingsPanel", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.35f, 0.52f, 0.68f, 0.84f));
            Stretch(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            AddPanelMotion(panel);

            GameObject topBar = CreatePanel(panel.transform, "TopBar", Anchor.Top, Vector2.zero, new Vector2(0f, 92f), new Color(0.12f, 0.18f, 0.26f, 0.72f));
            RectTransform topRect = topBar.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.offsetMin = new Vector2(0f, -92f);
            topRect.offsetMax = Vector2.zero;

            TextMeshProUGUI title = CreateText(topBar.transform, "SettingsTitle", "Settings / Controls", 26, TextAlignmentOptions.Left, new Color(1f, 0.92f, 0.68f, 1f));
            SetRect(title.rectTransform, Anchor.Left, new Vector2(150f, 0f), new Vector2(440f, 52f));

            Button closeButton = CreateTextButton(panel.transform, "CloseButton", "X", Anchor.TopRight, new Vector2(-62f, -32f), new Vector2(62f, 62f));
            UIPanelToggle toggle = closeButton.gameObject.AddComponent<UIPanelToggle>();
            Set(toggle, "_panel", panel);
            Set(toggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(closeButton.onClick, toggle.Hide);

            GameObject leftNav = CreateRect(panel.transform, "SettingsNav", Anchor.Stretch, Vector2.zero, Vector2.zero);
            RectTransform navRect = leftNav.GetComponent<RectTransform>();
            navRect.anchorMin = new Vector2(0f, 0f);
            navRect.anchorMax = new Vector2(0f, 1f);
            navRect.pivot = new Vector2(0f, 0.5f);
            navRect.anchoredPosition = new Vector2(120f, -20f);
            navRect.sizeDelta = new Vector2(280f, -210f);
            VerticalLayoutGroup navLayout = leftNav.AddComponent<VerticalLayoutGroup>();
            navLayout.spacing = 22f;
            navLayout.childForceExpandHeight = false;
            navLayout.childForceExpandWidth = true;
            Button controlsTab = CreateSettingsNavItem(leftNav.transform, "Controls", true);
            Button keyboardTab = CreateSettingsNavItem(leftNav.transform, "Keyboard Info", false);
            Button audioTab = CreateSettingsNavItem(leftNav.transform, "Audio", false);
            Button saveTab = CreateSettingsNavItem(leftNav.transform, "Save / Load", false);
            Button resetTab = CreateSettingsNavItem(leftNav.transform, "Reset", false);

            GameObject content = CreateRect(panel.transform, "SettingsContent", Anchor.Stretch, Vector2.zero, Vector2.zero);
            Stretch(content.GetComponent<RectTransform>(), new Vector2(420f, 130f), new Vector2(-120f, -110f));

            TextMeshProUGUI sectionTitle = CreateText(content.transform, "SectionTitle", "Controls", 30, TextAlignmentOptions.Left, Color.white);
            SetRect(sectionTitle.rectTransform, Anchor.TopLeft, Vector2.zero, new Vector2(360f, 48f));

            GameObject keyboardBox = CreatePanel(content.transform, "KeyboardDiagram", Anchor.TopLeft, new Vector2(0f, -86f), new Vector2(500f, 230f), new Color(1f, 1f, 1f, 0.18f));
            CreateKeyboardKey(keyboardBox.transform, "W", new Vector2(0f, 54f));
            CreateKeyboardKey(keyboardBox.transform, "A", new Vector2(-54f, 0f));
            CreateKeyboardKey(keyboardBox.transform, "S", new Vector2(0f, 0f));
            CreateKeyboardKey(keyboardBox.transform, "D", new Vector2(54f, 0f));
            CreateKeyboardKey(keyboardBox.transform, "Shift", new Vector2(-132f, -66f), new Vector2(100f, 42f));
            CreateKeyboardKey(keyboardBox.transform, "E", new Vector2(132f, 54f));
            CreateKeyboardKey(keyboardBox.transform, "Esc", new Vector2(180f, -66f), new Vector2(76f, 42f));

            GameObject controlsList = CreateRect(content.transform, "ControlInfoList", Anchor.TopLeft, new Vector2(540f, -84f), new Vector2(620f, 260f));
            VerticalLayoutGroup controlsLayout = controlsList.AddComponent<VerticalLayoutGroup>();
            controlsLayout.spacing = 10f;
            controlsLayout.childForceExpandHeight = false;
            controlsLayout.childForceExpandWidth = true;
            CreateInfoRow(controlsList.transform, "WASD", "이동");
            CreateInfoRow(controlsList.transform, "Mouse", "카메라 조작 / 관찰");
            CreateInfoRow(controlsList.transform, "Shift", "달리기");
            CreateInfoRow(controlsList.transform, "E", "상호작용");
            CreateInfoRow(controlsList.transform, "M / J / I / B", "지도 / 임무 / 아카이브 / 추리보드");
            CreateInfoRow(controlsList.transform, "Esc", "설정 열기 / 열린 UI 닫기");

            TextMeshProUGUI audioLabel = CreateText(content.transform, "AudioLabel", "사운드", 22, TextAlignmentOptions.Left, Color.white);
            SetRect(audioLabel.rectTransform, Anchor.TopLeft, new Vector2(0f, -340f), new Vector2(180f, 36f));
            TextMeshProUGUI masterLabel = CreateText(content.transform, "MasterVolumeLabel", "전체", 18, TextAlignmentOptions.Left, Color.white);
            SetRect(masterLabel.rectTransform, Anchor.TopLeft, new Vector2(180f, -310f), new Vector2(140f, 30f));
            Slider volumeSlider = CreateSlider(content.transform, "MasterVolumeSlider", new Vector2(320f, -304f), new Vector2(360f, 32f));
            TextMeshProUGUI bgmLabel = CreateText(content.transform, "BgmVolumeLabel", "배경음", 18, TextAlignmentOptions.Left, Color.white);
            SetRect(bgmLabel.rectTransform, Anchor.TopLeft, new Vector2(180f, -354f), new Vector2(140f, 30f));
            Slider bgmSlider = CreateSlider(content.transform, "BgmVolumeSlider", new Vector2(320f, -348f), new Vector2(360f, 32f));
            TextMeshProUGUI sfxLabel = CreateText(content.transform, "SfxVolumeLabel", "효과음", 18, TextAlignmentOptions.Left, Color.white);
            SetRect(sfxLabel.rectTransform, Anchor.TopLeft, new Vector2(180f, -398f), new Vector2(140f, 30f));
            Slider sfxSlider = CreateSlider(content.transform, "SfxVolumeSlider", new Vector2(320f, -392f), new Vector2(360f, 32f));

            Button loadButton = CreateTextButton(content.transform, "LoadButton", "저장된 게임 플레이 불러오기", Anchor.TopLeft, new Vector2(0f, -470f), new Vector2(330f, 52f));
            Button saveButton = CreateTextButton(content.transform, "SaveButton", "게임 플레이 현황 저장하기", Anchor.TopLeft, new Vector2(350f, -470f), new Vector2(330f, 52f));
            Button resetButton = CreateTextButton(content.transform, "ResetButton", "초기화", Anchor.TopLeft, new Vector2(700f, -470f), new Vector2(160f, 52f));

            TextMeshProUGUI status = CreateText(content.transform, "SaveStatusText", "", 18, TextAlignmentOptions.Left, new Color(0.95f, 0.95f, 0.86f, 1f));
            SetRect(status.rectTransform, Anchor.TopLeft, new Vector2(0f, -540f), new Vector2(780f, 36f));

            SettingsPanelUI settings = panel.AddComponent<SettingsPanelUI>();
            Set(settings, "_masterVolumeSlider", volumeSlider);
            Set(settings, "_bgmVolumeSlider", bgmSlider);
            Set(settings, "_sfxVolumeSlider", sfxSlider);
            Set(settings, "_statusText", status);
            SetObjectArray(settings, "_tabButtons", new Object[] { controlsTab, keyboardTab, audioTab, saveTab, resetTab });
            UnityEventTools.AddPersistentListener(saveButton.onClick, settings.SaveGameplay);
            UnityEventTools.AddPersistentListener(loadButton.onClick, settings.LoadGameplay);
            UnityEventTools.AddPersistentListener(resetButton.onClick, settings.ResetGameplay);

            panel.SetActive(false);
            return panel;
        }

        private static void CreateArchiveButtonV2(Transform parent, GameObject archivePanel)
        {
            GameObject buttonObject = CreateHudButton(parent, "ArchiveButton", "E", "편지 아카이브", new Vector2(-42f, -28f));
            Button button = buttonObject.GetComponent<Button>();
            UIPanelToggle toggle = buttonObject.AddComponent<UIPanelToggle>();
            Set(toggle, "_panel", archivePanel);
            Set(toggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(button.onClick, toggle.Show);
        }

        private static GameObject CreateInferenceBoardButtonV2(Transform parent, GameObject boardPanel)
        {
            GameObject buttonObject = CreateHudButton(parent, "InferenceBoardButton", "R", "추리보드", new Vector2(-182f, -28f));
            Button button = buttonObject.GetComponent<Button>();
            UIPanelToggle toggle = buttonObject.AddComponent<UIPanelToggle>();
            Set(toggle, "_panel", boardPanel);
            Set(toggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(button.onClick, toggle.Show);

            UnlockableHudButton unlockable = buttonObject.AddComponent<UnlockableHudButton>();
            Set(unlockable, "_unlocked", true);
            buttonObject.SetActive(true);
            return buttonObject;
        }

        private static void CreateSettingsButtonV2(Transform parent, GameObject settingsPanel)
        {
            GameObject buttonObject = CreateHudButton(parent, "SettingsButton", "Esc", "설정", new Vector2(-322f, -28f));
            Button button = buttonObject.GetComponent<Button>();
            UIPanelToggle toggle = buttonObject.AddComponent<UIPanelToggle>();
            Set(toggle, "_panel", settingsPanel);
            Set(toggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(button.onClick, toggle.Show);
        }

        private static void CreateArchiveButton(Transform parent, GameObject archivePanel)
        {
            GameObject buttonObject = CreateHudButton(parent, "ArchiveButton", "A", "아카이브", new Vector2(-42f, -28f));
            Button button = buttonObject.GetComponent<Button>();
            UIPanelToggle toggle = buttonObject.AddComponent<UIPanelToggle>();
            Set(toggle, "_panel", archivePanel);
            Set(toggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(button.onClick, toggle.Show);
        }

        private static GameObject CreateInferenceBoardButton(Transform parent, GameObject boardPanel)
        {
            GameObject buttonObject = CreateHudButton(parent, "InferenceBoardButton", "B", "추리보드", new Vector2(-182f, -28f));
            Button button = buttonObject.GetComponent<Button>();
            UIPanelToggle toggle = buttonObject.AddComponent<UIPanelToggle>();
            Set(toggle, "_panel", boardPanel);
            Set(toggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(button.onClick, toggle.Show);

            UnlockableHudButton unlockable = buttonObject.AddComponent<UnlockableHudButton>();
            Set(unlockable, "_unlocked", true);
            buttonObject.SetActive(true);
            return buttonObject;
        }

        private static void CreateSettingsButton(Transform parent, GameObject settingsPanel)
        {
            GameObject buttonObject = CreateHudButton(parent, "SettingsButton", "S", "설정", new Vector2(-322f, -28f));
            Button button = buttonObject.GetComponent<Button>();
            UIPanelToggle toggle = buttonObject.AddComponent<UIPanelToggle>();
            Set(toggle, "_panel", settingsPanel);
            Set(toggle, "_hideOnAwake", false);
            UnityEventTools.AddPersistentListener(button.onClick, toggle.Show);
        }

        private static GameObject CreateHudButton(Transform parent, string name, string iconText, string labelText, Vector2 position)
        {
            GameObject buttonObject = CreatePanel(parent, name, Anchor.TopRight, position, new Vector2(124f, 78f), new Color(0.05f, 0.08f, 0.09f, 0.44f));
            buttonObject.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            AddButtonAudio(button);
            button.targetGraphic = buttonObject.GetComponent<Image>();

            TextMeshProUGUI icon = CreateText(buttonObject.transform, "Icon", iconText, 28, TextAlignmentOptions.Center, Color.white);
            SetRect(icon.rectTransform, Anchor.Top, new Vector2(0f, -7f), new Vector2(112f, 32f));

            TextMeshProUGUI label = CreateText(buttonObject.transform, "Label", labelText, 16, TextAlignmentOptions.Center, Color.white);
            SetRect(label.rectTransform, Anchor.Bottom, new Vector2(0f, 9f), new Vector2(112f, 26f));
            return buttonObject;
        }

        private static void CreateActionSlotDock(Transform parent)
        {
            GameObject dock = CreateRect(parent, "ActionSlotDock", Anchor.BottomRight, new Vector2(-42f, 42f), new Vector2(280f, 180f));
            dock.GetComponent<RectTransform>().pivot = new Vector2(1f, 0f);
            VerticalLayoutGroup layout = dock.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            CreateActionSlot(dock.transform, "V", "소환");
            CreateActionSlot(dock.transform, "Mouse", "관찰");
            CreateActionSlot(dock.transform, "Shift", "달리기");
        }

        private static void CreateItemToastDock(Transform parent)
        {
            GameObject dock = CreateRect(parent, "ItemToastDock", Anchor.Right, new Vector2(-52f, -20f), new Vector2(330f, 260f));
            dock.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
            VerticalLayoutGroup layout = dock.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.LowerRight;

            GameObject template = CreatePanel(dock.transform, "ItemToastTemplate", Anchor.TopLeft, Vector2.zero, new Vector2(320f, 48f), new Color(0.05f, 0.06f, 0.05f, 0.55f));
            template.AddComponent<CanvasGroup>();
            CreateImage(template.transform, "Icon", Anchor.Left, new Vector2(28f, 0f), new Vector2(34f, 34f), null, new Color(1f, 1f, 1f, 0.24f));
            TextMeshProUGUI nameText = CreateText(template.transform, "NameText", "아이템", 17, TextAlignmentOptions.Left, new Color(0.92f, 0.92f, 0.84f, 1f));
            Stretch(nameText.rectTransform, new Vector2(56f, 6f), new Vector2(-74f, -6f));
            TextMeshProUGUI amountText = CreateText(template.transform, "AmountText", "x1", 17, TextAlignmentOptions.Right, new Color(0.98f, 0.92f, 0.72f, 1f));
            Stretch(amountText.rectTransform, new Vector2(230f, 6f), new Vector2(-14f, -6f));
            template.SetActive(false);

            ItemAcquisitionToastUI toastUI = dock.AddComponent<ItemAcquisitionToastUI>();
            Set(toastUI, "_toastRoot", dock.transform);
            Set(toastUI, "_toastTemplate", template);
        }

        private static void CreateInteractionPrompt(Transform parent)
        {
            GameObject root = CreatePanel(parent, "InteractionPrompt", Anchor.Bottom, new Vector2(0f, 126f), new Vector2(360f, 54f), new Color(0.04f, 0.04f, 0.045f, 0.58f));
            root.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
            GameObject key = CreatePanel(root.transform, "Key", Anchor.Left, new Vector2(36f, 0f), new Vector2(54f, 34f), new Color(1f, 1f, 1f, 0.86f));
            TextMeshProUGUI keyLabel = CreateText(key.transform, "KeyLabel", "F", 18, TextAlignmentOptions.Center, new Color(0.08f, 0.08f, 0.08f, 1f));
            Stretch(keyLabel.rectTransform, new Vector2(4f, 3f), new Vector2(-4f, -3f));
            TextMeshProUGUI label = CreateText(root.transform, "PromptText", "조사", 20, TextAlignmentOptions.Left, Color.white);
            Stretch(label.rectTransform, new Vector2(76f, 5f), new Vector2(-18f, -5f));

            InteractionPromptUI promptUI = root.AddComponent<InteractionPromptUI>();
            Set(promptUI, "_root", root);
            Set(promptUI, "_promptText", label);
            Set(promptUI, "_prefix", string.Empty);
            Set(promptUI, "_displayPriority", 10);

            CanvasGroup group = root.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = root.AddComponent<CanvasGroup>();
            }

            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        private static void CreateExplorationCrosshair(GameObject hudRoot)
        {
            Sprite circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
            GameObject root = CreateRect(hudRoot.transform, "ExplorationCrosshair", Anchor.Center, Vector2.zero, new Vector2(34f, 34f));
            CanvasGroup group = root.AddComponent<CanvasGroup>();
            group.alpha = 0.9f;
            group.blocksRaycasts = false;
            group.interactable = false;

            GameObject diamond = CreateImage(root.transform, "DiamondMark", Anchor.Center, Vector2.zero, new Vector2(19f, 19f), circleSprite, new Color(1f, 0.94f, 0.72f, 0.78f));
            diamond.transform.localEulerAngles = new Vector3(0f, 0f, 45f);

            GameObject center = CreateImage(root.transform, "CenterDot", Anchor.Center, Vector2.zero, new Vector2(6f, 6f), circleSprite, new Color(1f, 1f, 1f, 0.96f));
            center.transform.SetAsLastSibling();

            ExplorationCursorController cursorController = hudRoot.AddComponent<ExplorationCursorController>();
            Set(cursorController, "_crosshairRoot", root);
        }

        private static void CreateQuestUnlockToast(Transform parent)
        {
            GameObject root = CreateRect(parent, "QuestUnlockToast", Anchor.Top, new Vector2(0f, 118f), new Vector2(640f, 96f));
            CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            GameObject banner = CreatePanel(root.transform, "Banner", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.55f, 0.18f, 0.08f, 0.92f));
            Stretch(banner.GetComponent<RectTransform>(), new Vector2(52f, 14f), new Vector2(-52f, -14f));

            GameObject leftWing = CreatePanel(root.transform, "LeftWing", Anchor.Left, new Vector2(38f, 0f), new Vector2(108f, 28f), new Color(1f, 0.78f, 0.35f, 0.72f));
            leftWing.transform.localEulerAngles = new Vector3(0f, 0f, -12f);

            GameObject rightWing = CreatePanel(root.transform, "RightWing", Anchor.Right, new Vector2(-38f, 0f), new Vector2(108f, 28f), new Color(1f, 0.78f, 0.35f, 0.72f));
            rightWing.transform.localEulerAngles = new Vector3(0f, 0f, 12f);

            GameObject jewel = CreatePanel(root.transform, "CenterJewel", Anchor.Center, new Vector2(-216f, 0f), new Vector2(46f, 46f), new Color(0.12f, 0.55f, 0.58f, 0.96f));
            jewel.transform.localEulerAngles = new Vector3(0f, 0f, 45f);

            TextMeshProUGUI message = CreateText(root.transform, "Message", "새 임무 개방", 22, TextAlignmentOptions.Center, new Color(1f, 0.92f, 0.7f, 1f));
            SetRect(message.rectTransform, Anchor.Center, new Vector2(42f, 0f), new Vector2(460f, 42f));
            AddShadow(message.gameObject);

            QuestUnlockToastUI toast = root.AddComponent<QuestUnlockToastUI>();
            Set(toast, "_root", root);
            Set(toast, "_banner", root.GetComponent<RectTransform>());
            Set(toast, "_canvasGroup", canvasGroup);
            Set(toast, "_messageText", message);

            root.SetActive(false);
        }

        private static GameObject CreateUnlockOverlay(Transform parent)
        {
            GameObject overlay = CreatePanel(parent, "FeatureUnlockOverlay", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.02f, 0.025f, 0.035f, 0.68f));
            Stretch(overlay.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

            GameObject banner = CreatePanel(overlay.transform, "UnlockBanner", Anchor.Center, Vector2.zero, new Vector2(680f, 210f), new Color(0.08f, 0.1f, 0.14f, 0.92f));
            TextMeshProUGUI title = CreateText(overlay.transform, "UnlockTitle", "새 기능 해금", 34, TextAlignmentOptions.Center, new Color(1f, 0.88f, 0.62f, 1f));
            SetRect(title.rectTransform, Anchor.Center, new Vector2(0f, 42f), new Vector2(620f, 58f));
            TextMeshProUGUI description = CreateText(overlay.transform, "UnlockDescription", "새로운 UI 기능을 사용할 수 있습니다.", 22, TextAlignmentOptions.Center, Color.white);
            SetRect(description.rectTransform, Anchor.Center, new Vector2(0f, -30f), new Vector2(620f, 46f));

            overlay.SetActive(false);
            return overlay;
        }

        private static void CreateActionSlot(Transform parent, string keyText, string labelText)
        {
            GameObject slot = CreatePanel(parent, $"ActionSlot_{keyText}", Anchor.TopLeft, Vector2.zero, new Vector2(270f, 48f), new Color(0.05f, 0.05f, 0.05f, 0.34f));
            TextMeshProUGUI label = CreateText(slot.transform, "Label", labelText, 18, TextAlignmentOptions.Right, Color.white);
            SetRect(label.rectTransform, Anchor.Stretch, new Vector2(12f, 4f), new Vector2(-80f, -4f));

            GameObject key = CreatePanel(slot.transform, "Key", Anchor.Right, new Vector2(-26f, 0f), new Vector2(64f, 32f), new Color(1f, 1f, 1f, 0.86f));
            TextMeshProUGUI keyLabel = CreateText(key.transform, "KeyLabel", keyText, 15, TextAlignmentOptions.Center, new Color(0.08f, 0.08f, 0.08f, 1f));
            Stretch(keyLabel.rectTransform, new Vector2(4f, 3f), new Vector2(-4f, -3f));
        }

        private static void EnsureEventSystem()
        {
            EventSystem existing = Object.FindFirstObjectByType<EventSystem>();
            if (existing != null)
            {
                ReplaceLegacyInputModule(existing.gameObject);
                return;
            }

            GameObject eventSystem = new("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private static void ReplaceLegacyInputModule(GameObject eventSystem)
        {
            BaseInputModule[] inputModules = eventSystem.GetComponents<BaseInputModule>();
            foreach (BaseInputModule inputModule in inputModules)
            {
                if (inputModule != null && inputModule is not InputSystemUIInputModule)
                {
                    Object.DestroyImmediate(inputModule);
                }
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystem.AddComponent<InputSystemUIInputModule>();
            }
        }

        private static Sprite GetOrCreateCircleSprite()
        {
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
            if (existing != null)
            {
                return existing;
            }

            EnsureFolder("Assets/Art");
            EnsureFolder("Assets/Art/UI");

            const int size = 128;
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.47f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01((radius - distance) / 4f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            File.WriteAllBytes(CircleSpritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(CircleSpritePath);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(CircleSpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
        }

        private static GameObject CreateRect(Transform parent, string name, Anchor anchor, Vector2 position, Vector2 size)
        {
            return CreateRect(parent, name, anchor.Min, anchor.Max, position, size);
        }

        private static GameObject CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            GameObject obj = new(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return obj;
        }

        private static GameObject CreatePanel(Transform parent, string name, Anchor anchor, Vector2 position, Vector2 size, Color color)
        {
            GameObject panel = CreateRect(parent, name, anchor, position, size);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static void AddPanelMotion(GameObject panel)
        {
            AddPanelMotion(panel, UIPanelMotionPreset.GenericPanel);
        }

        private static void AddPanelMotion(GameObject panel, UIPanelMotionPreset preset)
        {
            if (panel.GetComponent<CanvasGroup>() == null)
            {
                panel.AddComponent<CanvasGroup>();
            }

            UIPanelAnimator animator = panel.GetComponent<UIPanelAnimator>();
            if (animator == null)
            {
                animator = panel.AddComponent<UIPanelAnimator>();
            }

            SetEnum(animator, "_motionPreset", (int)preset);
        }

        private static void AddButtonAudio(Button button)
        {
            if (button != null && button.GetComponent<UIButtonAudioFeedback>() == null)
            {
                button.gameObject.AddComponent<UIButtonAudioFeedback>();
            }
        }

        private static Transform CreateVerticalScrollContent(
            Transform parent,
            string name,
            Vector2 insetMin,
            Vector2 insetMax,
            float spacing,
            out ScrollRect scrollRect)
        {
            GameObject root = CreatePanel(parent, name, Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.035f));
            Stretch(root.GetComponent<RectTransform>(), insetMin, insetMax);

            scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 32f;

            GameObject viewport = CreatePanel(root.transform, "Viewport", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0f));
            Stretch(viewport.GetComponent<RectTransform>(), Vector2.zero, new Vector2(-16f, 0f));
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            GameObject content = CreateRect(viewport.transform, "Content", Anchor.Stretch, Vector2.zero, Vector2.zero);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 12, 12);
            layout.spacing = spacing;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject scrollbarObject = CreatePanel(root.transform, "Scrollbar", Anchor.Right, new Vector2(-6f, 0f), new Vector2(8f, 0f), new Color(1f, 1f, 1f, 0.14f));
            RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
            scrollbarRect.anchorMin = new Vector2(1f, 0f);
            scrollbarRect.anchorMax = new Vector2(1f, 1f);
            scrollbarRect.offsetMin = new Vector2(-12f, 10f);
            scrollbarRect.offsetMax = new Vector2(-4f, -10f);

            GameObject handle = CreatePanel(scrollbarObject.transform, "Handle", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(1f, 0.92f, 0.68f, 0.74f));
            Stretch(handle.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

            Scrollbar scrollbar = scrollbarObject.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.targetGraphic = handle.GetComponent<Image>();
            scrollbar.handleRect = handle.GetComponent<RectTransform>();

            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = contentRect;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

            return content.transform;
        }

        private static GameObject CreateImage(Transform parent, string name, Anchor anchor, Vector2 position, Vector2 size, Sprite sprite, Color color)
        {
            GameObject imageObject = CreateRect(parent, name, anchor, position, size);
            Image image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return imageObject;
        }

        private static Button CreateTextButton(Transform parent, string name, string text, Anchor anchor, Vector2 position, Vector2 size)
        {
            GameObject buttonObject = CreatePanel(parent, name, anchor, position, size, new Color(0.9f, 0.9f, 0.9f, 0.92f));
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();

            TextMeshProUGUI label = CreateText(buttonObject.transform, "Label", text, 18, TextAlignmentOptions.Center, new Color(0.08f, 0.08f, 0.08f, 1f));
            Stretch(label.rectTransform, new Vector2(8f, 6f), new Vector2(-8f, -6f));
            return button;
        }

        private static void CreateHelpListItem(Transform parent, string text, bool selected)
        {
            GameObject item = CreatePanel(parent, $"HelpItem_{text}", Anchor.TopLeft, Vector2.zero, new Vector2(370f, 58f), selected ? new Color(0.95f, 0.9f, 0.78f, 0.18f) : new Color(0.15f, 0.18f, 0.25f, 0.58f));
            TextMeshProUGUI label = CreateText(item.transform, "Label", text, 20, TextAlignmentOptions.Left, selected ? new Color(1f, 0.88f, 0.55f, 1f) : new Color(0.82f, 0.82f, 0.86f, 1f));
            Stretch(label.rectTransform, new Vector2(22f, 8f), new Vector2(-12f, -8f));
        }

        private static Button CreateSettingsNavItem(Transform parent, string text, bool selected)
        {
            GameObject item = CreatePanel(parent, $"Nav_{text}", Anchor.TopLeft, Vector2.zero, new Vector2(260f, 48f), selected ? new Color(1f, 1f, 1f, 0.18f) : new Color(1f, 1f, 1f, 0.04f));
            Button button = item.AddComponent<Button>();
            AddButtonAudio(button);
            button.targetGraphic = item.GetComponent<Image>();
            TextMeshProUGUI label = CreateText(item.transform, "Label", selected ? $"◇ {text}" : $"◆ {text}", selected ? 28 : 24, TextAlignmentOptions.Left, selected ? Color.white : new Color(0.94f, 0.94f, 0.86f, 0.92f));
            Stretch(label.rectTransform, new Vector2(14f, 4f), new Vector2(-10f, -4f));
            return button;
        }

        private static GameObject CreateSettingsSection(Transform parent, string name, bool active)
        {
            GameObject section = CreateRect(parent, name, Anchor.Stretch, Vector2.zero, Vector2.zero);
            Stretch(section.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            section.SetActive(active);
            return section;
        }

        private static TextMeshProUGUI CreateSettingsTitle(Transform parent, string text)
        {
            TextMeshProUGUI title = CreateText(parent, "SectionTitle", text, 30, TextAlignmentOptions.Left, Color.white);
            SetRect(title.rectTransform, Anchor.TopLeft, Vector2.zero, new Vector2(420f, 48f));
            return title;
        }

        private static void CreateKeyboardKey(Transform parent, string text, Vector2 position)
        {
            CreateKeyboardKey(parent, text, position, new Vector2(46f, 42f));
        }

        private static void CreateKeyboardKey(Transform parent, string text, Vector2 position, Vector2 size)
        {
            GameObject key = CreatePanel(parent, $"Key_{text}", Anchor.Center, position, size, new Color(1f, 1f, 1f, 0.82f));
            TextMeshProUGUI label = CreateText(key.transform, "Label", text, 15, TextAlignmentOptions.Center, new Color(0.1f, 0.12f, 0.16f, 1f));
            Stretch(label.rectTransform, new Vector2(4f, 4f), new Vector2(-4f, -4f));
        }

        private static void CreateInfoRow(Transform parent, string keyText, string descriptionText)
        {
            GameObject row = CreatePanel(parent, $"Info_{keyText}", Anchor.TopLeft, Vector2.zero, new Vector2(590f, 36f), new Color(1f, 1f, 1f, 0.12f));
            TextMeshProUGUI key = CreateText(row.transform, "Key", keyText, 17, TextAlignmentOptions.Left, new Color(1f, 0.92f, 0.68f, 1f));
            SetRect(key.rectTransform, Anchor.Left, new Vector2(14f, 0f), new Vector2(160f, 32f));
            TextMeshProUGUI description = CreateText(row.transform, "Description", descriptionText, 17, TextAlignmentOptions.Left, Color.white);
            SetRect(description.rectTransform, Anchor.Stretch, new Vector2(180f, 3f), new Vector2(-10f, -3f));
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 position, Vector2 size)
        {
            GameObject root = CreateRect(parent, name, Anchor.TopLeft, position, size);
            Slider slider = root.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            GameObject background = CreatePanel(root.transform, "Background", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.28f));
            Stretch(background.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

            GameObject fillArea = CreateRect(root.transform, "Fill Area", Anchor.Stretch, Vector2.zero, Vector2.zero);
            Stretch(fillArea.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            GameObject fill = CreatePanel(fillArea.transform, "Fill", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(1f, 0.88f, 0.38f, 0.9f));
            Stretch(fill.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

            GameObject handleArea = CreateRect(root.transform, "Handle Slide Area", Anchor.Stretch, Vector2.zero, Vector2.zero);
            Stretch(handleArea.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            GameObject handle = CreatePanel(handleArea.transform, "Handle", Anchor.Center, Vector2.zero, new Vector2(28f, 28f), Color.white);

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            return slider;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment, Color color)
        {
            GameObject obj = CreateRect(parent, name, Anchor.Center, Vector2.zero, new Vector2(220f, 40f));
            TextMeshProUGUI label = obj.AddComponent<TextMeshProUGUI>();
            label.text = text;
            TMP_FontAsset defaultFont = GetDefaultFontAsset();
            if (defaultFont != null)
            {
                label.font = defaultFont;
            }

            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = color;
            label.raycastTarget = false;
            label.margin = Vector4.zero;
            return label;
        }

        private static TMP_FontAsset GetDefaultFontAsset()
        {
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultFontAssetPath);
        }

        private static void SetRect(RectTransform rect, Anchor anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor.Min;
            rect.anchorMax = anchor.Max;
            rect.pivot = anchor.Pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void AddShadow(GameObject obj)
        {
            Shadow shadow = obj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private static void Set(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }

        private static void Set(Object target, string propertyName, bool value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }

        private static void Set(Object target, string propertyName, string value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }

        private static void Set(Object target, string propertyName, int value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }

        private static void SetEnum(Object target, string propertyName, int value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.enumValueIndex = value;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }

        private static void SetObjectArray(Object target, string propertyName, Object[] values)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
            {
                Debug.LogWarning($"[DLOSceneSetupTools] Missing array field {propertyName} on {target.name}.");
                return;
            }

            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private readonly struct Anchor
        {
            public static readonly Anchor Center = new(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            public static readonly Anchor Stretch = new(Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            public static readonly Anchor TopLeft = new(new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            public static readonly Anchor TopRight = new(new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            public static readonly Anchor Top = new(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            public static readonly Anchor Bottom = new(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            public static readonly Anchor BottomLeft = new(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            public static readonly Anchor BottomRight = new(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));
            public static readonly Anchor Left = new(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            public static readonly Anchor Right = new(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));

            public readonly Vector2 Min;
            public readonly Vector2 Max;
            public readonly Vector2 Pivot;

            private Anchor(Vector2 min, Vector2 max, Vector2 pivot)
            {
                Min = min;
                Max = max;
                Pivot = pivot;
            }
        }
    }
}
