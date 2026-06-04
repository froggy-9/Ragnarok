using DeadLetterOffice.Core;
using DeadLetterOffice.Dialogue;
using DeadLetterOffice.Interaction;
using DeadLetterOffice.Letter;
using DeadLetterOffice.NPC;
using DeadLetterOffice.Player;
using DeadLetterOffice.Scene;
using DeadLetterOffice.State;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DeadLetterOffice.Editor
{
    public static class DLOPrefabTools
    {
        private const string PrefabRoot = "Assets/Prefabs/DLO";
        private const string GameStatePath = "Assets/ScriptableObjectes/GameState/GameState.asset";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";

        [MenuItem("DLO/Setup/Create Placement Prefabs")]
        public static void CreatePlacementPrefabs()
        {
            EnsureFolder("Assets/Prefabs");
            EnsureFolder(PrefabRoot);

            AudioSource sfxSourcePrefab = CreateSfxSourcePrefab();
            CreatePersistentSystemsPrefab(sfxSourcePrefab);
            CreatePlayerPrefab();
            CreateCameraPrefab();
            CreateLetterPickupPrefab();
            CreateNpcPrefab();
            CreateInteractablePrefab();
            CreateStoryCameraTriggerPrefab();
            CreateMapAreaTriggerPrefab();
            CreateMinimalUiPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DLOPrefabTools] Placement prefabs created under Assets/Prefabs/DLO.");
        }

        private static AudioSource CreateSfxSourcePrefab()
        {
            GameObject root = new("PF_SfxSource");
            AudioSource source = root.AddComponent<AudioSource>();
            source.playOnAwake = false;

            SavePrefab(root, $"{PrefabRoot}/PF_SfxSource.prefab");
            return AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/PF_SfxSource.prefab").GetComponent<AudioSource>();
        }

        private static void CreatePersistentSystemsPrefab(AudioSource sfxSourcePrefab)
        {
            GameObject root = new("PF_PersistentSystems");
            root.AddComponent<GameModeManager>();
            root.AddComponent<SceneTransitionManager>();
            root.AddComponent<PersistentSystemsBootstrap>();

            SaveManager saveManager = root.AddComponent<SaveManager>();
            GameStateSO gameState = AssetDatabase.LoadAssetAtPath<GameStateSO>(GameStatePath);
            Set(saveManager, "_gameState", gameState);

            AudioManager audioManager = root.AddComponent<AudioManager>();
            AudioSource bgm = CreateAudioSourceChild(root.transform, "BGM_Source");
            AudioSource ambient = CreateAudioSourceChild(root.transform, "Ambient_Source");
            AudioSource voice = CreateAudioSourceChild(root.transform, "Voice_Source");
            Set(audioManager, "_bgmSource", bgm);
            Set(audioManager, "_ambientSource", ambient);
            Set(audioManager, "_voiceSource", voice);
            Set(audioManager, "_sfxSourcePrefab", sfxSourcePrefab);

            SavePrefab(root, $"{PrefabRoot}/PF_PersistentSystems.prefab");
        }

        private static void CreatePlayerPrefab()
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "PF_Player";
            root.tag = "Player";
            root.transform.position = Vector3.zero;

            CharacterController controller = root.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 1f, 0f);
            controller.height = 2f;
            controller.radius = 0.35f;

            Object.DestroyImmediate(root.GetComponent<CapsuleCollider>());

            root.AddComponent<PlayerController>();
            root.AddComponent<PlayerAnimationController>();

            PlayerInput input = root.AddComponent<PlayerInput>();
            input.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            input.defaultActionMap = "Player";
            input.notificationBehavior = PlayerNotifications.SendMessages;

            root.AddComponent<InteractionInputController>();
            root.AddComponent<DialogueInputController>();

            SavePrefab(root, $"{PrefabRoot}/PF_Player.prefab");
        }

        private static void CreateCameraPrefab()
        {
            GameObject root = new("PF_PlayerCamera");
            Camera camera = root.AddComponent<Camera>();
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 300f;

            root.AddComponent<AudioListener>();
            root.AddComponent<PlayerCameraController>();

            SavePrefab(root, $"{PrefabRoot}/PF_PlayerCamera.prefab");
        }

        private static void CreateLetterPickupPrefab()
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "PF_LetterPickup";
            root.transform.localScale = new Vector3(0.5f, 0.03f, 0.35f);
            root.AddComponent<LetterPickup>();
            SavePrefab(root, $"{PrefabRoot}/PF_LetterPickup.prefab");
        }

        private static void CreateNpcPrefab()
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "PF_NPC";
            root.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            root.AddComponent<NPCController>();
            SavePrefab(root, $"{PrefabRoot}/PF_NPC.prefab");
        }

        private static void CreateInteractablePrefab()
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "PF_InteractableObject";
            root.transform.localScale = Vector3.one * 0.7f;
            root.AddComponent<InteractableObject>();
            SavePrefab(root, $"{PrefabRoot}/PF_InteractableObject.prefab");
        }

        private static void CreateStoryCameraTriggerPrefab()
        {
            GameObject root = new("PF_StoryCameraTrigger");
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(4f, 2f, 4f);

            GameObject shot = new("ShotTransform");
            shot.transform.SetParent(root.transform);
            shot.transform.localPosition = new Vector3(0f, 2f, -5f);
            shot.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);

            StoryCameraTrigger trigger = root.AddComponent<StoryCameraTrigger>();
            Set(trigger, "_shotTransform", shot.transform);

            SavePrefab(root, $"{PrefabRoot}/PF_StoryCameraTrigger.prefab");
        }

        private static void CreateMapAreaTriggerPrefab()
        {
            GameObject root = new("PF_MapAreaTrigger");
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(6f, 2f, 6f);
            root.AddComponent<MapAreaTrigger>();
            SavePrefab(root, $"{PrefabRoot}/PF_MapAreaTrigger.prefab");
        }

        private static void CreateMinimalUiPrefab()
        {
            GameObject root = new("PF_MinimalUI");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();

            GameObject eventSystem = new("EventSystem");
            eventSystem.transform.SetParent(root.transform);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            GameObject narrationPanel = CreatePanel(root.transform, "NarrationPanel", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 72f), new Vector2(900f, 90f));
            TextMeshProUGUI narrationText = CreateText(narrationPanel.transform, "NarrationText", "조사 텍스트", 26);
            NarrationUI narrationUI = root.AddComponent<NarrationUI>();
            Set(narrationUI, "_root", narrationPanel);
            Set(narrationUI, "_text", narrationText);

            GameObject dialoguePanel = CreatePanel(root.transform, "DialoguePanel", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(1000f, 210f));
            TextMeshProUGUI nameText = CreateText(dialoguePanel.transform, "NameText", "Speaker", 24);
            nameText.rectTransform.anchoredPosition = new Vector2(-390f, 65f);
            nameText.rectTransform.sizeDelta = new Vector2(180f, 40f);
            TextMeshProUGUI dialogueText = CreateText(dialoguePanel.transform, "DialogueText", "Dialogue", 26);
            dialogueText.rectTransform.anchoredPosition = new Vector2(70f, 10f);
            dialogueText.rectTransform.sizeDelta = new Vector2(760f, 120f);
            GameObject choiceContainer = new("ChoiceContainer");
            choiceContainer.transform.SetParent(dialoguePanel.transform);
            RectTransform choiceRect = choiceContainer.AddComponent<RectTransform>();
            choiceRect.anchorMin = new Vector2(1f, 0.5f);
            choiceRect.anchorMax = new Vector2(1f, 0.5f);
            choiceRect.pivot = new Vector2(1f, 0.5f);
            choiceRect.anchoredPosition = new Vector2(-32f, 0f);
            choiceRect.sizeDelta = new Vector2(240f, 160f);
            VerticalLayoutGroup layout = choiceContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;

            Button choiceButton = CreateButton(choiceContainer.transform, "ChoiceButtonTemplate", "선택지");
            choiceButton.gameObject.SetActive(false);

            DialogueUI dialogueUI = root.AddComponent<DialogueUI>();
            Set(dialogueUI, "_root", dialoguePanel);
            Set(dialogueUI, "_nameText", nameText);
            Set(dialogueUI, "_dialogueText", dialogueText);
            Set(dialogueUI, "_choiceContainer", choiceContainer.transform);
            Set(dialogueUI, "_choiceButtonPrefab", choiceButton);

            DialogueManager dialogueManager = root.AddComponent<DialogueManager>();
            Set(dialogueManager, "_dialogueUI", dialogueUI);

            SavePrefab(root, $"{PrefabRoot}/PF_MinimalUI.prefab");
        }

        private static AudioSource CreateAudioSourceChild(Transform parent, string name)
        {
            GameObject child = new(name);
            child.transform.SetParent(parent);
            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            GameObject panel = new(name);
            panel.transform.SetParent(parent);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.05f, 0.05f, 0.05f, 0.78f);
            return panel;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize)
        {
            GameObject obj = new(name);
            obj.transform.SetParent(parent);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(800f, 70f);

            TextMeshProUGUI label = obj.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Left;
            return label;
        }

        private static Button CreateButton(Transform parent, string name, string text)
        {
            GameObject obj = new(name);
            obj.transform.SetParent(parent);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(220f, 42f);

            Image image = obj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            Button button = obj.AddComponent<Button>();

            TextMeshProUGUI label = CreateText(obj.transform, "Label", text, 20);
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.sizeDelta = rect.sizeDelta;
            return button;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
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

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}
