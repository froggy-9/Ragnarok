using System.Collections.Generic;
using DeadLetterOffice.Core;
using DeadLetterOffice.Letter;
using DeadLetterOffice.State;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeadLetterOffice.UI
{
    public class InventoryArchiveUI : MonoBehaviour
    {
        [SerializeField] private GameStateSO _gameState;
        [SerializeField] private Transform _tabRoot;
        [SerializeField] private Button _tabButtonTemplate;
        [SerializeField] private Transform _gridRoot;
        [SerializeField] private GameObject _slotTemplate;
        [SerializeField] private TMP_Text _capacityText;
        [SerializeField] private TMP_Text _detailNameText;
        [SerializeField] private TMP_Text _detailCategoryText;
        [SerializeField] private TMP_Text _detailDescriptionText;
        [SerializeField] private Image _detailIcon;
        [SerializeField] private Button _removeButton;

        private readonly List<Button> _tabButtons = new();
        private readonly List<GameObject> _slots = new();
        private CollectibleCategory _selectedCategory = CollectibleCategory.Evidence;
        private CollectibleSO _selectedItem;

        private void OnEnable()
        {
            GameEventBus.Subscribe<ArchiveRefreshedEvent>(OnArchiveRefreshed);
            GameEventBus.Subscribe<ItemCollectedEvent>(OnItemCollected);
            BuildTabs();
            Refresh();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<ArchiveRefreshedEvent>(OnArchiveRefreshed);
            GameEventBus.Unsubscribe<ItemCollectedEvent>(OnItemCollected);
        }

        public void Refresh()
        {
            ClearSlots();
            if (_gameState == null || _gridRoot == null || _slotTemplate == null)
            {
                return;
            }

            List<CollectibleSO> items = _gameState.GetUniqueCollectedItems();
            foreach (CollectibleSO item in items)
            {
                if (item == null || item.Category != _selectedCategory)
                {
                    continue;
                }

                GameObject slot = Instantiate(_slotTemplate, _gridRoot);
                slot.SetActive(true);
                BindSlot(slot, item);
                _slots.Add(slot);
            }

            if (_selectedItem == null || _selectedItem.Category != _selectedCategory || _gameState.CountItem(_selectedItem) <= 0)
            {
                _selectedItem = FirstItemInCategory(items, _selectedCategory);
            }

            RefreshDetails();
            RefreshCapacity();
        }

        private void BuildTabs()
        {
            if (_tabButtons.Count > 0 || _tabRoot == null || _tabButtonTemplate == null)
            {
                return;
            }

            _tabButtonTemplate.gameObject.SetActive(false);
            foreach (CollectibleCategory category in System.Enum.GetValues(typeof(CollectibleCategory)))
            {
                Button button = Instantiate(_tabButtonTemplate, _tabRoot);
                button.gameObject.SetActive(true);
                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.text = GetCategoryName(category);
                }

                CollectibleCategory captured = category;
                button.onClick.AddListener(() => SelectCategory(captured));
                _tabButtons.Add(button);
            }
        }

        private void SelectCategory(CollectibleCategory category)
        {
            _selectedCategory = category;
            _selectedItem = null;
            UIAudioPlayer.Play(UIAudioKind.Tab);
            Refresh();
        }

        private void BindSlot(GameObject slot, CollectibleSO item)
        {
            TMP_Text nameText = slot.transform.Find("NameText")?.GetComponent<TMP_Text>();
            TMP_Text countText = slot.transform.Find("CountText")?.GetComponent<TMP_Text>();
            Image iconImage = slot.transform.Find("Icon")?.GetComponent<Image>();
            Button button = slot.GetComponent<Button>();

            if (nameText != null)
            {
                nameText.text = string.IsNullOrWhiteSpace(item.DisplayName) ? item.name : item.DisplayName;
            }

            if (countText != null)
            {
                int count = _gameState != null ? _gameState.CountItem(item) : 0;
                countText.text = item.Stackable && count > 1 ? $"x{count}" : string.Empty;
            }

            if (iconImage != null)
            {
                iconImage.sprite = item.Icon;
                iconImage.enabled = item.Icon != null;
            }

            if (button != null)
            {
                button.onClick.AddListener(() => SelectItem(item));
            }
        }

        private void SelectItem(CollectibleSO item)
        {
            _selectedItem = item;
            UIAudioPlayer.Play(UIAudioKind.Select);
            RefreshDetails();
        }

        private void RefreshDetails()
        {
            bool hasItem = _selectedItem != null;
            SetText(_detailNameText, hasItem ? GetItemName(_selectedItem) : "아이템 없음");
            SetText(_detailCategoryText, hasItem ? GetCategoryName(_selectedItem.Category) : string.Empty);
            SetText(_detailDescriptionText, hasItem ? _selectedItem.Description : "이 탭에 저장된 아이템이 없습니다.");

            if (_detailIcon != null)
            {
                _detailIcon.sprite = hasItem ? _selectedItem.Icon : null;
                _detailIcon.enabled = hasItem && _selectedItem.Icon != null;
            }

            if (_removeButton != null)
            {
                _removeButton.gameObject.SetActive(hasItem && _selectedItem.CanRemove);
                _removeButton.onClick.RemoveAllListeners();
                if (hasItem && _selectedItem.CanRemove)
                {
                    _removeButton.onClick.AddListener(RemoveSelectedItem);
                }
            }
        }

        private void RemoveSelectedItem()
        {
            if (_gameState == null || _selectedItem == null)
            {
                return;
            }

            if (_gameState.RemoveItem(_selectedItem))
            {
                GameEventBus.Publish(new ArchiveRefreshedEvent());
            }
        }

        private void RefreshCapacity()
        {
            if (_gameState != null && _capacityText != null)
            {
                _capacityText.text = $"{_gameState.UsedInventorySlots}/{_gameState.InventoryCapacity}";
            }
        }

        private void ClearSlots()
        {
            foreach (GameObject slot in _slots)
            {
                if (slot != null)
                {
                    Destroy(slot);
                }
            }

            _slots.Clear();

            if (_slotTemplate != null)
            {
                _slotTemplate.SetActive(false);
            }
        }

        private void OnArchiveRefreshed(ArchiveRefreshedEvent evt)
        {
            Refresh();
        }

        private void OnItemCollected(ItemCollectedEvent evt)
        {
            Refresh();
        }

        private static CollectibleSO FirstItemInCategory(List<CollectibleSO> items, CollectibleCategory category)
        {
            foreach (CollectibleSO item in items)
            {
                if (item != null && item.Category == category)
                {
                    return item;
                }
            }

            return null;
        }

        private static string GetItemName(CollectibleSO item)
        {
            return string.IsNullOrWhiteSpace(item.DisplayName) ? item.name : item.DisplayName;
        }

        private static string GetCategoryName(CollectibleCategory category)
        {
            return category switch
            {
                CollectibleCategory.Evidence => "증거",
                CollectibleCategory.Letter => "편지",
                CollectibleCategory.Material => "재료",
                CollectibleCategory.Consumable => "소비",
                CollectibleCategory.KeyItem => "중요",
                _ => "기타"
            };
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
