using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeadLetterOffice.UI
{
    public class QuestLogUI : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Transform _questList;
        [SerializeField] private Button _questButtonTemplate;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _areaText;
        [SerializeField] private TMP_Text _objectiveText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private GameObject _rewardRoot;
        [SerializeField] private Transform _rewardList;
        [SerializeField] private GameObject _rewardItemTemplate;
        [SerializeField] private StoryQuestListSO _questListAsset;
        [SerializeField] private StoryQuestSO[] _quests;

        private Button _selectedButton;

        private void Awake()
        {
            UseQuestListAssetIfAssigned();
            BuildList();
            SelectFirstQuest();
        }

        public void Show()
        {
            if (_root != null)
            {
                _root.SetActive(true);
            }
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        public void SetQuests(StoryQuestSO[] quests)
        {
            _questListAsset = null;
            _quests = quests;
            BuildList();
            SelectFirstQuest();
        }

        public void SetQuestList(StoryQuestListSO questList)
        {
            _questListAsset = questList;
            UseQuestListAssetIfAssigned();
            BuildList();
            SelectFirstQuest();
        }

        private void SelectFirstQuest()
        {
            if (_quests == null)
            {
                return;
            }

            foreach (StoryQuestSO quest in _quests)
            {
                if (quest != null)
                {
                    SelectQuest(quest);
                    return;
                }
            }
        }

        private void BuildList()
        {
            if (_questList == null || _questButtonTemplate == null)
            {
                return;
            }

            ClearChildren(_questList, _questButtonTemplate.gameObject);
            _questButtonTemplate.gameObject.SetActive(false);
            _selectedButton = null;

            if (_quests == null)
            {
                return;
            }

            StoryQuestType? lastType = null;
            foreach (StoryQuestSO quest in _quests)
            {
                if (quest == null)
                {
                    continue;
                }

                if (lastType != quest.Type)
                {
                    CreateGroupHeader(quest.Type);
                    lastType = quest.Type;
                }

                Button button = Instantiate(_questButtonTemplate, _questList);
                button.gameObject.SetActive(true);
                button.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 92f);

                TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.alignment = TextAlignmentOptions.Left;
                    label.text = BuildQuestButtonLabel(quest);
                }

                button.onClick.AddListener(() => SelectQuest(quest, button));
            }
        }

        private void SelectQuest(StoryQuestSO quest, Button selectedButton = null)
        {
            if (quest == null)
            {
                return;
            }

            SetSelectedButton(selectedButton);
            SetText(_titleText, quest.Title);
            SetText(_areaText, string.IsNullOrWhiteSpace(quest.Area) ? "" : $"⌖ {quest.Area}");
            SetText(_objectiveText, $"▶ {quest.Objective}");
            SetText(_descriptionText, quest.Description);
            BuildRewards(quest.Rewards);
        }

        private void BuildRewards(StoryQuestReward[] rewards)
        {
            bool hasRewards = rewards != null && rewards.Length > 0;
            if (_rewardRoot != null)
            {
                _rewardRoot.SetActive(hasRewards);
            }

            if (_rewardList == null || _rewardItemTemplate == null)
            {
                return;
            }

            ClearChildren(_rewardList, _rewardItemTemplate);
            _rewardItemTemplate.SetActive(false);

            if (!hasRewards)
            {
                return;
            }

            foreach (StoryQuestReward reward in rewards)
            {
                GameObject item = Instantiate(_rewardItemTemplate, _rewardList);
                item.SetActive(true);

                TMP_Text label = item.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = reward.Amount > 1 ? $"x{reward.Amount}" : reward.ItemName;
                }

                Image[] images = item.GetComponentsInChildren<Image>(true);
                foreach (Image image in images)
                {
                    if (image.gameObject == item)
                    {
                        continue;
                    }

                    if (reward.Icon != null)
                    {
                        image.sprite = reward.Icon;
                        image.color = Color.white;
                    }

                    break;
                }
            }
        }

        private void CreateGroupHeader(StoryQuestType type)
        {
            GameObject header = new GameObject($"Header_{type}", typeof(RectTransform));
            header.transform.SetParent(_questList, false);
            RectTransform rect = header.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 34f);

            TMP_Text label = header.AddComponent<TextMeshProUGUI>();
            label.fontSize = 20f;
            label.alignment = TextAlignmentOptions.Left;
            label.color = new Color(0.95f, 0.9f, 0.76f, 1f);
            label.text = type switch
            {
                StoryQuestType.Main => "스토리 임무",
                StoryQuestType.Commission => "의뢰 임무",
                _ => "서브 임무"
            };
        }

        private void UseQuestListAssetIfAssigned()
        {
            if (_questListAsset != null)
            {
                _quests = _questListAsset.Quests;
            }
        }

        private static string BuildQuestButtonLabel(StoryQuestSO quest)
        {
            string marker = quest.Type == StoryQuestType.Main ? "◆" : "◇";
            string chapter = string.IsNullOrWhiteSpace(quest.ChapterName) ? "" : $"\n<size=80%>{quest.ChapterName}</size>";
            string distance = quest.DistanceMeters > 0 ? $"\n<size=75%>{quest.DistanceMeters}m</size>" : "";
            string completed = quest.Completed ? "  <size=75%>완료</size>" : "";
            return $"{marker} {quest.Title}{completed}{chapter}{distance}";
        }

        private void SetSelectedButton(Button selectedButton)
        {
            if (_selectedButton != null && _selectedButton.targetGraphic != null)
            {
                _selectedButton.targetGraphic.color = new Color(0.12f, 0.16f, 0.22f, 0.72f);
            }

            _selectedButton = selectedButton;

            if (_selectedButton != null && _selectedButton.targetGraphic != null)
            {
                _selectedButton.targetGraphic.color = new Color(0.95f, 0.9f, 0.76f, 0.22f);
            }
        }

        private static void SetText(TMP_Text target, string text)
        {
            if (target != null)
            {
                target.text = text;
            }
        }

        private static void ClearChildren(Transform parent, GameObject keep)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child.gameObject != keep)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}
