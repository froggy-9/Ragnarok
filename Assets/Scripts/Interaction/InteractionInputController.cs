using DeadLetterOffice.Core;
using DeadLetterOffice.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeadLetterOffice.Interaction
{
    public class InteractionInputController : MonoBehaviour
    {
        [SerializeField] private LayerMask _interactionMask = ~0;
        [SerializeField] private Transform _origin;
        [SerializeField] private float _interactionRadius = 2.2f;
        [SerializeField] private string _defaultPromptText = "조사";
        [SerializeField] private Key _interactKey = Key.F;

        private readonly Collider[] _hits = new Collider[24];
        private IInteractable _focusedInteractable;
        private string _focusedPromptText;

        private void Awake()
        {
            if (_origin == null)
            {
                _origin = transform;
            }
        }

        private void Start()
        {
            EnsurePromptUisAreListening();
        }

        private void Update()
        {
            RefreshFocus();

            if (Keyboard.current != null && Keyboard.current[_interactKey].wasPressedThisFrame)
            {
                TryInteract();
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            TryInteract();
        }

        public void OnInteract(InputValue value)
        {
            if (!value.isPressed)
            {
                return;
            }

            TryInteract();
        }

        public void TryInteract()
        {
            if (!CanUseInteraction())
            {
                return;
            }

            if (_focusedInteractable == null || !_focusedInteractable.CanInteract())
            {
                RefreshFocus(true);
                return;
            }

            _focusedInteractable.OnInteract();
            RefreshFocus(true);
        }

        private void RefreshFocus(bool forcePublish = false)
        {
            IInteractable interactable = FindNearestInteractable(out string promptText);
            bool changed = !ReferenceEquals(_focusedInteractable, interactable) || _focusedPromptText != promptText;
            _focusedInteractable = interactable;
            _focusedPromptText = promptText;

            if (changed || forcePublish)
            {
                GameEventBus.Publish(new InteractionPromptChangedEvent(_focusedInteractable != null, _focusedPromptText));
            }
        }

        private IInteractable FindNearestInteractable(out string promptText)
        {
            promptText = string.Empty;
            if (!CanUseInteraction() || _origin == null)
            {
                return null;
            }

            int hitCount = Physics.OverlapSphereNonAlloc(_origin.position, _interactionRadius, _hits, _interactionMask);
            IInteractable bestInteractable = null;
            IInteractionPromptProvider bestPromptProvider = null;
            float bestDistanceSqr = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hits[i];
                if (hit == null)
                {
                    continue;
                }

                IInteractable interactable = hit.GetComponentInParent<IInteractable>();
                if (interactable == null || !interactable.CanInteract())
                {
                    continue;
                }

                Component interactableComponent = interactable as Component;
                if (interactableComponent == null || interactableComponent.transform == transform)
                {
                    continue;
                }

                float distanceSqr = (interactableComponent.transform.position - _origin.position).sqrMagnitude;
                if (distanceSqr >= bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                bestInteractable = interactable;
                bestPromptProvider = hit.GetComponentInParent<IInteractionPromptProvider>();
            }

            if (bestInteractable == null)
            {
                return null;
            }

            promptText = bestPromptProvider != null ? bestPromptProvider.GetPromptText() : _defaultPromptText;
            return bestInteractable;
        }

        private static bool CanUseInteraction()
        {
            return !ServiceLocator.TryGet(out GameModeManager modeManager) || modeManager.CurrentMode == GameMode.Exploration;
        }

        private static void EnsurePromptUisAreListening()
        {
            InteractionPromptUI[] prompts = Object.FindObjectsByType<InteractionPromptUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (InteractionPromptUI prompt in prompts)
            {
                if (prompt != null && !prompt.gameObject.activeSelf)
                {
                    prompt.gameObject.SetActive(true);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Transform gizmoOrigin = _origin != null ? _origin : transform;
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.28f);
            Gizmos.DrawWireSphere(gizmoOrigin.position, _interactionRadius);
        }
#endif
    }
}
