using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace DeadLetterOffice.UI
{
    public static class EventSystemInputModuleFixer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ReplaceLegacyModules()
        {
            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            foreach (EventSystem eventSystem in eventSystems)
            {
                if (eventSystem == null)
                {
                    continue;
                }

                GameObject target = eventSystem.gameObject;
                BaseInputModule[] inputModules = target.GetComponents<BaseInputModule>();
                foreach (BaseInputModule inputModule in inputModules)
                {
                    if (inputModule != null && inputModule is not InputSystemUIInputModule)
                    {
                        Object.Destroy(inputModule);
                    }
                }

                if (target.GetComponent<InputSystemUIInputModule>() == null)
                {
                    target.AddComponent<InputSystemUIInputModule>();
                }
            }
        }
    }
}
