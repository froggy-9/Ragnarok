using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeadLetterOffice.UI
{
    [System.Serializable]
    public struct HelpArchiveEntry
    {
        public string Title;
        public string Subtitle;
        [TextArea(3, 10)]
        public string Body;
    }

    public class HelpArchiveUI : MonoBehaviour
    {
        [SerializeField] private Transform _entryList;
        [SerializeField] private Button _entryButtonTemplate;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _subtitleText;
        [SerializeField] private TMP_Text _bodyText;
        [SerializeField] private HelpEntrySO[] _entryAssets;
        [SerializeField] private HelpArchiveEntry[] _entries;

        private void Awake()
        {
            EnsureDefaultEntries();
            BuildList();
            SelectFirstEntry();
        }

        public void SetEntries(HelpArchiveEntry[] entries)
        {
            _entries = entries;
            EnsureDefaultEntries();
            BuildList();
            SelectFirstEntry();
        }

        public void SetEntryAssets(HelpEntrySO[] entries)
        {
            _entryAssets = entries;
            BuildList();
            SelectFirstEntry();
        }

        private void BuildList()
        {
            if (_entryList == null || _entryButtonTemplate == null)
            {
                return;
            }

            ClearChildren(_entryList, _entryButtonTemplate.gameObject);
            _entryButtonTemplate.gameObject.SetActive(false);

            if (_entryAssets != null && _entryAssets.Length > 0)
            {
                for (int i = 0; i < _entryAssets.Length; i++)
                {
                    int index = i;
                    HelpEntrySO entry = _entryAssets[i];
                    if (entry == null)
                    {
                        continue;
                    }

                    Button button = Instantiate(_entryButtonTemplate, _entryList);
                    button.gameObject.SetActive(true);
                    SetButtonLabel(button, entry.Title);
                    button.onClick.AddListener(() => SelectEntryAsset(index));
                }

                return;
            }

            for (int i = 0; i < _entries.Length; i++)
            {
                int index = i;
                Button button = Instantiate(_entryButtonTemplate, _entryList);
                button.gameObject.SetActive(true);
                SetButtonLabel(button, _entries[i].Title);
                button.onClick.AddListener(() => SelectEntry(index));
            }
        }

        private void SelectFirstEntry()
        {
            if (_entryAssets != null && _entryAssets.Length > 0)
            {
                SelectEntryAsset(0);
                return;
            }

            SelectEntry(0);
        }

        private void SelectEntryAsset(int index)
        {
            if (_entryAssets == null || index < 0 || index >= _entryAssets.Length || _entryAssets[index] == null)
            {
                return;
            }

            HelpEntrySO entry = _entryAssets[index];
            SetText(_titleText, entry.Title);
            SetText(_subtitleText, entry.Subtitle);
            SetText(_bodyText, entry.Body);
        }

        private void SelectEntry(int index)
        {
            if (_entries == null || _entries.Length == 0 || index < 0 || index >= _entries.Length)
            {
                return;
            }

            HelpArchiveEntry entry = _entries[index];
            SetText(_titleText, entry.Title);
            SetText(_subtitleText, entry.Subtitle);
            SetText(_bodyText, entry.Body);
        }

        private void EnsureDefaultEntries()
        {
            if ((_entryAssets != null && _entryAssets.Length > 0) || (_entries != null && _entries.Length > 0))
            {
                return;
            }

            _entries = new[]
            {
                new HelpArchiveEntry
                {
                    Title = "이동과 카메라",
                    Subtitle = "탐색 중 기본 조작",
                    Body = "플레이어는 하나의 캐릭터를 조작하며 맵을 탐색합니다.\n\n- WASD로 이동합니다.\n- Shift로 달립니다.\n- 마우스로 시점을 조절합니다.\n- 스토리 진행 중 특정 위치에 도착하면 별도의 카메라 연출이 재생될 수 있습니다."
                },
                new HelpArchiveEntry
                {
                    Title = "지도와 위치",
                    Subtitle = "미니맵과 전체 지도",
                    Body = "미니맵은 플레이 중 계속 표시됩니다.\n\n- 미니맵 버튼을 누르면 전체 지도 화면이 열립니다.\n- 전체 지도에는 플레이어의 현재 위치가 표시됩니다.\n- 지도 이미지는 나중에 실제 맵 렌더나 제작 이미지로 교체할 수 있습니다."
                },
                new HelpArchiveEntry
                {
                    Title = "임무 추적",
                    Subtitle = "메인 스토리와 서브 스토리",
                    Body = "왼쪽 임무 문구를 누르면 임무 화면이 열립니다.\n\n- 메인 스토리는 개척 임무로 표시합니다.\n- 서브 스토리는 모험 임무로 표시합니다.\n- 보상이 없는 임무는 보상 영역이 숨겨집니다."
                },
                new HelpArchiveEntry
                {
                    Title = "추리보드",
                    Subtitle = "단서 연결과 추론",
                    Body = "추리보드는 스토리 진행 중 해금됩니다.\n\n- 인물, 물건, 장소 같은 단서를 카드로 다룹니다.\n- 스토리 진행에 따라 새로운 단서 연결이 열립니다.\n- 실제 화면 구성은 다음 단계에서 더 다듬을 수 있습니다."
                }
            };
        }

        private static void SetButtonLabel(Button button, string text)
        {
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = text;
            }
        }

        private static void SetText(TMP_Text target, string text)
        {
            if (target != null)
            {
                target.text = text;
            }
        }

        private static void ClearChildren(Transform parent, GameObject keep)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child.gameObject != keep)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}
