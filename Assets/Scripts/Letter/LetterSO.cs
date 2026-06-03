using UnityEngine;

namespace DeadLetterOffice.Letter
{
    [CreateAssetMenu(menuName = "DLO/Letter/Letter")]
    public class LetterSO : ScriptableObject
    {
        [SerializeField] private string _letterNumber;
        [SerializeField] private string _recipient;
        [SerializeField] private string _sender;
        [SerializeField] private string _postmark;
        [SerializeField] private string _discoveryLocation;
        [SerializeField] private Sprite _letterImage;
        [SerializeField] private LetterSegmentSO[] _segments;

        public string LetterId => name;
        public string LetterNumber => _letterNumber;
        public string Recipient => _recipient;
        public string Sender => _sender;
        public string Postmark => _postmark;
        public string DiscoveryLocation => _discoveryLocation;
        public Sprite LetterImage => _letterImage;
        public LetterSegmentSO[] Segments => _segments;
    }
}
