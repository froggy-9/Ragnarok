using System.IO;
using DeadLetterOffice.Dialogue;
using DeadLetterOffice.NPC;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DeadLetterOffice.Editor
{
    public static class DLODialogueTestSetupTools
    {
        private const string Root = "Assets/ScriptableObjectes/TestDialogue";

        [MenuItem("DLO/Setup/Create Test NPC Dialogue")]
        public static void CreateTestNpcDialogue()
        {
            EnsureFolder(Root);

            DialogueLineSO greetingLine = LoadOrCreate<DialogueLineSO>($"{Root}/line_test_npc_greeting.asset");
            Set(greetingLine, "_speaker", "테스트 NPC");
            Set(greetingLine, "_text", "안녕하세요.");

            DialogueLineSO answerLine = LoadOrCreate<DialogueLineSO>($"{Root}/line_test_npc_answer.asset");
            Set(answerLine, "_speaker", "테스트 NPC");
            Set(answerLine, "_text", "좋아요. 지금 대화 시스템은 정상으로 보입니다.");

            DialogueSO answerDialogue = LoadOrCreate<DialogueSO>($"{Root}/dlg_test_npc_answer.asset");
            SetArray(answerDialogue, "_lines", new Object[] { answerLine });
            SetArray(answerDialogue, "_choices", new Object[0]);

            DialogueChoiceSO choice = LoadOrCreate<DialogueChoiceSO>($"{Root}/choice_test_npc_hello.asset");
            Set(choice, "_choiceText", "안녕하세요.");
            Set(choice, "_nextDialogue", answerDialogue);

            DialogueSO greetingDialogue = LoadOrCreate<DialogueSO>($"{Root}/dlg_test_npc_greeting.asset");
            SetArray(greetingDialogue, "_lines", new Object[] { greetingLine });
            SetArray(greetingDialogue, "_choices", new Object[] { choice });

            NPCDataSO npcData = LoadOrCreate<NPCDataSO>($"{Root}/npc_test_dialogue.asset");
            Set(npcData, "_displayName", "테스트 NPC");
            SetDialogueEntries(npcData, greetingDialogue);

            CreateOrUpdateSceneNpc(npcData);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("[DLODialogueTestSetupTools] Test NPC dialogue assets and scene NPC created.");
        }

        private static void CreateOrUpdateSceneNpc(NPCDataSO npcData)
        {
            GameObject npc = GameObject.Find("DLO_TestDialogueNPC");
            if (npc == null)
            {
                npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                npc.name = "DLO_TestDialogueNPC";
                npc.transform.position = new Vector3(2f, 1f, 2f);
            }

            Collider collider = npc.GetComponent<Collider>();
            if (collider == null)
            {
                collider = npc.AddComponent<CapsuleCollider>();
            }

            collider.isTrigger = true;

            NPCController controller = npc.GetComponent<NPCController>();
            if (controller == null)
            {
                controller = npc.AddComponent<NPCController>();
            }

            Set(controller, "_npcData", npcData);
            Set(controller, "_promptText", "대화");
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            EnsureFolder(Path.GetDirectoryName(path)?.Replace("\\", "/"));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
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

        private static void SetArray(Object target, string propertyName, Object[] values)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
            {
                Debug.LogWarning($"[DLODialogueTestSetupTools] Missing array property {propertyName} on {target.name}.");
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

        private static void SetDialogueEntries(NPCDataSO npc, DialogueSO dialogue)
        {
            SerializedObject serializedObject = new(npc);
            SerializedProperty entries = serializedObject.FindProperty("_dialogues");
            entries.arraySize = 1;
            SerializedProperty entry = entries.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("_requiredFlags").arraySize = 0;
            entry.FindPropertyRelative("_dialogue").objectReferenceValue = dialogue;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(npc);
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = Path.GetFileName(path);

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
