using DeadLetterOffice.Dialogue;
using DeadLetterOffice.State;
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
    public static class DLODialogueSetupTools
    {
        private const string DevScenePath = "Assets/Scenes/DevScenes/DevScen.unity";
        private const string CanvasName = "DLO_DialogueCanvas";
        private const string DefaultFontAssetPath = "Assets/Art/Fonts/GowunBatang-Regular SDF.asset";
        private static bool s_buildInCurrentScene;

        [MenuItem("DLO/Setup/Rebuild DevScene Dialogue UI")]
        public static void BuildDevSceneDialogueUI()
        {
            if (!s_buildInCurrentScene)
            {
                OpenSceneIfExists(DevScenePath);
            }

            ClearPreviousDialogueCanvas();
            Canvas canvas = CreateDialogueCanvas();
            GameObject dialogueSystem = CreateDialogueSystem(canvas.transform);

            DialogueUI dialogueUI = dialogueSystem.AddComponent<DialogueUI>();
            DialogueManager manager = dialogueSystem.AddComponent<DialogueManager>();
            DialogueInputController input = dialogueSystem.AddComponent<DialogueInputController>();

            GameObject root = CreateDialogueRoot(canvas.transform);
            GameObject portrait = CreatePortrait(root.transform);
            GameObject illustrationPanel = CreateIllustrationPanel(root.transform);
            GameObject box = CreateDialogueBox(root.transform);
            TextMeshProUGUI nameText = CreateText(box.transform, "SpeakerNameText", "관리자", 26, TextAlignmentOptions.Left, new Color(1f, 0.88f, 0.62f, 1f));
            SetRect(nameText.rectTransform, new Vector2(34f, -24f), new Vector2(380f, 42f), Anchor.TopLeft);

            TextMeshProUGUI dialogueText = CreateText(box.transform, "DialogueText", "여기에 대사가 표시됩니다.", 28, TextAlignmentOptions.TopLeft, Color.white);
            dialogueText.textWrappingMode = TextWrappingModes.Normal;
            dialogueText.lineSpacing = 12f;
            SetRect(dialogueText.rectTransform, new Vector2(34f, -78f), new Vector2(980f, 142f), Anchor.TopLeft);

            GameObject advanceIndicator = CreateAdvanceIndicator(box.transform);

            Transform choiceContainer = CreateChoiceContainer(root.transform).transform;
            Button choiceButtonPrefab = CreateChoiceButtonTemplate(choiceContainer);

            Set(dialogueUI, "_root", root);
            Set(dialogueUI, "_characterImage", portrait.GetComponent<Image>());
            Set(dialogueUI, "_illustrationPanel", illustrationPanel);
            Set(dialogueUI, "_illustrationImage", illustrationPanel.transform.Find("IllustrationImage")?.GetComponent<Image>());
            Set(dialogueUI, "_nameText", nameText);
            Set(dialogueUI, "_dialogueText", dialogueText);
            Set(dialogueUI, "_choiceContainer", choiceContainer);
            Set(dialogueUI, "_choiceButtonPrefab", choiceButtonPrefab);
            Set(dialogueUI, "_advanceIndicator", advanceIndicator);
            Set(dialogueUI, "_charactersPerSecond", 26f);
            Set(dialogueUI, "_fadeDuration", 0.32f);
            Set(dialogueUI, "_lineStartDelay", 0.35f);
            Set(dialogueUI, "_choiceFadeDuration", 0.22f);
            Set(dialogueUI, "_choiceStaggerDelay", 0.055f);
            Set(manager, "_dialogueUI", dialogueUI);
            Set(manager, "_gameState", FindFirstAsset<GameStateSO>());
            Set(input, "_dialogueManager", manager);

            root.SetActive(false);
            EnsureEventSystem();

            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DLODialogueSetupTools] DevScene_UI dialogue canvas rebuilt.");
        }

        [MenuItem("DLO/Setup/Rebuild Current Scene Dialogue UI")]
        public static void BuildCurrentSceneDialogueUI()
        {
            s_buildInCurrentScene = true;
            try
            {
                BuildDevSceneDialogueUI();
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
                Debug.LogWarning($"[DLODialogueSetupTools] Scene not found at {scenePath}. Building in the currently open scene instead.");
                return;
            }

            EditorSceneManager.OpenScene(scenePath);
        }

        private static void ClearPreviousDialogueCanvas()
        {
            GameObject previous = GameObject.Find(CanvasName);
            if (previous != null)
            {
                Object.DestroyImmediate(previous);
            }
        }

        private static Canvas CreateDialogueCanvas()
        {
            GameObject canvasObject = new(CanvasName);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreateDialogueSystem(Transform parent)
        {
            GameObject system = new("DialogueSystem");
            system.transform.SetParent(parent, false);
            RectTransform rect = system.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return system;
        }

        private static GameObject CreateDialogueRoot(Transform parent)
        {
            GameObject root = CreatePanel(parent, "DialogueRoot", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0f));
            Stretch(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            return root;
        }

        private static GameObject CreatePortrait(Transform parent)
        {
            GameObject portrait = CreatePanel(parent, "CharacterPortrait", Anchor.BottomLeft, new Vector2(110f, 120f), new Vector2(360f, 500f), new Color(1f, 1f, 1f, 0f));
            Image image = portrait.GetComponent<Image>();
            image.preserveAspect = true;
            image.enabled = false;
            return portrait;
        }

        private static GameObject CreateIllustrationPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "IllustrationPanel", Anchor.Center, new Vector2(0f, 96f), new Vector2(780f, 500f), new Color(0.02f, 0.025f, 0.04f, 0.9f));
            Image background = panel.GetComponent<Image>();
            background.raycastTarget = false;

            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.86f, 0.56f, 0.32f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            GameObject imageObject = CreatePanel(panel.transform, "IllustrationImage", Anchor.Stretch, Vector2.zero, Vector2.zero, Color.white);
            Stretch(imageObject.GetComponent<RectTransform>(), new Vector2(18f, 18f), new Vector2(-18f, -18f));
            Image image = imageObject.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;

            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateDialogueBox(Transform parent)
        {
            GameObject box = CreatePanel(parent, "DialogueBox", Anchor.Bottom, new Vector2(0f, 54f), new Vector2(1180f, 238f), new Color(0.04f, 0.05f, 0.08f, 0.88f));
            Image image = box.GetComponent<Image>();
            image.raycastTarget = true;

            Outline outline = box.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.86f, 0.56f, 0.36f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            return box;
        }

        private static GameObject CreateAdvanceIndicator(Transform parent)
        {
            GameObject root = CreateRect(parent, "AdvanceIndicator", Anchor.Bottom, new Vector2(0f, 24f), new Vector2(46f, 26f));
            CanvasGroup group = root.AddComponent<CanvasGroup>();
            group.alpha = 0.92f;
            group.blocksRaycasts = false;
            group.interactable = false;

            GameObject left = CreatePanel(root.transform, "LeftWing", Anchor.Center, new Vector2(-8f, 0f), new Vector2(22f, 5f), new Color(1f, 0.88f, 0.58f, 1f));
            left.transform.localEulerAngles = new Vector3(0f, 0f, -35f);
            left.GetComponent<Image>().raycastTarget = false;

            GameObject right = CreatePanel(root.transform, "RightWing", Anchor.Center, new Vector2(8f, 0f), new Vector2(22f, 5f), new Color(1f, 0.88f, 0.58f, 1f));
            right.transform.localEulerAngles = new Vector3(0f, 0f, 35f);
            right.GetComponent<Image>().raycastTarget = false;

            root.SetActive(false);
            return root;
        }

        private static GameObject CreateChoiceContainer(Transform parent)
        {
            GameObject container = CreatePanel(parent, "ChoiceContainer", Anchor.Right, new Vector2(-150f, -12f), new Vector2(430f, 360f), new Color(0f, 0f, 0f, 0f));
            VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            return container;
        }

        private static Button CreateChoiceButtonTemplate(Transform parent)
        {
            Button button = CreateTextButton(parent, "ChoiceButtonTemplate", "선택지", new Vector2(0f, 0f), new Vector2(400f, 58f));
            button.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.2f, 0.92f);
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
            label.color = Color.white;
            label.fontSize = 20f;
            button.gameObject.SetActive(false);
            return button;
        }

        private static Button CreateTextButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
        {
            GameObject buttonObject = CreatePanel(parent, name, Anchor.Top, position, size, new Color(0.9f, 0.9f, 0.9f, 0.92f));
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();
            TextMeshProUGUI label = CreateText(buttonObject.transform, "Label", text, 18, TextAlignmentOptions.Center, new Color(0.08f, 0.08f, 0.08f, 1f));
            Stretch(label.rectTransform, new Vector2(12f, 5f), new Vector2(-12f, -5f));
            return button;
        }

        private static GameObject CreatePanel(Transform parent, string name, Anchor anchor, Vector2 position, Vector2 size, Color color)
        {
            GameObject panel = CreateRect(parent, name, anchor, position, size);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static GameObject CreateRect(Transform parent, string name, Anchor anchor, Vector2 position, Vector2 size)
        {
            GameObject obj = new(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            SetRect(rect, position, size, anchor);
            return obj;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment, Color color)
        {
            GameObject obj = CreateRect(parent, name, Anchor.Center, Vector2.zero, new Vector2(220f, 40f));
            TextMeshProUGUI label = obj.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = color;
            label.raycastTarget = false;

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultFontAssetPath);
            if (font != null)
            {
                label.font = font;
            }

            return label;
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObject = new("EventSystem");
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

            StandaloneInputModule legacyInput = eventSystem.GetComponent<StandaloneInputModule>();
            if (legacyInput != null)
            {
                Object.DestroyImmediate(legacyInput);
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }

        private static void Set(Object target, string fieldName, object value)
        {
            System.Reflection.FieldInfo field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(target, value);
            EditorUtility.SetDirty(target);
        }

        private static T FindFirstAsset<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids == null || guids.Length == 0)
            {
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void SetRect(RectTransform rect, Vector2 position, Vector2 size, Anchor anchor)
        {
            switch (anchor)
            {
                case Anchor.TopLeft:
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0f, 1f);
                    break;
                case Anchor.Top:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    break;
                case Anchor.TopRight:
                    rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(1f, 1f);
                    break;
                case Anchor.BottomLeft:
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
                    rect.pivot = new Vector2(0f, 0f);
                    break;
                case Anchor.Bottom:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);
                    break;
                case Anchor.BottomRight:
                    rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
                    rect.pivot = new Vector2(1f, 0f);
                    break;
                case Anchor.Right:
                    rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
                    rect.pivot = new Vector2(1f, 0.5f);
                    break;
                case Anchor.Stretch:
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    break;
                default:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    break;
            }

            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private enum Anchor
        {
            Center,
            TopLeft,
            Top,
            TopRight,
            BottomLeft,
            Bottom,
            BottomRight,
            Right,
            Stretch
        }
    }
}
