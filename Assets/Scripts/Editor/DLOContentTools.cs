using System.Collections.Generic;
using System.IO;
using DeadLetterOffice.Archive;
using DeadLetterOffice.Board;
using DeadLetterOffice.Chapter;
using DeadLetterOffice.Dialogue;
using DeadLetterOffice.Letter;
using DeadLetterOffice.NPC;
using DeadLetterOffice.State;
using UnityEditor;
using UnityEngine;

namespace DeadLetterOffice.Editor
{
    public static class DLOContentTools
    {
        private const string Root = "Assets/ScriptableObjectes";

        [MenuItem("DLO/Content/Create Chapter 1 Starter Content")]
        public static void CreateChapter1StarterContent()
        {
            EnsureFolders();

            Dictionary<string, FlagSO> flags = CreateFlags();
            CreateGameState(flags);

            Dictionary<string, CharacterFileSO> characters = CreateCharacterFiles();
            CreateCharacterEntries(characters, flags);

            Dictionary<string, LetterSO> letters = CreateLetters(flags);
            CreateCollectibles();
            Dictionary<string, BoardCardSO> cards = CreateBoardCards(flags);
            CreateBoardConnections(cards, flags);
            CreateDialogues(flags);
            CreateNpcData();
            CreateChapter(flags);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[DLOContentTools] Chapter 1 starter content created. Letters: {letters.Count}, Flags: {flags.Count}");
        }

        [MenuItem("DLO/Content/Validate Content")]
        public static void ValidateContent()
        {
            int issues = 0;

            issues += ValidateAssets<LetterSO>("Letter", letter =>
            {
                if (letter.Segments == null || letter.Segments.Length == 0)
                {
                    return "has no segments";
                }

                return null;
            });

            issues += ValidateAssets<DialogueSO>("Dialogue", dialogue =>
            {
                if (dialogue.Lines == null || dialogue.Lines.Length == 0)
                {
                    return "has no lines";
                }

                return null;
            });

            issues += ValidateAssets<NPCDataSO>("NPC", npc =>
            {
                if (npc.GetBestDialogue() == null)
                {
                    return "has no available default dialogue. This may be fine if all dialogues are conditional.";
                }

                return null;
            });

            issues += ValidateAssets<BoardConnectionSO>("Board Connection", connection =>
            {
                if (connection.FromCard == null || connection.ToCard == null)
                {
                    return "has missing cards";
                }

                if (connection.CompletionFlag == null)
                {
                    return "has no completion flag";
                }

                return null;
            });

            if (issues == 0)
            {
                Debug.Log("[DLOContentTools] Content validation passed.");
            }
            else
            {
                Debug.LogWarning($"[DLOContentTools] Content validation finished with {issues} issue(s).");
            }
        }

        private static Dictionary<string, FlagSO> CreateFlags()
        {
            Dictionary<string, string> definitions = new()
            {
                ["letter01Found"] = "편지 #1 발견 (작업대)",
                ["letter02Found"] = "편지 #2 발견 (거주지)",
                ["letter03Found"] = "편지 #3 발견 (우산 주인)",
                ["letter04Found"] = "편지 #4 발견 (창고)",
                ["letter05Found"] = "편지 #5 발견 (창고)",
                ["letter06Found"] = "편지 #6 발견 (창고 깊숙)",
                ["letter07Found"] = "편지 #7 발견 (다락방)",
                ["letter08Found"] = "편지 #8 발견 (관리자 서랍)",
                ["boardCrowSparrow"] = "까마귀-참새 연결 완성",
                ["boardSparrowManager"] = "참새-관리자 연결 완성",
                ["boardMorelNetwork"] = "Morel-네트워크 연결",
                ["boardPlayerLetter7"] = "편지 #7-플레이어 연결",
                ["npcJanitorTalked"] = "청소부 첫 대화",
                ["npcUmbrellaTalked"] = "우산 주인 첫 대화",
                ["umbrellaShown"] = "우산을 우산 주인에게 보여줌",
                ["beat04LucaMet"] = "루카 첫 만남 완료",
                ["beat05Triggered"] = "편지 #7 발견 후 클라이맥스 트리거",
                ["chapter01End"] = "챕터 1 종료"
            };

            Dictionary<string, FlagSO> flags = new();
            foreach (KeyValuePair<string, string> definition in definitions)
            {
                FlagSO flag = LoadOrCreate<FlagSO>($"{Root}/Flags/flag_{ToSnake(definition.Key)}.asset");
                Set(flag, "_description", definition.Value);
                flags[definition.Key] = flag;
            }

            return flags;
        }

