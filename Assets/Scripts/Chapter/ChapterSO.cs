using UnityEngine;

namespace DeadLetterOffice.Chapter
{
    [CreateAssetMenu(menuName = "DLO/Chapter/Chapter")]
    public class ChapterSO : ScriptableObject
    {
        [SerializeField] private int _chapterNumber = 1;
        [SerializeField] private string _displayName;
        [SerializeField] private BeatSO[] _beats;

        public int ChapterNumber => _chapterNumber;
        public string DisplayName => _displayName;
        public BeatSO[] Beats => _beats;
    }
}
