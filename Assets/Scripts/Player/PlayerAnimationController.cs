using UnityEngine;

namespace DeadLetterOffice.Player
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Animator _animator;
        [SerializeField] private string _speedParameter = "Speed";
        [SerializeField] private string _movingParameter = "IsMoving";

        private void Awake()
        {
            if (_playerController == null)
            {
                _playerController = GetComponentInParent<PlayerController>();
            }

            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }

        private void Update()
        {
            if (_playerController == null || _animator == null)
            {
                return;
            }

            Vector3 velocity = _playerController.Velocity;
            float horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            _animator.SetFloat(_speedParameter, horizontalSpeed);
            _animator.SetBool(_movingParameter, _playerController.IsMoving);
        }
    }
}
