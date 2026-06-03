using System.Collections;
using DeadLetterOffice.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeadLetterOffice.Scene
{
    public class SceneTransitionManager : MonoBehaviour
    {
        [SerializeField] private string _titleSceneName = "Scene_Title";
        [SerializeField] private float _fadeSeconds = 0.35f;

        private bool _isTransitioning;
        private string _currentGameplayScene;

        public string CurrentGameplayScene => _currentGameplayScene;
        public bool IsTransitioning => _isTransitioning;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<SceneTransitionManager>();
        }

        public void StartTitle()
        {
            TransitionTo(_titleSceneName);
        }

        public void TransitionTo(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("[SceneTransitionManager] Scene name is empty.");
                return;
            }

            if (_isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] Transition already in progress.");
                return;
            }

            StartCoroutine(TransitionRoutine(sceneName));
        }

        private IEnumerator TransitionRoutine(string sceneName)
        {
            _isTransitioning = true;

            if (_fadeSeconds > 0f)
            {
                yield return new WaitForSeconds(_fadeSeconds);
            }

            if (!string.IsNullOrEmpty(_currentGameplayScene) && SceneManager.GetSceneByName(_currentGameplayScene).isLoaded)
            {
                AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(_currentGameplayScene);
                while (unloadOperation != null && !unloadOperation.isDone)
                {
                    yield return null;
                }
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (loadOperation == null)
            {
                Debug.LogError($"[SceneTransitionManager] Failed to load scene: {sceneName}");
                _isTransitioning = false;
                yield break;
            }

            while (!loadOperation.isDone)
            {
                yield return null;
            }

            UnityEngine.SceneManagement.Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid())
            {
                SceneManager.SetActiveScene(loadedScene);
            }

            _currentGameplayScene = sceneName;

            if (_fadeSeconds > 0f)
            {
                yield return new WaitForSeconds(_fadeSeconds);
            }

            _isTransitioning = false;
        }
    }
}
