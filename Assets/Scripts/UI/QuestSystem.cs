using System.Collections.Generic;
using DeadLetterOffice.Core;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.UI
{
    public class QuestSystem : MonoBehaviour
    {
        [SerializeField] private StoryQuestListSO _questPipeline;
        [SerializeField] private GameStateSO _gameState;
        [SerializeField] private bool _showInitialUnlockToasts;

        private readonly List<StoryQuestSO> _registeredQuests = new();
        private readonly HashSet<StoryQuestSO> _registeredQuestSet = new();
        private StoryQuestSO _acceptedQuest;

        public IReadOnlyList<StoryQuestSO> RegisteredQuests => _registeredQuests;
        public StoryQuestListSO QuestPipeline => _questPipeline;
        public StoryQuestSO AcceptedQuest => _acceptedQuest;

        private void OnEnable()
        {
            GameEventBus.Subscribe<FlagChangedEvent>(OnFlagChanged);
            GameEventBus.Subscribe<ItemCollectedEvent>(OnItemCollected);
            RefreshRegistrations(_showInitialUnlockToasts);
            PublishCurrentObjective();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<FlagChangedEvent>(OnFlagChanged);
            GameEventBus.Unsubscribe<ItemCollectedEvent>(OnItemCollected);
        }

        public void SetQuestPipeline(StoryQuestListSO questPipeline)
        {
            _questPipeline = questPipeline;
            _registeredQuests.Clear();
            _registeredQuestSet.Clear();
            RefreshRegistrations(_showInitialUnlockToasts);
            PublishCurrentObjective();
            GameEventBus.Publish(new QuestLogChangedEvent());
        }

        public void RefreshRegistrations(bool showUnlockToasts = true)
        {
            if (_questPipeline == null)
            {
                return;
            }

            bool changed = false;
            foreach (StoryQuestSO quest in _questPipeline.GetAllQuests())
            {
                if (quest == null || _registeredQuestSet.Contains(quest) || !quest.CanRegister(_gameState))
                {
                    continue;
                }

                _registeredQuestSet.Add(quest);
                _registeredQuests.Add(quest);
                changed = true;

                if (showUnlockToasts)
                {
                    GameEventBus.Publish(new QuestUnlockedEvent(quest));
                }
            }

            if (changed)
            {
                GameEventBus.Publish(new QuestLogChangedEvent());
            }
        }

        public bool AcceptQuest(StoryQuestSO quest)
        {
            if (quest == null || !_registeredQuestSet.Contains(quest) || quest.Completed || !quest.CanStart(_gameState))
            {
                return false;
            }

            if (_acceptedQuest != null && _acceptedQuest != quest)
            {
                _acceptedQuest.Cancel();
            }

            quest.Accept();
            _acceptedQuest = quest;
            PublishCurrentObjective();
            GameEventBus.Publish(new QuestAcceptedEvent(quest));
            GameEventBus.Publish(new QuestLogChangedEvent());
            return true;
        }

        public bool CancelQuest(StoryQuestSO quest)
        {
            if (quest == null || !quest.Accepted)
            {
                return false;
            }

            quest.Cancel();
            if (_acceptedQuest == quest)
            {
                _acceptedQuest = null;
            }

            PublishCurrentObjective();
            GameEventBus.Publish(new QuestLogChangedEvent());
            return true;
        }

        public bool CanAcceptQuest(StoryQuestSO quest)
        {
            return quest != null && _registeredQuestSet.Contains(quest) && !quest.Completed && quest.CanStart(_gameState);
        }

        private void OnFlagChanged(FlagChangedEvent evt)
        {
            RefreshRegistrations(true);
            PublishCurrentObjective();
            GameEventBus.Publish(new QuestLogChangedEvent());
        }

        private void OnItemCollected(ItemCollectedEvent evt)
        {
            RefreshRegistrations(true);
            PublishCurrentObjective();
            GameEventBus.Publish(new QuestLogChangedEvent());
        }

        private void PublishCurrentObjective()
        {
            StoryQuestSO quest = GetCurrentQuest();
            if (quest == null)
            {
                GameEventBus.Publish(new QuestObjectiveChangedEvent(string.Empty));
                return;
            }

            GameEventBus.Publish(new QuestObjectiveChangedEvent(quest.Objective, null, quest.DistanceMeters, quest.ProgressText, quest.Title, quest.Area));
        }

        private StoryQuestSO GetCurrentQuest()
        {
            if (_acceptedQuest != null && !_acceptedQuest.Completed)
            {
                return _acceptedQuest;
            }

            foreach (StoryQuestSO quest in _registeredQuests)
            {
                if (quest != null && quest.Accepted && !quest.Completed)
                {
                    return quest;
                }
            }

            return null;
        }
    }
}