        private static void CreateGameState(Dictionary<string, FlagSO> flags)
        {
            GameStateSO gameState = LoadOrCreate<GameStateSO>($"{Root}/GameState/GameState.asset");

            Set(gameState, "_letter01Found", flags["letter01Found"]);
            Set(gameState, "_letter02Found", flags["letter02Found"]);
            Set(gameState, "_letter03Found", flags["letter03Found"]);
            Set(gameState, "_letter04Found", flags["letter04Found"]);
            Set(gameState, "_letter05Found", flags["letter05Found"]);
            Set(gameState, "_letter06Found", flags["letter06Found"]);
            Set(gameState, "_letter07Found", flags["letter07Found"]);
            Set(gameState, "_letter08Found", flags["letter08Found"]);
            Set(gameState, "_boardCrowSparrow", flags["boardCrowSparrow"]);
            Set(gameState, "_boardSparrowManager", flags["boardSparrowManager"]);
            Set(gameState, "_boardMorelNetwork", flags["boardMorelNetwork"]);
            Set(gameState, "_boardPlayerLetter7", flags["boardPlayerLetter7"]);
            Set(gameState, "_npcJanitorTalked", flags["npcJanitorTalked"]);
            Set(gameState, "_npcUmbrellaTalked", flags["npcUmbrellaTalked"]);
            Set(gameState, "_umbrellaShown", flags["umbrellaShown"]);
            Set(gameState, "_beat04LucaMet", flags["beat04LucaMet"]);
            Set(gameState, "_beat05Triggered", flags["beat05Triggered"]);
            Set(gameState, "_chapter01End", flags["chapter01End"]);
        }

        private static Dictionary<string, CharacterFileSO> CreateCharacterFiles()
        {
            CharacterSeed[] seeds =
            {
                new("player", "Elisa Vayne", "E. Vayne", "E", "주인공. 야간 우체부이자 과거 내부고발자."),
                new("manager", "Manager", "-", "-", "우체국 야간 관리자. 친절함 뒤에 죄책감과 목적이 있다."),
                new("luca", "Luca", "L. Arden", "L", "플레이어의 소꿉친구. 정체를 숨긴 채 플레이어를 살핀다."),
                new("morel", "Vesper Morel", "V. Morel", "V", "과거 네트워크의 흔적을 따라 드러나는 인물."),
                new("ashwood", "Phelan Ashwood", "P. Ashwood", "P", "네트워크 동료."),
                new("aeris", "Kael Aeris", "K. Aeris", "K", "네트워크 동료.")
            };

            Dictionary<string, CharacterFileSO> characters = new();
            foreach (CharacterSeed seed in seeds)
            {
                CharacterFileSO file = LoadOrCreate<CharacterFileSO>($"{Root}/Archive/char_{seed.Id}.asset");
                Set(file, "_displayName", seed.DisplayName);
                Set(file, "_shortName", seed.ShortName);
                Set(file, "_codeName", seed.CodeName);
                Set(file, "_description", seed.Description);
                characters[seed.Id] = file;
            }

            return characters;
        }

        private static void CreateCharacterEntries(Dictionary<string, CharacterFileSO> characters, Dictionary<string, FlagSO> flags)
        {
            CreateCharacterEntry("morel_01", characters["morel"], "V. Morel이라는 이름이 반복해서 등장한다. 아직 생존 여부는 알 수 없다.", flags["letter01Found"]);
            CreateCharacterEntry("player_01", characters["player"], "편지 #7은 플레이어가 사건의 외부인이 아니라는 첫 단서다.", flags["letter07Found"]);
            CreateCharacterEntry("luca_01", characters["luca"], "루카는 플레이어를 너무 잘 아는 사람처럼 행동한다.", flags["beat04LucaMet"]);
        }

