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
            EnsureQuestListLayout();

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

        private void Start()
        {
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

            Refresh();
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
                if (quest == null)
                {
                    continue;
                }

                _visibleQuests.Add(quest);
            }
        }

        private IEnumerable<StoryQuestSO> GetQuestSource()
        {
            if (_questSystem != null)
            {
                _questSystem.RefreshRegistrations(false);
                if (HasAnyQuest(_questSystem.RegisteredQuests))
                {
                    return _questSystem.RegisteredQuests;
                }

                if (_questSystem.QuestPipeline != null)
                {
                    return _questSystem.QuestPipeline.GetAllQuests();
                }
            }

            if (_questListAsset != null)
            {
                return _questListAsset.GetAllQuests();
            }

            return _quests ?? System.Array.Empty<StoryQuestSO>();
        }

        private static bool HasAnyQuest(IEnumerable<StoryQuestSO> quests)
        {
            if (quests == null)
            {
                return false;
            }

            foreach (StoryQuestSO quest in quests)
            {
                if (quest != null)
                {
                    return true;
                }
            }

            return false;
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
            if (_questList == null)
            {
                return;
            }

            EnsureQuestListLayout();
            ClearChildren(_questList, _questButtonTemplate != null ? _questButtonTemplate.gameObject : null);
            if (_questButtonTemplate != null)
            {
                _questButtonTemplate.gameObject.SetActive(false);
            }
            _selectedButton = null;

            StoryQuestType? lastType = null;
            int createdCount = 0;
            foreach (StoryQuestSO quest in _visibleQuests)
            {
                if (lastType != quest.Type)
                {
                    CreateGroupHeader(quest.Type);
                    lastType = quest.Type;
                }

                Button button = CreateQuestEntryButton(quest);
                button.onClick.AddListener(() => SelectQuest(quest, button));
                createdCount++;
            }

            ForceQuestListLayout();

            if (createdCount == 0)
            {
                Debug.LogWarning("[QuestLogUI] No quest entry was created. Check QuestSystem/QuestPipeline/QuestListSO references.", this);
            }
            else
            {
                Debug.Log($"[QuestLogUI] Created {createdCount} quest entry button(s) under {_questList.name}.", this);
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
            SetText(_areaText, string.IsNullOrWhiteSpace(quest.Area) ? string.Empty : quest.Area);
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

            bool accepted = _selectedQuest.Accepted
                ? CancelSelectedQuest()
                : AcceptSelectedQuest();

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
                SetActionButton(true, "취소");
                SetText(_lockReasonText, string.Empty);
                return;
            }

            bool canAccept = _questSystem != null ? _questSystem.CanAcceptQuest(quest) : quest.CanStart(null);
            SetActionButton(canAccept, canAccept ? "수락" : "잠김");
            SetText(_lockReasonText, canAccept ? string.Empty : quest.LockedMessage);
        }

        private bool AcceptSelectedQuest()
        {
            return _questSystem != null
                ? _questSystem.AcceptQuest(_selectedQuest)
                : TryAcceptWithoutSystem(_selectedQuest);
        }

        private bool CancelSelectedQuest()
        {
            if (_selectedQuest == null)
            {
                return false;
            }

            if (_questSystem != null)
            {
                return _questSystem.CancelQuest(_selectedQuest);
            }

            _selectedQuest.Cancel();
            GameEventBus.Publish(new QuestObjectiveChangedEvent(string.Empty));
            GameEventBus.Publish(new QuestLogChangedEvent());
            return true;
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
            GameEventBus.Publish(new QuestObjectiveChangedEvent(quest.Objective, null, quest.DistanceMeters, quest.ProgressText, quest.Title, quest.Area));
            GameEventBus.Publish(new QuestAcceptedEvent(quest));
            GameEventBus.Publish(new QuestLogChangedEvent());
            return true;
        }

        private void CreateGroupHeader(StoryQuestType type)
        {
            if (_questList == null)
            {
                return;
            }

            GameObject header = new($"Header_{type}", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
            header.transform.SetParent(_questList, false);

            float entryWidth = GetQuestEntryWidth();
            RectTransform rectTransform = header.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 0.5f);
            rectTransform.anchorMax = new Vector2(0f, 0.5f);
            rectTransform.pivot = new Vector2(0f, 0.5f);
            rectTransform.sizeDelta = new Vector2(entryWidth, 34f);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, entryWidth);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 34f);

            LayoutElement layoutElement = header.GetComponent<LayoutElement>();
            layoutElement.minWidth = entryWidth;
            layoutElement.preferredWidth = entryWidth;
            layoutElement.minHeight = 34f;
            layoutElement.preferredHeight = 34f;
            layoutElement.flexibleWidth = 1f;

            TMP_Text label = header.GetComponent<TMP_Text>();
            label.raycastTarget = false;
            label.alignment = TextAlignmentOptions.Left;
            label.fontSize = 19f;
            label.fontStyle = FontStyles.Bold;
            label.color = new Color(1f, 0.88f, 0.48f, 1f);
            label.text = type switch
            {
                StoryQuestType.Main => "메인 임무",
                StoryQuestType.Commission => "의뢰 임무",
                _ => "서브 임무"
            };
        }

        private Button CreateQuestEntryButton(StoryQuestSO quest)
        {
            GameObject row = new($"QuestEntry_{quest.name}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            row.transform.SetParent(_questList, false);

            float entryWidth = GetQuestEntryWidth();
            RectTransform rectTransform = row.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 0.5f);
            rectTransform.anchorMax = new Vector2(0f, 0.5f);
            rectTransform.pivot = new Vector2(0f, 0.5f);
            rectTransform.sizeDelta = new Vector2(entryWidth, 76f);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, entryWidth);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 76f);

            LayoutElement layoutElement = row.GetComponent<LayoutElement>();
            ApplyQuestEntryLayout(layoutElement, entryWidth);

            Image background = row.GetComponent<Image>();
            ApplyQuestEntryBackground(background, quest);
            background.color = quest.Accepted
                ? new Color(0.95f, 0.72f, 0.25f, 0.75f)
                : new Color(0.2f, 0.38f, 0.75f, 0.75f);
            Outline outline = row.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.18f);
            outline.effectDistance = new Vector2(1f, -1f);

            Button button = row.GetComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;

            GameObject accentObject = new("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            accentObject.transform.SetParent(row.transform, false);
            RectTransform accentRect = accentObject.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.sizeDelta = new Vector2(4f, 0f);
            accentRect.anchoredPosition = Vector2.zero;

            Image accent = accentObject.GetComponent<Image>();
            accent.raycastTarget = false;
            accent.color = quest.Accepted
                ? new Color(1f, 0.9f, 0.46f, 1f)
                : new Color(0.46f, 0.68f, 0.95f, 1f);

            GameObject labelObject = new("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(row.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(18f, 8f);
            labelRect.offsetMax = new Vector2(-12f, -8f);

            TMP_Text label = labelObject.GetComponent<TMP_Text>();
            ApplyQuestEntryLabel(label, quest);

            return button;
        }

        private void ApplyQuestEntryVisual(Button button, StoryQuestSO quest)
        {
            if (button == null)
            {
                return;
            }

            float entryWidth = GetQuestEntryWidth();
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0f, 0.5f);
                rectTransform.anchorMax = new Vector2(0f, 0.5f);
                rectTransform.pivot = new Vector2(0f, 0.5f);
                rectTransform.sizeDelta = new Vector2(entryWidth, 76f);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, entryWidth);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 76f);
            }

            LayoutElement layoutElement = button.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = button.gameObject.AddComponent<LayoutElement>();
            }

            ApplyQuestEntryLayout(layoutElement, entryWidth);

            CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            Image background = button.GetComponent<Image>();
            if (background == null)
            {
                background = button.gameObject.AddComponent<Image>();
            }

            ApplyQuestEntryBackground(background, quest);
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            button.enabled = true;

            CanvasRenderer canvasRenderer = button.GetComponent<CanvasRenderer>();
            if (canvasRenderer != null)
            {
                canvasRenderer.SetAlpha(1f);
                canvasRenderer.cull = false;
            }

            Outline outline = button.GetComponent<Outline>();
            if (outline == null)
            {
                outline = button.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = new Color(1f, 1f, 1f, 0.2f);
            outline.effectDistance = new Vector2(1f, -1f);

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label == null)
            {
                label = CreateQuestEntryLabel(button.transform);
            }

            ApplyQuestEntryLabel(label, quest);
        }

        private void ApplyQuestEntryLabel(TMP_Text label, StoryQuestSO quest)
        {
            if (label == null)
            {
                return;
            }

            label.gameObject.SetActive(true);
            CopyTextStyle(label);
            label.raycastTarget = false;
            label.alignment = TextAlignmentOptions.Left;
            label.enableAutoSizing = true;
            label.fontSizeMin = 13f;
            label.fontSizeMax = 18f;
            label.color = new Color(0.98f, 0.98f, 0.94f, 1f);
            label.text = BuildQuestButtonLabel(quest);

            RectTransform labelRect = label.GetComponent<RectTransform>();
            if (labelRect != null)
            {
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(14f, 8f);
                labelRect.offsetMax = new Vector2(-10f, -8f);
            }
        }

        private TMP_Text CreateQuestEntryLabel(Transform parent)
        {
            GameObject labelObject = new("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);
            return labelObject.GetComponent<TMP_Text>();
        }

        private static void ApplyQuestEntryLayout(LayoutElement layoutElement, float entryWidth)
        {
            if (layoutElement == null)
            {
                return;
            }

            layoutElement.minWidth = entryWidth;
            layoutElement.preferredWidth = entryWidth;
            layoutElement.minHeight = 76f;
            layoutElement.preferredHeight = 76f;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;
        }

        private static void ApplyQuestEntryBackground(Graphic background, StoryQuestSO quest)
        {
            if (background == null)
            {
                return;
            }

            background.raycastTarget = true;
            background.color = quest.Accepted ? AcceptedQuestColor() : DefaultQuestColor();
        }

        private float GetQuestEntryWidth()
        {
            const float fallbackWidth = 320f;
            RectTransform listRect = _questList as RectTransform;
            if (listRect != null && listRect.rect.width > 16f)
            {
                return Mathf.Max(220f, listRect.rect.width - 8f);
            }

            RectTransform parentRect = _questList != null ? _questList.parent as RectTransform : null;
            if (parentRect != null && parentRect.rect.width > 16f)
            {
                return Mathf.Max(220f, parentRect.rect.width - 16f);
            }

            return fallbackWidth;
        }

        private void CopyTextStyle(TMP_Text target)
        {
            if (target == null)
            {
                return;
            }

            TMP_Text source = _questButtonTemplate != null
                ? _questButtonTemplate.GetComponentInChildren<TMP_Text>(true)
                : _titleText;

            if (source == null)
            {
                return;
            }

            target.font = source.font;
            target.fontSharedMaterial = source.fontSharedMaterial;
        }

        private void EnsureQuestListLayout()
        {
            if (_questList == null)
            {
                return;
            }

            RectTransform rectTransform = _questList as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(0f, rectTransform.sizeDelta.y);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, GetParentWidth(rectTransform, 320f));
            }

            VerticalLayoutGroup layout = _questList.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = _questList.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layout.childAlignment = TextAnchor.UpperLeft;
            layout.spacing = 8f;
            layout.padding = new RectOffset(0, 8, 0, 12);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = _questList.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = _questList.gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            NormalizeViewportMask();
        }

        private void NormalizeViewportMask()
        {
            if (_questList == null || _questList.parent == null)
            {
                return;
            }

            Transform viewport = _questList.parent;
            Mask mask = viewport.GetComponent<Mask>();
            if (mask != null)
            {
                mask.showMaskGraphic = false;
            }

            Image maskImage = viewport.GetComponent<Image>();
            if (maskImage != null)
            {
                maskImage.color = new Color(1f, 1f, 1f, 1f);
                maskImage.raycastTarget = false;
            }
        }

        private static float GetParentWidth(RectTransform rectTransform, float fallback)
        {
            RectTransform parent = rectTransform.parent as RectTransform;
            if (parent != null && parent.rect.width > 16f)
            {
                return parent.rect.width;
            }

            return fallback;
        }

        private void ForceQuestListLayout()
        {
            RectTransform rectTransform = _questList as RectTransform;
            if (rectTransform == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
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
            string location = string.IsNullOrWhiteSpace(quest.Area) ? string.Empty : $"\n<size=75%>{quest.Area}</size>";
            string distance = quest.DistanceMeters > 0 ? $" <size=75%>{quest.DistanceMeters}m</size>" : string.Empty;
            string completed = quest.Completed ? "  <size=75%>완료</size>" : string.Empty;
            return $"{marker} {quest.Title}{completed}{chapter}{location}{distance}";
        }

        private void SetSelectedButton(Button selectedButton)
        {
            if (_selectedButton != null && _selectedButton.targetGraphic != null)
            {
                _selectedButton.targetGraphic.color = DefaultQuestColor();
            }

            _selectedButton = selectedButton;

            if (_selectedButton != null && _selectedButton.targetGraphic != null)
            {
                _selectedButton.targetGraphic.color = SelectedQuestColor();
            }
        }

        private static Color DefaultQuestColor()
        {
            return new Color(1f, 1f, 1f, 0.16f);
        }

        private static Color AcceptedQuestColor()
        {
            return new Color(1f, 0.88f, 0.38f, 0.28f);
        }

        private static Color SelectedQuestColor()
        {
            return new Color(1f, 0.88f, 0.38f, 0.38f);
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
                if (keep != null && child.gameObject == keep)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                    continue;
                }

                DestroyImmediate(child.gameObject);
            }
        }
    }
}
