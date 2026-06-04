using UnityEngine;
using UnityEngine.UI;

namespace DeadLetterOffice.UI
{
    public class MiniMapUI : MonoBehaviour
    {
        [SerializeField] private RawImage _renderedMap;
        [SerializeField] private Image _imageMap;
        [SerializeField] private RectTransform _mapArea;
        [SerializeField] private RectTransform _playerMarker;
        [SerializeField] private Transform _player;
        [SerializeField] private Texture _mapTexture;
        [SerializeField] private Sprite _fallbackMapSprite;
        [SerializeField] private Vector2 _worldMin = new(-50f, -50f);
        [SerializeField] private Vector2 _worldMax = new(50f, 50f);

        private void Awake()
        {
            ApplyMapSource();
        }

        private void LateUpdate()
        {
            UpdatePlayerMarker();
        }

        public void SetPlayer(Transform player)
        {
            _player = player;
            UpdatePlayerMarker();
        }

        public void SetRenderedMap(Texture texture)
        {
            _mapTexture = texture;
            ApplyMapSource();
        }

        public void SetImageMap(Sprite sprite)
        {
            _fallbackMapSprite = sprite;
            ApplyMapSource();
        }

        public void SetWorldBounds(Vector2 worldMin, Vector2 worldMax)
        {
            _worldMin = worldMin;
            _worldMax = worldMax;
            UpdatePlayerMarker();
        }

        private void ApplyMapSource()
        {
            if (_renderedMap != null)
            {
                _renderedMap.texture = _mapTexture;
                _renderedMap.gameObject.SetActive(_mapTexture != null);
            }

            if (_imageMap != null)
            {
                _imageMap.sprite = _fallbackMapSprite;
                _imageMap.gameObject.SetActive(_mapTexture == null);
            }
        }

        private void UpdatePlayerMarker()
        {
            if (_player == null || _playerMarker == null || _mapArea == null)
            {
                return;
            }

            float normalizedX = Mathf.InverseLerp(_worldMin.x, _worldMax.x, _player.position.x);
            float normalizedY = Mathf.InverseLerp(_worldMin.y, _worldMax.y, _player.position.z);

            Rect rect = _mapArea.rect;
            _playerMarker.anchoredPosition = new Vector2(
                Mathf.Clamp01(normalizedX) * rect.width - rect.width * 0.5f,
                Mathf.Clamp01(normalizedY) * rect.height - rect.height * 0.5f);
            _playerMarker.gameObject.SetActive(true);
        }
    }
}
