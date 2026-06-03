using System.Collections;
using DeadLetterOffice.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeadLetterOffice.Player
{
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _targetOffset = new(0f, 1.45f, 0f);
        [SerializeField] private float _distance = 5f;
        [SerializeField] private float _minDistance = 2.2f;
        [SerializeField] private float _maxDistance = 8f;
        [SerializeField] private float _zoomSpeed = 0.015f;
        [SerializeField] private float _mouseLookSensitivity = 0.12f;
        [SerializeField] private float _followSmoothTime = 0.08f;
        [SerializeField] private float _rotationSmoothTime = 0.05f;
        [SerializeField] private float _minPitch = -12f;
        [SerializeField] private float _maxPitch = 55f;
        [SerializeField] private LayerMask _collisionMask = ~0;
        [SerializeField] private float _collisionRadius = 0.25f;

        private Vector2 _lookInput;
        private Vector3 _positionVelocity;
        private float _yaw;
        private float _pitch = 20f;
        private float _yawVelocity;
        private float _pitchVelocity;
        private Coroutine _shotRoutine;

        private void OnEnable()
        {
            GameEventBus.Subscribe<CameraShotRequestedEvent>(OnCameraShotRequested);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CameraShotRequestedEvent>(OnCameraShotRequested);
        }

        private void LateUpdate()
        {
            if (_shotRoutine != null || _target == null)
            {
                return;
            }

            if (CanControlCamera())
            {
                ApplyMouseWheelZoom();
                ApplyLookInput();
            }

            FollowTarget();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            Vector2 scroll = context.ReadValue<Vector2>();
            Zoom(scroll.y);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void Zoom(float scrollAmount)
        {
            if (Mathf.Approximately(scrollAmount, 0f))
            {
                return;
            }

            _distance = Mathf.Clamp(_distance - scrollAmount * _zoomSpeed, _minDistance, _maxDistance);
        }

        public void PlayShot(Transform shotTransform, float duration, bool lockPlayer)
        {
            if (shotTransform == null)
            {
                return;
            }

            if (_shotRoutine != null)
            {
                StopCoroutine(_shotRoutine);
            }

            _shotRoutine = StartCoroutine(ShotRoutine(shotTransform, duration, lockPlayer));
        }

        private void ApplyMouseWheelZoom()
        {
            if (Mouse.current == null)
            {
                return;
            }

            Zoom(Mouse.current.scroll.ReadValue().y);
        }

        private void ApplyLookInput()
        {
            if (_lookInput.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            _yaw += _lookInput.x * _mouseLookSensitivity;
            _pitch = Mathf.Clamp(_pitch - _lookInput.y * _mouseLookSensitivity, _minPitch, _maxPitch);
        }

        private void FollowTarget()
        {
            Quaternion targetRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 focusPoint = _target.position + _targetOffset;
            float targetDistance = GetCollisionAdjustedDistance(focusPoint, targetRotation);
            Vector3 desiredPosition = focusPoint - targetRotation * Vector3.forward * targetDistance;

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _positionVelocity, _followSmoothTime);

            float currentYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, _yaw, ref _yawVelocity, _rotationSmoothTime);
            float currentPitch = Mathf.SmoothDampAngle(NormalizeAngle(transform.eulerAngles.x), _pitch, ref _pitchVelocity, _rotationSmoothTime);
            transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        }

        private float GetCollisionAdjustedDistance(Vector3 focusPoint, Quaternion targetRotation)
        {
            Vector3 direction = -(targetRotation * Vector3.forward);
            if (Physics.SphereCast(focusPoint, _collisionRadius, direction, out RaycastHit hit, _distance, _collisionMask))
            {
                return Mathf.Max(_minDistance, hit.distance - _collisionRadius);
            }

            return _distance;
        }

        private IEnumerator ShotRoutine(Transform shotTransform, float duration, bool lockPlayer)
        {
            GameModeManager modeManager = null;
            GameMode previousMode = GameMode.Exploration;

            if (lockPlayer && ServiceLocator.TryGet(out modeManager))
            {
                previousMode = modeManager.CurrentMode;
                modeManager.SetCinematicMode();
            }

            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;
            float elapsed = 0f;
            float shotDuration = Mathf.Max(0.01f, duration);

            while (elapsed < shotDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / shotDuration);
                transform.position = Vector3.Lerp(startPosition, shotTransform.position, t);
                transform.rotation = Quaternion.Slerp(startRotation, shotTransform.rotation, t);
                yield return null;
            }

            transform.SetPositionAndRotation(shotTransform.position, shotTransform.rotation);

            if (lockPlayer && modeManager != null)
            {
                modeManager.SetMode(previousMode);
            }

            SyncOrbitFromTransform();
            _shotRoutine = null;
        }

        private void SyncOrbitFromTransform()
        {
            Vector3 euler = transform.eulerAngles;
            _yaw = euler.y;
            _pitch = NormalizeAngle(euler.x);
        }

        private static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }

        private static bool CanControlCamera()
        {
            return !ServiceLocator.TryGet(out GameModeManager modeManager) || modeManager.CanControlCamera;
        }

        private void OnCameraShotRequested(CameraShotRequestedEvent evt)
        {
            PlayShot(evt.ShotTransform, evt.Duration, evt.LockPlayer);
        }
    }
}
