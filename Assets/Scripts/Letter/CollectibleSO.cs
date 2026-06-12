using UnityEngine;

namespace DeadLetterOffice.Letter
{
    public enum CollectibleCategory
    {
        Evidence,
        Letter,
        Material,
        Consumable,
        KeyItem,
        Misc
    }

    [CreateAssetMenu(menuName = "DLO/Collectible")]
    public class CollectibleSO : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(2, 6)] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private CollectibleCategory _category = CollectibleCategory.Evidence;
        [SerializeField] private bool _canRemove;
        [SerializeField] private bool _stackable = true;
        [SerializeField, Range(1, 999)] private int _maxStack = 99;

        public string ItemId => name;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public CollectibleCategory Category => _category;
        public bool CanRemove => _canRemove;
        public bool Stackable => _stackable;
        public int MaxStack => _stackable ? _maxStack : 1;
    }
}
