using DeadLetterOffice.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeadLetterOffice.UI
{
    public class QuestObjectiveHUD : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _objectiveText;
        [SerializeField] private TMP_Text _locationText;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private TMP_Text _distanceText;
        [SerializeField] private Image _markerIcon;
        [SerializeField] private Transform _player;
        [SerializeField] private Transform _target;
        [SerializeField] private string _defaultObjective = "";
        [SerializeField] private int _staticDistanceMeters = 49;
        [SerializeField] private bool _showWhenObjectiveIsEmpty;

        private bool _hasObjective;

        private void Awake()
        {
            if (_root == null)
            {
                _root = gameObject;
            }

            ApplyObjective(_defaultObjective, _target, _staticDistanceMeters, string.Empty, string.Empty, string.Empty);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<QuestObjectiveChangedEvent>(OnQuestObjectiveChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<QuestObjectiveChangedEvent>(OnQuestObjectiveChanged);
        }

        private void LateUpdate()
        {
            UpdateDistance();
        }

        public void SetPlayer(Transform player)
        {
            _player = player;
            UpdateDistance();
        }

        public void SetObjective(string objectiveText)
        {
            ApplyObjective(objectiveText, _target, _staticDistanceMeters, string.Empty, string.Empty, string.Empty);
        }

        public void SetObjective(string objectiveText, Transform target)
        {
            ApplyObjective(objectiveText, target, -1, string.Empty, string.Empty, string.Empty);
        }

        public void SetObjective(string objectiveText, int distanceMeters)
        {
            ApplyObjective(objectiveText, null, distanceMeters, string.Empty, string.Empty, string.Empty);
        }

        private void OnQuestObjectiveChanged(QuestObjectiveChangedEvent evt)
        {
            ApplyObjective(evt.ObjectiveText, evt.Target, evt.StaticDistanceMeters, evt.ProgressText, evt.QuestTitle, evt.LocationText);
        }

        private void ApplyObjective(string objectiveText, Transform target, int staticDistanceMeters, string progressText, string titleText, string locationText)
        {
            _target = target;
            _staticDistanceMeters = staticDistanceMeters;

            bool hasObjective = !string.IsNullOrWhiteSpace(objectiveText);
            _hasObjective = hasObjective;
            if (_root != null)
            {
                _root.SetActive(true);
            }

            if (_titleText != null)
            {
                _titleText.text = hasObjective ? titleText : string.Empty;
            }

            if (_locationText != null)
            {
                _locationText.text = hasObjective ? locationText : string.Empty;
            }

            if (_objectiveText != null)
            {
                _objectiveText.text = hasObjective ? BuildObjectiveText(titleText, locationText, objectiveText) : string.Empty;
            }

            if (_progressText != null)
            {
                _progressText.text = hasObjective ? progressText : string.Empty;
            }

            if (_markerIcon != null)
            {
                _markerIcon.enabled = hasObjective;
            }

            UpdateDistance();
        }

        private string BuildObjectiveText(string titleText, string locationText, string objectiveText)
        {
            if (_titleText != null || _locationText != null)
            {
                return objectiveText;
            }

            if (string.IsNullOrWhiteSpace(titleText) && string.IsNullOrWhiteSpace(locationText))
            {
                return objectiveText;
            }

            if (string.IsNullOrWhiteSpace(locationText))
            {
                return $"{titleText}\n{objectiveText}";
            }

            if (string.IsNullOrWhiteSpace(titleText))
            {
                return $"{locationText}\n{objectiveText}";
            }

            return $"{titleText}\n<size=75%>{locationText}</size>\n{objectiveText}";
        }

        private void UpdateDistance()
        {
            if (_distanceText == null)
            {
                return;
            }

            if (!_hasObjective)
            {
                _distanceText.text = string.Empty;
                return;
            }

            if (_player != null && _target != null)
            {
                int meters = Mathf.Max(0, Mathf.RoundToInt(Vector3.Distance(_player.position, _target.position)));
                _distanceText.text = $"{meters}m";
                return;
            }

            _distanceText.text = _staticDistanceMeters >= 0 ? $"{_staticDistanceMeters}m" : string.Empty;
        }
    }
}
