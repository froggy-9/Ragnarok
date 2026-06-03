using UnityEngine;

namespace DeadLetterOffice.Letter
{
    [CreateAssetMenu(menuName = "DLO/Collectible")]
    public class CollectibleSO : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(2, 6)] private string _description;
        [SerializeField] private Sprite _icon;

        public string ItemId => name;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
    }
}
