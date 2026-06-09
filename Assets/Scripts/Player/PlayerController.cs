using System.Collections;
using DeadLetterOffice.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeadLetterOffice.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _walkSpeed = 3.5f;
        [SerializeField] private float _sprintSpeed = 5.5f;
        [SerializeField] private float _acceleration = 10f;
        [SerializeField] private float _deceleration = 14f;
        [SerializeField] private float _rotationSpeed = 12f;
        [SerializeField] private float _jumpHeight = 1.2f;
        [SerializeField] private float _gravity = -20f;
        [SerializeField] private float _groundedStickVelocity = -2f;
        [SerializeField] private float _maxFallSpeed = -40f;
        [SerializeField] private bool _disableRootRigidbody = true;
        [SerializeField] private float _dialogueLookAtDuration = 0.28f;

        private CharacterController _characterController;
        private Vector2 _moveInput;
        private bool _isSprinting;
        private bool _jumpRequested;
        private float _verticalVelocity;
        private Vector3 _horizontalVelocity;
        private Coroutine _lookAtRoutine;

        public Vector3 Velocity { get; private set; }
        public bool IsMoving => new Vector2(Velocity.x, Velocity.z).sqrMagnitude > 0.01f;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            DisableConflictingRigidbody();

            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DialogueRequestedEvent>(OnDialogueRequested);
            ResetMovementState();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DialogueRequestedEvent>(OnDialogueRequested);
            if (_lookAtRoutine != null)
            {
                StopCoroutine(_lookAtRoutine);
                _lookAtRoutine = null;
            }

            ResetMovementState();
        }

        private void Reset()
        {
            CharacterController controller = GetComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 45f;
            controller.skinWidth = 0.08f;
        }

        private void Update()
        {
            if (!CanControl())
            {
                Velocity = Vector3.zero;
                ApplyGravityOnly();
                return;
            }

            Move();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            _isSprinting = context.ReadValueAsButton();
        }

        public void OnSprint(InputValue value)
        {
            _isSprinting = value.isPressed;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _jumpRequested = true;
            }
        }

        public void OnJump(InputValue value)
        {
            if (value.isPressed)
            {
                _jumpRequested = true;
            }
        }

        private void Move()
        {
            Vector3 cameraForward = _cameraTransform != null ? _cameraTransform.forward : Vector3.forward;
            Vector3 cameraRight = _cameraTransform != null ? _cameraTransform.right : Vector3.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = cameraForward * _moveInput.y + cameraRight * _moveInput.x;
            if (moveDirection.sqrMagnitude > 1f)
            {
                moveDirection.Normalize();
            }

            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = _groundedStickVelocity;
            }

            if (_jumpRequested && _characterController.isGrounded)
            {
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }

            _jumpRequested = false;
            _verticalVelocity = Mathf.Max(_verticalVelocity + _gravity * Time.deltaTime, _maxFallSpeed);

            bool isSprinting = _isSprinting || IsRightMousePressed();
            float speed = isSprinting ? _sprintSpeed : _walkSpeed;
            Vector3 targetHorizontalVelocity = moveDirection * speed;
            float velocityChangeRate = moveDirection.sqrMagnitude > 0.001f ? _acceleration : _deceleration;
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetHorizontalVelocity, velocityChangeRate * Time.deltaTime);
            Velocity = new Vector3(_horizontalVelocity.x, _verticalVelocity, _horizontalVelocity.z);

            CollisionFlags flags = _characterController.Move(Velocity * Time.deltaTime);
            if ((flags & CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
            {
                _verticalVelocity = 0f;
            }

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }

        private void ApplyGravityOnly()
        {
            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = _groundedStickVelocity;
            }

            _verticalVelocity = Mathf.Max(_verticalVelocity + _gravity * Time.deltaTime, _maxFallSpeed);
            _characterController.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
        }

        private void ResetMovementState()
        {
            _moveInput = Vector2.zero;
            _isSprinting = false;
            _jumpRequested = false;
            _verticalVelocity = _groundedStickVelocity;
            _horizontalVelocity = Vector3.zero;
            Velocity = Vector3.zero;
        }

        private void OnDialogueRequested(DialogueRequestedEvent evt)
        {
            LookAt(evt.FocusTarget);
        }

        private void LookAt(Transform target)
        {
            if (target == null)
            {
                return;
            }

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            if (_lookAtRoutine != null)
            {
                StopCoroutine(_lookAtRoutine);
            }

            _lookAtRoutine = StartCoroutine(LookAtRoutine(Quaternion.LookRotation(direction.normalized)));
        }

        private IEnumerator LookAtRoutine(Quaternion targetRotation)
        {
            Quaternion startRotation = transform.rotation;
            float duration = Mathf.Max(0.01f, _dialogueLookAtDuration);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }

            transform.rotation = targetRotation;
            _lookAtRoutine = null;
        }

        private void DisableConflictingRigidbody()
        {
            if (!_disableRootRigidbody || !TryGetComponent(out Rigidbody rootRigidbody))
            {
                return;
            }

            rootRigidbody.isKinematic = true;
            rootRigidbody.useGravity = false;
        }

        private static bool IsRightMousePressed()
        {
            return Mouse.current != null && Mouse.current.rightButton.isPressed;
        }

        private static bool CanControl()
        {
            return !ServiceLocator.TryGet(out GameModeManager modeManager) || modeManager.CanControlPlayer;
        }
    }
}
