using DeadLetterOffice.Core;
using DeadLetterOffice.Letter;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.UI
{
    public enum StoryQuestType
    {
        Main,
        Sub,
        Commission
    }

    [System.Serializable]
    public struct StoryQuestReward
    {
        [SerializeField] private QuestRewardItemSO _item;
        [Min(1)]
        [SerializeField] private int _amount;

        public QuestRewardItemSO Item => _item;
        public int Amount => Mathf.Max(1, _amount);
        public string ItemName => _item != null ? _item.ItemName : "보상";
        public Sprite Icon => _item != null ? _item.Icon : null;
    }

    [System.Serializable]
    public struct StoryQuestStep
    {
        [SerializeField] private string _objective;
        [SerializeField] private string _location;
        [Min(0)]
        [SerializeField] private int _distanceMeters;
        [TextArea(2, 6)]
        [SerializeField] private string _description;
        [SerializeField] private string _progressFormat;
        [Min(0)]
        [SerializeField] private int _progressCurrent;
        [Min(1)]
        [SerializeField] private int _progressRequired;
        [SerializeField] private FlagSO _completedFlag;

        public string Objective => _objective;
        public string Location => _location;
        public int DistanceMeters => _distanceMeters;
        public string Description => _description;
        public int ProgressCurrent => Mathf.Clamp(_progressCurrent, 0, ProgressRequired);
        public int ProgressRequired => Mathf.Max(1, _progressRequired);
        public string ProgressText => string.Format(string.IsNullOrWhiteSpace(_progressFormat) ? "{0}/{1}" : _progressFormat, ProgressCurrent, ProgressRequired);
        public bool HasObjective => !string.IsNullOrWhiteSpace(_objective);
        public bool HasLocation => !string.IsNullOrWhiteSpace(_location);
        public bool HasDescription => !string.IsNullOrWhiteSpace(_description);
        public bool HasProgress => _progressRequired > 1 || _progressCurrent > 0 || !string.IsNullOrWhiteSpace(_progressFormat);
        public bool IsCompleted => _completedFlag != null && _completedFlag.Value;

        public void SetProgress(int current, int required)
        {
            _progressCurrent = Mathf.Max(0, current);
            _progressRequired = Mathf.Max(1, required);
        }
    }

    [CreateAssetMenu(menuName = "DLO/Quest/Quest")]
    public class StoryQuestSO : ScriptableObject
    {
        [Header("Unlock Conditions")]
        [SerializeField] private StoryQuestType _type = StoryQuestType.Main;
        [SerializeField] private bool _availableByDefault = true;
        [SerializeField] private FlagSO[] _requiredFlags;
        [SerializeField] private CollectibleSO[] _requiredUnlockItems;
        [SerializeField] private StoryQuestSO[] _requiredCompletedUnlockQuests;

        [Header("Start Conditions")]
        [SerializeField] private FlagSO[] _requiredStartFlags;
        [SerializeField] private CollectibleSO[] _requiredItems;
        [SerializeField] private StoryQuestSO[] _requiredCompletedQuests;

        [Header("Runtime State")]
        [SerializeField] private FlagSO _completionFlag;
        [SerializeField] private bool _completed;
        private bool _accepted;

        [Header("Display")]
        [SerializeField] private string _title = "관리자가 알려준 작업";
        [SerializeField] private string _chapterName = "Chapter 1";
        [SerializeField] private string _area = "야간 우체국";
        [SerializeField] private string _objective = "관리자가 알려준 작업대로 가기";
        [Min(0)]
        [SerializeField] private int _distanceMeters = 49;
        [TextArea(3, 8)]
        [SerializeField] private string _description = "의뢰 내용을 확인하고 다음 단서를 추적한다.";
        [SerializeField] private string _lockedMessage = "조건을 만족해야 진행할 수 있다.";
        [SerializeField] private string _progressFormat = "{0}/{1}";
        [Min(0)]
        [SerializeField] private int _progressCurrent;
        [Min(1)]
        [SerializeField] private int _progressRequired = 1;
        [SerializeField] private StoryQuestStep[] _steps;
        [SerializeField] private StoryQuestReward[] _rewards;

        public StoryQuestType Type => _type;
        public bool AvailableByDefault => _availableByDefault;
        public bool Accepted => _accepted;
        public bool Completed => IsCompleted();
        public string Title => _title;
        public string ChapterName => _chapterName;
        public string Area => CurrentStep.HasLocation ? CurrentStep.Location : _area;
        public string Objective => CurrentStep.HasObjective ? CurrentStep.Objective : _objective;
        public int DistanceMeters => CurrentStep.HasObjective ? CurrentStep.DistanceMeters : _distanceMeters;
        public string Description => CurrentStep.HasDescription ? CurrentStep.Description : _description;
        public string LockedMessage => _lockedMessage;
        public int ProgressCurrent => Mathf.Clamp(_progressCurrent, 0, ProgressRequired);
        public int ProgressRequired => Mathf.Max(1, _progressRequired);
        public string ProgressText => CurrentStep.HasProgress
            ? CurrentStep.ProgressText
            : string.Format(string.IsNullOrWhiteSpace(_progressFormat) ? "{0}/{1}" : _progressFormat, ProgressCurrent, ProgressRequired);
        public StoryQuestStep[] Steps => _steps;
        public int CurrentStepIndex => GetCurrentStepIndex();
        public int StepCount => _steps != null ? _steps.Length : 0;
        public StoryQuestReward[] Rewards => _rewards;

        public bool CanRegister()
        {
            return _availableByDefault || ConditionUtility.AreFlagsMet(_requiredFlags);
        }

        public bool CanRegister(GameStateSO gameState)
        {
            return _availableByDefault || AreUnlockConditionsMet(gameState);
        }

        public bool CanStart(GameStateSO gameState)
        {
            return ConditionUtility.AreFlagsMet(_requiredStartFlags)
                && ConditionUtility.AreItemsMet(gameState, _requiredItems)
                && AreRequiredQuestsCompleted();
        }

        public bool IsCompleted()
        {
            return _completed || (_completionFlag != null && _completionFlag.Value);
        }

        public void Accept()
        {
            _accepted = true;
        }

        public void Cancel()
        {
            _accepted = false;
        }

        public void SetProgress(int current, int required)
        {
            _progressCurrent = Mathf.Max(0, current);
            _progressRequired = Mathf.Max(1, required);
        }

        public void SetStepProgress(int stepIndex, int current, int required)
        {
            if (_steps == null || stepIndex < 0 || stepIndex >= _steps.Length)
            {
                return;
            }

            _steps[stepIndex].SetProgress(current, required);
        }

        private void OnDisable()
        {
            _accepted = false;
        }

        private bool AreUnlockConditionsMet(GameStateSO gameState)
        {
            return ConditionUtility.AreFlagsMet(_requiredFlags)
                && ConditionUtility.AreItemsMet(gameState, _requiredUnlockItems)
                && AreQuestsCompleted(_requiredCompletedUnlockQuests);
        }

        private bool AreRequiredQuestsCompleted()
        {
            return AreQuestsCompleted(_requiredCompletedQuests);
        }

        private StoryQuestStep CurrentStep
        {
            get
            {
                if (_steps == null || _steps.Length == 0)
                {
                    return default;
                }

                return _steps[GetCurrentStepIndex()];
            }
        }

        private int GetCurrentStepIndex()
        {
            if (_steps == null || _steps.Length == 0)
            {
                return -1;
            }

            for (int i = 0; i < _steps.Length; i++)
            {
                if (!_steps[i].IsCompleted)
                {
                    return i;
                }
            }

            return _steps.Length - 1;
        }

        private static bool AreQuestsCompleted(StoryQuestSO[] quests)
        {
            if (quests == null || quests.Length == 0)
            {
                return true;
            }

            foreach (StoryQuestSO quest in quests)
            {
                if (quest == null)
                {
                    Debug.LogWarning("[StoryQuestSO] Null required quest is treated as no condition.");
                    continue;
                }

                if (!quest.Completed)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
