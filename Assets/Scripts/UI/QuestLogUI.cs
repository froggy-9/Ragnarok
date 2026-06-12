using System.Collections.Generic;
using DeadLetterOffice.Core;
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
        [SerializeField] private Button _mainTabButton;
        [SerializeField] private Button _subTabButton;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _areaText;
        [SerializeField] private TMP_Text _objectiveText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private TMP_Text _lockReasonText;
        [SerializeField] private Button _actionButton;
        [SerializeField] private TMP_Text _actionButtonText;
        [SerializeField] private GameObject _rewardRoot;
        [SerializeField] private Transform _rewardList;
        [SerializeField] private GameObject _rewardItemTemplate;
        [SerializeField] private StoryQuestListSO _questListAsset;
        [SerializeField] private StoryQuestSO[] _quests;
        [SerializeField] private QuestSystem _questSystem;

        private readonly List<StoryQuestSO> _visibleQuests = new();
        private StoryQuestType _currentTab = StoryQuestType.Main;
        private Button _selectedButton;
        private StoryQuestSO _selectedQuest;

        private void Awake()
        {
            if (_questSystem == null)
            {
                _questSystem = FindFirstObjectByType<QuestSystem>();
            }

            if (_mainTabButton != null)
            {
                _mainTabButton.onClick.AddListener(() => SetCurrentTab(StoryQuestType.Main));
            }

            if (_subTabButton != null)
            {
                _subTabButton.onClick.AddListener(() => SetCurrentTab(StoryQuestType.Sub));
            }

            if (_actionButton != null)
            {
                _actionButton.onClick.AddListener(OnActionButtonClicked);
            }

            Refresh();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<QuestLogChangedEvent>(OnQuestLogChanged);
            Refresh();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<QuestLogChangedEvent>(OnQuestLogChanged);
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
            _questSystem = null;
            _quests = quests;
            Refresh();
        }

        public void SetQuestList(StoryQuestListSO questList)
        {
            _questListAsset = questList;
            _questSystem = null;
            Refresh();
        }

        public void SetCurrentTab(StoryQuestType type)
        {
            _currentTab = type;
            Refresh();
        }

        private void OnQuestLogChanged(QuestLogChangedEvent evt)
        {
            Refresh();
        }

        private void Refresh()
        {
            RebuildVisibleQuests();
            BuildList();
            SelectFirstQuest();
            ApplyTabVisuals();
        }

        private void RebuildVisibleQuests()
        {
            _visibleQuests.Clear();
            IEnumerable<StoryQuestSO> source = GetQuestSource();
            foreach (StoryQuestSO quest in source)
            {
                if (quest != null && quest.Type == _currentTab)
                {
                    _visibleQuests.Add(quest);
                }
            }
        }

        private IEnumerable<StoryQuestSO> GetQuestSource()
        {
            if (_questSystem != null)
            {
                return _questSystem.RegisteredQuests;
            }

            if (_questListAsset != null)
            {
                return _questListAsset.GetAllQuests();
            }

            return _quests ?? System.Array.Empty<StoryQuestSO>();
        }

        private void SelectFirstQuest()
        {
            foreach (StoryQuestSO quest in _visibleQuests)
            {
                if (quest != null)
                {
                    SelectQuest(quest);
                    return;
                }
            }

            ClearDetails();
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

            foreach (StoryQuestSO quest in _visibleQuests)
            {
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

            _selectedQuest = quest;
            SetSelectedButton(selectedButton);
            SetText(_titleText, quest.Title);
            SetText(_areaText, string.IsNullOrWhiteSpace(quest.Area) ? string.Empty : $"◇ {quest.Area}");
            SetText(_objectiveText, $"▶ {quest.Objective}");
            SetText(_descriptionText, quest.Description);
            SetText(_progressText, quest.ProgressText);
            BuildRewards(quest.Rewards);
            ApplyActionState(quest);
        }

        private void ClearDetails()
        {
            _selectedQuest = null;
            SetText(_titleText, _currentTab == StoryQuestType.Main ? "진행 중인 메인 임무 없음" : "진행 중인 서브 임무 없음");
            SetText(_areaText, string.Empty);
            SetText(_objectiveText, string.Empty);
            SetText(_descriptionText, string.Empty);
            SetText(_progressText, string.Empty);
            SetText(_lockReasonText, string.Empty);
            BuildRewards(null);
            SetActionButton(false, "수락");
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

        private void ApplyTabVisuals()
        {
            SetTabVisual(_mainTabButton, _currentTab == StoryQuestType.Main);
            SetTabVisual(_subTabButton, _currentTab == StoryQuestType.Sub);
        }

        private void OnActionButtonClicked()
        {
            if (_selectedQuest == null)
            {
                return;
            }

            bool accepted = _questSystem != null
                ? _questSystem.AcceptQuest(_selectedQuest)
                : TryAcceptWithoutSystem(_selectedQuest);

            if (accepted)
            {
                ApplyActionState(_selectedQuest);
            }
        }

        private void ApplyActionState(StoryQuestSO quest)
        {
            if (quest == null)
            {
                SetActionButton(false, "수락");
                SetText(_lockReasonText, string.Empty);
                return;
            }

            if (quest.Completed)
            {
                SetActionButton(false, "완료");
                SetText(_lockReasonText, string.Empty);
                return;
            }

            if (quest.Accepted)
            {
                SetActionButton(false, "진행 중");
                SetText(_lockReasonText, string.Empty);
                return;
            }

            bool canAccept = _questSystem != null ? _questSystem.CanAcceptQuest(quest) : quest.CanStart(null);
            SetActionButton(canAccept, canAccept ? "수락" : "잠김");
            SetText(_lockReasonText, canAccept ? string.Empty : quest.LockedMessage);
        }

        private void SetActionButton(bool interactable, string label)
        {
            if (_actionButton != null)
            {
                _actionButton.interactable = interactable;
            }

            SetText(_actionButtonText, label);
        }

        private static bool TryAcceptWithoutSystem(StoryQuestSO quest)
        {
            if (quest == null || quest.Completed || !quest.CanStart(null))
            {
                return false;
            }

            quest.Accept();
            GameEventBus.Publish(new QuestObjectiveChangedEvent(quest.Objective, null, quest.DistanceMeters, quest.ProgressText));
            GameEventBus.Publish(new QuestAcceptedEvent(quest));
            GameEventBus.Publish(new QuestLogChangedEvent());
            return true;
        }

        private static void SetTabVisual(Button button, bool selected)
        {
            if (button == null || button.targetGraphic == null)
            {
                return;
            }

            button.targetGraphic.color = selected
                ? new Color(0.95f, 0.87f, 0.62f, 0.92f)
                : new Color(1f, 1f, 1f, 0.18f);
        }

        private static string BuildQuestButtonLabel(StoryQuestSO quest)
        {
            string marker = quest.Type == StoryQuestType.Main ? "◇" : "◆";
            string chapter = string.IsNullOrWhiteSpace(quest.ChapterName) ? string.Empty : $"\n<size=80%>{quest.ChapterName}</size>";
            string distance = quest.DistanceMeters > 0 ? $"\n<size=75%>{quest.DistanceMeters}m</size>" : string.Empty;
            string completed = quest.Completed ? "  <size=75%>완료</size>" : string.Empty;
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
