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
        [SerializeField] private float _rotationSpeed = 12f;
        [SerializeField] private float _gravity = -20f;

        private CharacterController _characterController;
        private Vector2 _moveInput;
        private bool _isSprinting;
        private float _verticalVelocity;

        public Vector3 Velocity { get; private set; }
        public bool IsMoving => new Vector2(Velocity.x, Velocity.z).sqrMagnitude > 0.01f;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
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

        public void OnSprint(InputAction.CallbackContext context)
        {
            _isSprinting = context.ReadValueAsButton();
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
                _verticalVelocity = -1f;
            }

            _verticalVelocity += _gravity * Time.deltaTime;

            float speed = _isSprinting ? _sprintSpeed : _walkSpeed;
            Vector3 horizontalVelocity = moveDirection * speed;
            Velocity = new Vector3(horizontalVelocity.x, _verticalVelocity, horizontalVelocity.z);

            _characterController.Move(Velocity * Time.deltaTime);

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
                _verticalVelocity = -1f;
            }

            _verticalVelocity += _gravity * Time.deltaTime;
            _characterController.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
        }

        private static bool CanControl()
        {
            return !ServiceLocator.TryGet(out GameModeManager modeManager) || modeManager.CanControlPlayer;
        }
    }
}
