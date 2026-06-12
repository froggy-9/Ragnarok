using UnityEngine;
using UnityEngine.InputSystem;

namespace DeadLetterOffice.Dialogue
{
    public class DialogueInputController : MonoBehaviour
    {
        [SerializeField] private DialogueManager _dialogueManager;
        [SerializeField] private Key _keyboardAdvanceKey = Key.Space;
        [SerializeField] private Key _alternateAdvanceKey = Key.Enter;
        [SerializeField] private bool _advanceWithLeftMouse = true;

        private int _lastAdvanceFrame = -1;

        private void Update()
        {
            if (_dialogueManager == null || !_dialogueManager.IsRunning)
            {
                return;
            }

            if (WasKeyboardAdvancePressed() || WasMouseAdvancePressed())
            {
                TryAdvanceOncePerFrame();
            }
        }

        public void OnAdvance(InputAction.CallbackContext context)
        {
            if (!context.performed || _dialogueManager == null)
            {
                return;
            }

            TryAdvanceOncePerFrame();
        }

        public void OnAdvance(InputValue value)
        {
            if (!value.isPressed || _dialogueManager == null)
            {
                return;
            }

            TryAdvanceOncePerFrame();
        }

        private void TryAdvanceOncePerFrame()
        {
            if (_lastAdvanceFrame == Time.frameCount)
            {
                return;
            }

            _lastAdvanceFrame = Time.frameCount;
            _dialogueManager.SkipOrAdvance();
        }

        private bool WasKeyboardAdvancePressed()
        {
            return Keyboard.current != null
                && (Keyboard.current[_keyboardAdvanceKey].wasPressedThisFrame
                    || Keyboard.current[_alternateAdvanceKey].wasPressedThisFrame);
        }

        private bool WasMouseAdvancePressed()
        {
            return _advanceWithLeftMouse
                && Mouse.current != null
                && Mouse.current.leftButton.wasPressedThisFrame;
        }
    }
}
