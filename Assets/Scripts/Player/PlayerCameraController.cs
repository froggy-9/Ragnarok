using System.Collections;
using DeadLetterOffice.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeadLetterOffice.Player
{
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private string _targetTag = "Player";
        [SerializeField] private Vector3 _targetOffset = new(0f, 1.45f, 0f);
        [SerializeField] private float _distance = 6f;
        [SerializeField] private float _minDistance = 1.8f;
        [SerializeField] private float _maxDistance = 10f;
        [SerializeField] private float _zoomSpeed = 2.5f;
        [SerializeField] private float _minimumZoomStep = 2.5f;
        [SerializeField] private float _mouseLookSensitivity = 0.09f;
        [SerializeField] private float _followSmoothTime = 0.12f;
        [SerializeField] private float _rotationSmoothTime = 0.09f;
        [SerializeField] private float _minPitch = -12f;
        [SerializeField] private float _maxPitch = 55f;
        [SerializeField] private LayerMask _collisionMask = ~0;
        [SerializeField] private float _collisionRadius = 0.25f;
        [SerializeField] private bool _lockCursorDuringGameplay = true;
        [SerializeField] private Key _cursorUnlockKey = Key.LeftAlt;
        [SerializeField] private float _dialogueFocusPitch = 18f;
        [SerializeField] private float _dialogueFocusYawOffset = 32f;

        private Vector2 _lookInput;
        private Vector3 _positionVelocity;
        private float _yaw;
        private float _pitch = 20f;
        private float _yawVelocity;
        private float _pitchVelocity;
        private Coroutine _shotRoutine;

        private void OnValidate()
        {
            _minDistance = Mathf.Max(0.1f, _minDistance);
            _maxDistance = Mathf.Max(_minDistance + 0.1f, _maxDistance);
            _distance = Mathf.Clamp(_distance, _minDistance, _maxDistance);
            _zoomSpeed = Mathf.Max(0.01f, _zoomSpeed);
            _minimumZoomStep = Mathf.Max(0.01f, _minimumZoomStep);
        }

        private void Awake()
        {
            FindTargetIfMissing();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<CameraShotRequestedEvent>(OnCameraShotRequested);
            GameEventBus.Subscribe<DialogueRequestedEvent>(OnDialogueRequested);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CameraShotRequestedEvent>(OnCameraShotRequested);
            GameEventBus.Unsubscribe<DialogueRequestedEvent>(OnDialogueRequested);
            UnlockCursor();
        }

        private void LateUpdate()
        {
            FindTargetIfMissing();

            if (_shotRoutine != null || _target == null)
            {
                return;
            }

            if (CanControlCamera())
            {
                UpdateCursorLock();
                ApplyMouseWheelZoom();
                if (!IsCursorUnlockKeyPressed())
                {
                    ApplyLookInput();
                }
                else
                {
                    _lookInput = Vector2.zero;
                }
            }
            else
            {
                UnlockCursor();
            }

            FollowTarget();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputValue value)
        {
            _lookInput = value.Get<Vector2>();
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            Vector2 scroll = context.ReadValue<Vector2>();
            Zoom(scroll.y);
        }

        public void OnZoom(InputValue value)
        {
            Vector2 scroll = value.Get<Vector2>();
            Zoom(scroll.y);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void FindTargetIfMissing()
        {
            if (_target != null || string.IsNullOrWhiteSpace(_targetTag))
            {
                return;
            }

            GameObject targetObject = GameObject.FindGameObjectWithTag(_targetTag);
            if (targetObject != null)
            {
                _target = targetObject.transform;
            }
        }

        public void Zoom(float scrollAmount)
        {
            if (Mathf.Approximately(scrollAmount, 0f))
            {
                return;
            }

            float normalizedScroll = Mathf.Sign(scrollAmount);
            float zoomStep = Mathf.Max(_zoomSpeed, _minimumZoomStep);
            _distance = Mathf.Clamp(_distance - normalizedScroll * zoomStep, _minDistance, _maxDistance);
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
            Vector2 lookInput = _lookInput;
            if (Mouse.current != null)
            {
                lookInput += Mouse.current.delta.ReadValue();
            }

            if (lookInput.sqrMagnitude <= 0.0001f)
            {
                _lookInput = Vector2.zero;
                return;
            }

            _yaw += lookInput.x * _mouseLookSensitivity;
            _pitch = Mathf.Clamp(_pitch - lookInput.y * _mouseLookSensitivity, _minPitch, _maxPitch);
            _lookInput = Vector2.zero;
        }

        private void UpdateCursorLock()
        {
            if (!_lockCursorDuringGameplay || Keyboard.current == null)
            {
                return;
            }

            if (IsCursorUnlockKeyPressed())
            {
                UnlockCursor();
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private bool IsCursorUnlockKeyPressed()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard[_cursorUnlockKey].isPressed;
        }

        private static void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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

        private void OnDialogueRequested(DialogueRequestedEvent evt)
        {
            FocusDialogueTarget(evt.FocusTarget);
        }

        private void FocusDialogueTarget(Transform focusTarget)
        {
            FindTargetIfMissing();

            if (_target == null || focusTarget == null)
            {
                return;
            }

            Vector3 direction = focusTarget.position - _target.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            _yaw = Quaternion.LookRotation(direction.normalized).eulerAngles.y + _dialogueFocusYawOffset;
            _pitch = Mathf.Clamp(_dialogueFocusPitch, _minPitch, _maxPitch);
        }
    }
}
