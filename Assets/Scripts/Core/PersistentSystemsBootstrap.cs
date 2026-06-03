using DeadLetterOffice.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeadLetterOffice.Core
{
    public class PersistentSystemsBootstrap : MonoBehaviour
    {
        [SerializeField] private string _persistentSceneName = "Scene_Persistent";
        [SerializeField] private string _firstSceneName = "Scene_Title";

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            UnityEngine.SceneManagement.Scene persistentScene = SceneManager.GetSceneByName(_persistentSceneName);
            if (!persistentScene.isLoaded && !string.IsNullOrWhiteSpace(_persistentSceneName))
            {
                SceneManager.LoadSceneAsync(_persistentSceneName, LoadSceneMode.Additive);
            }

            if (ServiceLocator.TryGet(out SceneTransitionManager sceneTransitionManager))
            {
                sceneTransitionManager.TransitionTo(_firstSceneName);
            }
        }
    }
}