        private static Dictionary<string, LetterSO> CreateLetters(Dictionary<string, FlagSO> flags)
        {
            LetterSeed[] seeds =
            {
                new("01", "V. Morel", "P. Ashwood", "3년 전, 11월", "우체국 내 관리자의 작업대", "오랜만이야.\n잘 지내고 있지?\n지난번에 맡긴 것들은 내가 잘 챙겨뒀어.\n걱정하지 않아도 돼.\n\n아, 그리고 - 아래층 창고 있잖아.\n당분간은 그쪽으로 가지 않는 게 좋을 것 같아.\n이유는 나중에 설명할게.\n\n다음에 또 연락할게.\n건강해.\n\n- P"),
                new("02", "V. Morel", "████████", "4년 전, 3월", "#1 수신인의 거주지 우편함 깊숙한 곳", "그날 이후로 잘 지내고 계신지요.\n한 가지만 부탁드려도 될까요.\n그 계절 얘기는 - 이제 꺼내지 않으셨으면 해요.\n듣는 귀는 언제나 생각보다 많으니까요.\n부디 평온한 나날 되시길."),
                new("03", "", "V. Morel", "2년 전, 6월", "우산가게의 주인", "V,\n지난 겨울에 빌려주신 우산,\n이제야 돌려드리게 됐네요.\n비가 오는 날이면 괜히 생각이 나서요.\n그때 처마 밑에서 둘이 서 있던 것.\n아무것도 아닌 날이었는데\n이상하게 오래 기억에 남아요.\n잘 지내고 계시죠?"),
                new("04", "L. Marel", "K. Aeris", "5년 전, 9월", "우체국 창고", "이번 주 목요일,\n서쪽 기록실 근처에 잠깐 들를 일이 있어.\n세 번째 서랍에\n작년에 맡겨뒀던 서류들이 있거든.\n슬슬 찾아가야 할 것 같아서.\n아, 비 오는 날은 피해줘.\n그쪽 길이 많이 질퍽거려서.\n\nK. Aeris"),
                new("05", "L. Marel", "K. Aeris", "5년 전, 12월", "우체국 창고", "Lucien Marel,\n이번 모임은 없던 걸로 하자.\n갑자기 일이 좀 생겼어.\n별거 아닌데 - 그냥 당분간은\n각자 조용히 지내는 게 나을 것 같아.\n연락은 내가 먼저 할게.\n기다려줘.\n\n- K"),
                new("06", "████████", "████████", "5년 전, 12월", "우체국 창고 비밀의 공간", "보내지 못할 것 같아서\n그냥 여기다 써.\n잘 지내.\n부탁이야.\n봄이 오면 - 그 골목 있잖아,\n우리 셋이서 자주 앉았던 데.\n한 번만 가줘.\n거기서 생각해줘도 충분해."),
                new("07", "E. Vayne", "--", "5년 전", "우체국 다락방", "오랫동안 여기 있었어.\n네가 올 것 같아서.\n별로 할 말은 없고 - 그냥 한 가지만.\n이 도시는 잊어버리게 만들어.\n근데 있잖아.\n네 손은 아직 기억할 거야.\n뭘 쥐고 있었는지."),
                new("08", "L. Arden", "E. Vayne", "-- (발송되지 않음)", "관리자의 서랍 안", "루카,\n네가 맞았어.\n근데 있잖아 - 틀렸어도 했을 것 같아.\n에밀이 없어진 날,\n아무도 그 이름을 부르지 않더라고.\n다음 날도. 그 다음 날도.\n그게 제일 무서웠어.\n사라지는 것보다\n불리지 않는 게.\n미안해.\n그리고 고마워.\n\n- [플레이어 이름]")
            };

            Dictionary<string, LetterSO> letters = new();
            foreach (LetterSeed seed in seeds)
            {
                LetterSegmentSO segment = LoadOrCreate<LetterSegmentSO>($"{Root}/Letters/seg_letter_{seed.Number}_body.asset");
                Set(segment, "_text", seed.Body);
                Set(segment, "_redactedText", seed.Body);

                LetterSO letter = LoadOrCreate<LetterSO>($"{Root}/Letters/letter_{seed.Number}.asset");
                Set(letter, "_letterNumber", $"#{seed.Number}");
                Set(letter, "_recipient", seed.Recipient);
                Set(letter, "_sender", seed.Sender);
                Set(letter, "_postmark", seed.Postmark);
                Set(letter, "_discoveryLocation", seed.DiscoveryLocation);
                SetArray(letter, "_segments", new Object[] { segment });
                letters[seed.Number] = letter;
            }

            Set(AssetDatabase.LoadAssetAtPath<LetterSegmentSO>($"{Root}/Letters/seg_letter_05_body.asset"), "_revealFlag", flags["boardCrowSparrow"]);
            Set(AssetDatabase.LoadAssetAtPath<LetterSegmentSO>($"{Root}/Letters/seg_letter_07_body.asset"), "_revealFlag", flags["boardPlayerLetter7"]);

            return letters;
        }

