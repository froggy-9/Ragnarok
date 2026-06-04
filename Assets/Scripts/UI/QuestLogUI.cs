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
        [SerializeField] private StoryQuestSO[] _quests;

        private void Awake()
        {
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
            _quests = quests;
            BuildList();
            SelectFirstQuest();
        }

        private void SelectFirstQuest()
        {
            if (_quests != null && _quests.Length > 0)
            {
                SelectQuest(_quests[0]);
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

            if (_quests == null)
            {
                return;
            }

            foreach (StoryQuestSO quest in _quests)
            {
                if (quest == null)
                {
                    continue;
                }

                Button button = Instantiate(_questButtonTemplate, _questList);
                button.gameObject.SetActive(true);
                TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    string prefix = quest.Type == StoryQuestType.Main ? "개척 임무" : "모험 임무";
                    label.text = $"{prefix}\n{quest.Title}";
                }

                button.onClick.AddListener(() => SelectQuest(quest));
            }
        }

        private void SelectQuest(StoryQuestSO quest)
        {
            if (quest == null)
            {
                return;
            }

            SetText(_titleText, quest.Title);
            SetText(_areaText, quest.Area);
            SetText(_objectiveText, quest.Objective);
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
                    label.text = reward.Amount > 1 ? $"{reward.Name} x{reward.Amount}" : reward.Name;
                }

                Image icon = item.GetComponentInChildren<Image>();
                if (icon != null && reward.Icon != null)
                {
                    icon.sprite = reward.Icon;
                }
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
