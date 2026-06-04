using UnityEngine;

namespace DeadLetterOffice.UI
{
    [CreateAssetMenu(menuName = "Dead Letter Office/UI/Help Entry")]
    public class HelpEntrySO : ScriptableObject
    {
        [SerializeField] private string _title = "이동과 카메라";
        [SerializeField] private string _subtitle = "탐색 중 기본 조작";
        [TextArea(3, 12)]
        [SerializeField] private string _body = "플레이어 이동, 카메라 조작, UI 사용법을 적습니다.";

        public string Title => _title;
        public string Subtitle => _subtitle;
        public string Body => _body;
    }
}