        private static void CreateCollectibles()
        {
            CollectibleSO umbrella = LoadOrCreate<CollectibleSO>($"{Root}/Collectibles/item_umbrella.asset");
            Set(umbrella, "_displayName", "낡은 우산");
            Set(umbrella, "_description", "V. Morel의 흔적과 연결된 우산. 우산 주인에게 보여줄 수 있다.");
        }

        private static Dictionary<string, BoardCardSO> CreateBoardCards(Dictionary<string, FlagSO> flags)
        {
            BoardCardSeed[] seeds =
            {
                new("letter_01", "편지 #1", BoardCardType.Letter, "V. Morel의 이름이 처음 등장한다.", flags["letter01Found"]),
                new("letter_04", "편지 #4", BoardCardType.Letter, "네트워크의 존재를 암시한다.", flags["letter04Found"]),
                new("letter_07", "편지 #7", BoardCardType.Letter, "플레이어와 사건의 연결.", flags["letter07Found"]),
                new("morel", "V. Morel", BoardCardType.Character, "편지들이 가리키는 이름.", flags["letter01Found"]),
                new("network", "편지 네트워크", BoardCardType.Event, "누군가 편지로 증거를 모았다.", flags["letter04Found"]),
                new("player", "E. Vayne", BoardCardType.Character, "플레이어의 과거 이름.", flags["letter07Found"]),
                new("manager", "관리자", BoardCardType.Character, "무언가를 알고 있는 우체국 관리자.", flags["letter01Found"])
            };

            Dictionary<string, BoardCardSO> cards = new();
            foreach (BoardCardSeed seed in seeds)
            {
                BoardCardSO card = LoadOrCreate<BoardCardSO>($"{Root}/Board/card_{seed.Id}.asset");
                Set(card, "_displayName", seed.DisplayName);
                Set(card, "_cardType", seed.Type);
                Set(card, "_summary", seed.Summary);
                SetArray(card, "_requiredFlags", new Object[] { seed.RequiredFlag });
                cards[seed.Id] = card;
            }

            return cards;
        }

        private static void CreateBoardConnections(Dictionary<string, BoardCardSO> cards, Dictionary<string, FlagSO> flags)
        {
            CreateConnection("crow_sparrow", cards["letter_04"], cards["network"], flags["boardCrowSparrow"], "편지 속 암호명이 네트워크와 이어진다.");
            CreateConnection("sparrow_manager", cards["network"], cards["manager"], flags["boardSparrowManager"], "관리자는 네트워크와 무관하지 않다.");
            CreateConnection("morel_network", cards["morel"], cards["network"], flags["boardMorelNetwork"], "Morel은 네트워크의 핵심 인물 중 하나다.");
            CreateConnection("player_letter7", cards["player"], cards["letter_07"], flags["boardPlayerLetter7"], "플레이어 본인이 사건과 직접 연결된다.");
        }

