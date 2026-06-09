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

    [CreateAssetMenu(menuName = "Dead Letter Office/UI/Story Quest")]
    public class StoryQuestSO : ScriptableObject
    {
        [SerializeField] private StoryQuestType _type = StoryQuestType.Main;
        [SerializeField] private bool _completed;
        [SerializeField] private string _title = "심연으로 추락한 자들";
        [SerializeField] private string _chapterName = "Noctua Chapter: Act I";
        [SerializeField] private string _area = "아벨른-VI 큰 황금구역";
        [SerializeField] private string _objective = "가정용 탐지기 로봇 부품 찾기";
        [Min(0)]
        [SerializeField] private int _distanceMeters;
        [TextArea(3, 8)]
        [SerializeField] private string _description = "의뢰 내용을 확인하고 다음 단서를 추적한다.";
        [SerializeField] private StoryQuestReward[] _rewards;

        public StoryQuestType Type => _type;
        public bool Completed => _completed;
        public string Title => _title;
        public string ChapterName => _chapterName;
        public string Area => _area;
        public string Objective => _objective;
        public int DistanceMeters => _distanceMeters;
        public string Description => _description;
        public StoryQuestReward[] Rewards => _rewards;
    }
}
