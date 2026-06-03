using DeadLetterOffice.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeadLetterOffice.Interaction
{
    public class InteractionInputController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _interactionMask = ~0;
        [SerializeField] private float _maxDistance = 100f;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            TryInteractAtPointer();
        }

        public void TryInteractAtPointer()
        {
            if (_camera == null || Mouse.current == null)
            {
                Debug.LogWarning("[InteractionInputController] Camera or mouse is missing.");
                return;
            }

            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Ray ray = _camera.ScreenPointToRay(screenPosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _interactionMask))
            {
                return;
            }

            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null && interactable.CanInteract())
            {
                interactable.OnInteract();
            }
        }
    }
}