        private static void CreateDialogues(Dictionary<string, FlagSO> flags)
        {
            DialogueLineSO managerLine = CreateLine("manager_default_line_01", "관리자", "오늘은 우편물 정리만 하면 돼요. 아래층 창고는 아직 정리가 덜 됐으니 가지 않는 게 좋겠습니다.");
            DialogueSO managerDialogue = LoadOrCreate<DialogueSO>($"{Root}/Dailogues/dlg_manager_default.asset");
            SetArray(managerDialogue, "_lines", new Object[] { managerLine });

            DialogueLineSO janitorLine = CreateLine("janitor_default_line_01", "청소부", "밤에는 사람들이 말을 아끼는 법이지. 그래도 우체국은 늘 뭔가를 기억하고 있어.");
            DialogueSO janitorDialogue = LoadOrCreate<DialogueSO>($"{Root}/Dailogues/dlg_janitor_default.asset");
            SetArray(janitorDialogue, "_lines", new Object[] { janitorLine });
            Set(janitorDialogue, "_setFlagOnComplete", flags["npcJanitorTalked"]);

            DialogueLineSO lucaLine = CreateLine("luca_first_line_01", "낯선 남자", "너는 여전히 같은 선택을 할까? 아니, 미안. 처음 보는 사람에게 할 말은 아니네.");
            DialogueSO lucaDialogue = LoadOrCreate<DialogueSO>($"{Root}/Dailogues/dlg_luca_first.asset");
            SetArray(lucaDialogue, "_requiredFlags", new Object[] { flags["letter02Found"] });
            SetArray(lucaDialogue, "_lines", new Object[] { lucaLine });
            Set(lucaDialogue, "_setFlagOnComplete", flags["beat04LucaMet"]);
        }

        private static void CreateNpcData()
        {
            NPCDataSO manager = LoadOrCreate<NPCDataSO>($"{Root}/NPCs/npc_manager.asset");
            Set(manager, "_displayName", "관리자");
            SetDialogueEntries(manager, new[] { AssetDatabase.LoadAssetAtPath<DialogueSO>($"{Root}/Dailogues/dlg_manager_default.asset") });

            NPCDataSO janitor = LoadOrCreate<NPCDataSO>($"{Root}/NPCs/npc_janitor.asset");
            Set(janitor, "_displayName", "청소부");
            SetDialogueEntries(janitor, new[] { AssetDatabase.LoadAssetAtPath<DialogueSO>($"{Root}/Dailogues/dlg_janitor_default.asset") });

            NPCDataSO luca = LoadOrCreate<NPCDataSO>($"{Root}/NPCs/npc_luca.asset");
            Set(luca, "_displayName", "루카");
            SetDialogueEntries(luca, new[] { AssetDatabase.LoadAssetAtPath<DialogueSO>($"{Root}/Dailogues/dlg_luca_first.asset") });
        }

        private static void CreateChapter(Dictionary<string, FlagSO> flags)
        {
            BeatSO beat01 = CreateBeat("beat_01_first_shift", "01 첫 출근", "관리자가 업무를 설명한다.", null);
            BeatSO beat02 = CreateBeat("beat_02_letter_found", "02 편지 발견", "작업대에서 첫 편지를 발견한다.", flags["letter01Found"]);
            BeatSO beat04 = CreateBeat("beat_04_luca_met", "04 루카 첫 만남", "루카가 정체를 숨긴 채 접근한다.", flags["beat04LucaMet"]);
            BeatSO beat05 = CreateBeat("beat_05_attic_letter", "05 다락방 편지", "플레이어 본인 앞으로 온 오래된 편지를 발견한다.", flags["letter07Found"]);

            ChapterSO chapter = LoadOrCreate<ChapterSO>($"{Root}/Chapters/chapter_01.asset");
            Set(chapter, "_chapterNumber", 1);
            Set(chapter, "_displayName", "Chapter 1 - Dead Letter Office");
            SetArray(chapter, "_beats", new Object[] { beat01, beat02, beat04, beat05 });
        }

        private static void CreateConnection(string id, BoardCardSO from, BoardCardSO to, FlagSO flag, string memo)
        {
            BoardConnectionSO connection = LoadOrCreate<BoardConnectionSO>($"{Root}/Board/conn_{id}.asset");
            Set(connection, "_fromCard", from);
            Set(connection, "_toCard", to);
            Set(connection, "_completionFlag", flag);
            Set(connection, "_successMemo", memo);
        }

        private static DialogueLineSO CreateLine(string id, string speaker, string text)
        {
            DialogueLineSO line = LoadOrCreate<DialogueLineSO>($"{Root}/Dailogues/line_{id}.asset");
            Set(line, "_speaker", speaker);
            Set(line, "_text", text);
            return line;
        }

