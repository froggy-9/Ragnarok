using UnityEngine;

namespace DeadLetterOffice.UI
{
    public enum StoryQuestType
    {
        Main,
        Sub
    }

    [System.Serializable]
    public struct StoryQuestReward
    {
        public string Name;
        public Sprite Icon;
        public int Amount;
    }

    [CreateAssetMenu(menuName = "Dead Letter Office/UI/Story Quest")]
    public class StoryQuestSO : ScriptableObject
    {
        [SerializeField] private StoryQuestType _type = StoryQuestType.Main;
        [SerializeField] private string _title = "심연으로 추락한 자들";
        [SerializeField] private string _area = "임시 구역";
        [SerializeField] private string _objective = "가정용 탐지기 로봇 부품 찾기";
        [TextArea(3, 8)]
        [SerializeField] private string _description = "임무 내용을 확인하고 다음 단서를 추적한다.";
        [SerializeField] private StoryQuestReward[] _rewards;

        public StoryQuestType Type => _type;
        public string Title => _title;
        public string Area => _area;
        public string Objective => _objective;
        public string Description => _description;
        public StoryQuestReward[] Rewards => _rewards;
    }
}
