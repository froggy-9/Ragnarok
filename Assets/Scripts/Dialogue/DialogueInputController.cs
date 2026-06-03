using UnityEngine;
using UnityEngine.InputSystem;

namespace DeadLetterOffice.Dialogue
{
    public class DialogueInputController : MonoBehaviour
    {
        [SerializeField] private DialogueManager _dialogueManager;

        public void OnAdvance(InputAction.CallbackContext context)
        {
            if (!context.performed || _dialogueManager == null)
            {
                return;
            }

            _dialogueManager.SkipOrAdvance();
        }
    }
}
