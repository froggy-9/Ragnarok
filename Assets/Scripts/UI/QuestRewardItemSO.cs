using UnityEngine;

namespace DeadLetterOffice.UI
{
    [CreateAssetMenu(menuName = "Dead Letter Office/UI/Quest Reward Item")]
    public class QuestRewardItemSO : ScriptableObject
    {
        [SerializeField] private string _itemName = "단서";
        [SerializeField] private Sprite _icon;

        public string ItemName => _itemName;
        public Sprite Icon => _icon;
    }
}
