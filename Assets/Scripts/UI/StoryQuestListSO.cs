using System.Collections.Generic;
using UnityEngine;

namespace DeadLetterOffice.UI
{
    [CreateAssetMenu(menuName = "DLO/Quest/Quest Pipeline")]
    public class StoryQuestListSO : ScriptableObject
    {
        [SerializeField] private string _listName = "챕터 1 퀘스트 파이프라인";
        [SerializeField] private StoryQuestSO[] _mainQuests;
        [SerializeField] private StoryQuestSO[] _subQuests;

        public string ListName => _listName;
        public StoryQuestSO[] MainQuests => _mainQuests;
        public StoryQuestSO[] SubQuests => _subQuests;

        public IEnumerable<StoryQuestSO> GetAllQuests()
        {
            foreach (StoryQuestSO quest in Enumerate(_mainQuests))
            {
                yield return quest;
            }

            foreach (StoryQuestSO quest in Enumerate(_subQuests))
            {
                yield return quest;
            }
        }

        private static IEnumerable<StoryQuestSO> Enumerate(StoryQuestSO[] quests)
        {
            if (quests == null)
            {
                yield break;
            }

            foreach (StoryQuestSO quest in quests)
            {
                if (quest != null)
                {
                    yield return quest;
                }
            }
        }
    }
}
