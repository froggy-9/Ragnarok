using UnityEngine;

namespace DeadLetterOffice.UI
{
    [CreateAssetMenu(menuName = "Dead Letter Office/UI/Story Quest List")]
    public class StoryQuestListSO : ScriptableObject
    {
        [SerializeField] private string _listName = "진행 중";
        [SerializeField] private StoryQuestSO[] _quests;

        public string ListName => _listName;
        public StoryQuestSO[] Quests => _quests;
    }
}
