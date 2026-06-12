using DeadLetterOffice.Interaction;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DeadLetterOffice.Editor
{
    public static class DLOInteractionSetupTools
    {
        private const string DevScenePath = "Assets/Scenes/DevScenes/DevScen.unity";
        private const string CanvasName = "DLO_InteractionCanvas";
        private const string DefaultFontAssetPath = "Assets/Art/Fonts/GowunBatang-Regular SDF.asset";
        private static bool s_buildInCurrentScene;

        [MenuItem("DLO/Setup/Rebuild DevScene Interaction UI")]
        public static void BuildDevSceneInteractionUI()
        {
            if (!s_buildInCurrentScene)
            {
                OpenSceneIfExists(DevScenePath);
            }

            ClearPreviousCanvas();
            DLOSceneSetupTools.BuildCurrentSceneMainHud();
            Debug.Log("[DLOInteractionSetupTools] Legacy interaction canvas removed. Interaction prompt is now rebuilt inside DLO_MainHUD.");
        }

        [MenuItem("DLO/Setup/Rebuild Current Scene Interaction UI")]
        public static void BuildCurrentSceneInteractionUI()
        {
            s_buildInCurrentScene = true;
            try
            {
                BuildDevSceneInteractionUI();
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
                Debug.LogWarning($"[DLOInteractionSetupTools] Scene not found at {scenePath}. Building in the currently open scene instead.");
                return;
            }

            EditorSceneManager.OpenScene(scenePath);
        }

        private static void ClearPreviousCanvas()
        {
            GameObject previous = GameObject.Find(CanvasName);
            if (previous != null)
            {
                Object.DestroyImmediate(previous);
            }
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new(CanvasName);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 75;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreatePrompt(Transform parent)
        {
            GameObject root = CreatePanel(parent, "InteractionPrompt", Anchor.Bottom, new Vector2(0f, 146f), new Vector2(360f, 58f), new Color(0.04f, 0.04f, 0.045f, 0.72f));
            root.AddComponent<CanvasGroup>().blocksRaycasts = false;

            GameObject key = CreatePanel(root.transform, "Key", Anchor.Left, new Vector2(32f, 0f), new Vector2(48f, 38f), new Color(1f, 1f, 1f, 0.9f));
            TextMeshProUGUI keyLabel = CreateText(key.transform, "KeyLabel", "F", 19, TextAlignmentOptions.Center, new Color(0.08f, 0.08f, 0.08f, 1f));
            Stretch(keyLabel.rectTransform, new Vector2(4f, 3f), new Vector2(-4f, -3f));

            TextMeshProUGUI prompt = CreateText(root.transform, "PromptText", "F  조사", 20, TextAlignmentOptions.Left, Color.white);
            Stretch(prompt.rectTransform, new Vector2(88f, 5f), new Vector2(-18f, -5f));

            return root;
        }

        private static GameObject CreatePanel(Transform parent, string name, Anchor anchor, Vector2 position, Vector2 size, Color color)
        {
            GameObject obj = new(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            SetRect(rect, anchor, position, size);

            Image image = obj.AddComponent<Image>();
            image.color = color;
            return obj;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment, Color color)
        {
            GameObject obj = new(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            SetRect(rect, Anchor.Center, Vector2.zero, new Vector2(220f, 40f));

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

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void SetRect(RectTransform rect, Anchor anchor, Vector2 position, Vector2 size)
        {
            switch (anchor)
            {
                case Anchor.Left:
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
                    rect.pivot = new Vector2(0f, 0.5f);
                    break;
                case Anchor.Bottom:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);
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
            Left,
            Bottom
        }
    }
}