        private static void CreateCharacterEntry(string id, CharacterFileSO characterFile, string entryText, FlagSO requiredFlag)
        {
            CharacterEntrySO entry = LoadOrCreate<CharacterEntrySO>($"{Root}/Archive/entry_{id}.asset");
            Set(entry, "_characterFile", characterFile);
            Set(entry, "_entryText", entryText);
            SetArray(entry, "_requiredFlags", new Object[] { requiredFlag });
        }

        private static BeatSO CreateBeat(string id, string displayName, string description, FlagSO requiredFlag)
        {
            BeatSO beat = LoadOrCreate<BeatSO>($"{Root}/Chapters/{id}.asset");
            Set(beat, "_displayName", displayName);
            Set(beat, "_description", description);
            SetArray(beat, "_requiredFlags", requiredFlag == null ? new Object[0] : new Object[] { requiredFlag });
            return beat;
        }

        private static void SetDialogueEntries(NPCDataSO npc, DialogueSO[] dialogues)
        {
            SerializedObject serializedObject = new(npc);
            SerializedProperty entries = serializedObject.FindProperty("_dialogues");
            entries.arraySize = dialogues.Length;

            for (int i = 0; i < dialogues.Length; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("_dialogue").objectReferenceValue = dialogues[i];
                entry.FindPropertyRelative("_requiredFlags").arraySize = 0;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(npc);
        }

        private static int ValidateAssets<T>(string label, System.Func<T, string> validate) where T : Object
        {
            int issues = 0;
            foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { Root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                string message = validate(asset);
                if (!string.IsNullOrEmpty(message))
                {
                    Debug.LogWarning($"[DLOContentTools] {label} '{asset.name}' {message}. Path: {path}", asset);
                    issues++;
                }
            }

            return issues;
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

        private static void Set(Object target, string propertyName, object value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"[DLOContentTools] Missing property {propertyName} on {target.name}", target);
                return;
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    property.stringValue = value as string ?? string.Empty;
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = value is int intValue ? intValue : 0;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = value is bool boolValue && boolValue;
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = (int)value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = value as Object;
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private static void SetArray(Object target, string propertyName, Object[] values)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"[DLOContentTools] Missing array property {propertyName} on {target.name}", target);
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

        private static void EnsureFolders()
        {
            EnsureFolder(Root);
            EnsureFolder($"{Root}/Archive");
            EnsureFolder($"{Root}/Board");
            EnsureFolder($"{Root}/Chapters");
            EnsureFolder($"{Root}/Collectibles");
            EnsureFolder($"{Root}/Dailogues");
            EnsureFolder($"{Root}/Flags");
            EnsureFolder($"{Root}/GameState");
            EnsureFolder($"{Root}/Letters");
            EnsureFolder($"{Root}/NPCs");
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

        private static string ToSnake(string value)
        {
            List<char> result = new();
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (char.IsUpper(c) && i > 0)
                {
                    result.Add('_');
                }

                result.Add(char.ToLowerInvariant(c));
            }

            return new string(result.ToArray());
        }

        private readonly struct CharacterSeed
        {
            public readonly string Id;
            public readonly string DisplayName;
            public readonly string ShortName;
            public readonly string CodeName;
            public readonly string Description;

            public CharacterSeed(string id, string displayName, string shortName, string codeName, string description)
            {
                Id = id;
                DisplayName = displayName;
                ShortName = shortName;
                CodeName = codeName;
                Description = description;
            }
        }

        private readonly struct LetterSeed
        {
            public readonly string Number;
            public readonly string Recipient;
            public readonly string Sender;
            public readonly string Postmark;
            public readonly string DiscoveryLocation;
            public readonly string Body;

            public LetterSeed(string number, string recipient, string sender, string postmark, string discoveryLocation, string body)
            {
                Number = number;
                Recipient = recipient;
                Sender = sender;
                Postmark = postmark;
                DiscoveryLocation = discoveryLocation;
                Body = body;
            }
        }

        private readonly struct BoardCardSeed
        {
            public readonly string Id;
            public readonly string DisplayName;
            public readonly BoardCardType Type;
            public readonly string Summary;
            public readonly FlagSO RequiredFlag;

            public BoardCardSeed(string id, string displayName, BoardCardType type, string summary, FlagSO requiredFlag)
            {
                Id = id;
                DisplayName = displayName;
                Type = type;
                Summary = summary;
                RequiredFlag = requiredFlag;
            }
        }
    }
}
