using DeadLetterOffice.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeadLetterOffice.UI
{
    public class QuestObjectiveHUD : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _objectiveText;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private TMP_Text _distanceText;
        [SerializeField] private Image _markerIcon;
        [SerializeField] private Transform _player;
        [SerializeField] private Transform _target;
        [SerializeField] private string _defaultObjective = "Go to the current objective";
        [SerializeField] private int _staticDistanceMeters = 49;
        [SerializeField] private bool _showWhenObjectiveIsEmpty;

        private void Awake()
        {
            if (_root == null)
            {
                _root = gameObject;
            }

            ApplyObjective(_defaultObjective, _target, _staticDistanceMeters, string.Empty);
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
            ApplyObjective(objectiveText, _target, _staticDistanceMeters, string.Empty);
        }

        public void SetObjective(string objectiveText, Transform target)
        {
            ApplyObjective(objectiveText, target, -1, string.Empty);
        }

        public void SetObjective(string objectiveText, int distanceMeters)
        {
            ApplyObjective(objectiveText, null, distanceMeters, string.Empty);
        }

        private void OnQuestObjectiveChanged(QuestObjectiveChangedEvent evt)
        {
            ApplyObjective(evt.ObjectiveText, evt.Target, evt.StaticDistanceMeters, evt.ProgressText);
        }

        private void ApplyObjective(string objectiveText, Transform target, int staticDistanceMeters, string progressText)
        {
            _target = target;
            _staticDistanceMeters = staticDistanceMeters;

            bool hasObjective = !string.IsNullOrWhiteSpace(objectiveText);
            if (_root != null)
            {
                _root.SetActive(hasObjective || _showWhenObjectiveIsEmpty);
            }

            if (_objectiveText != null)
            {
                _objectiveText.text = hasObjective ? objectiveText : string.Empty;
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

        private void UpdateDistance()
        {
            if (_distanceText == null)
            {
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
