using UnityEngine;

namespace DeadLetterOffice.UI
{
    public class UnlockableHudButton : MonoBehaviour
    {
        [SerializeField] private bool _unlocked;

        private void Awake()
        {
            ApplyVisibility();
        }

        public void Unlock()
        {
            _unlocked = true;
            ApplyVisibility();
        }

        public void Lock()
        {
            _unlocked = false;
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            gameObject.SetActive(_unlocked);
        }
    }
}
