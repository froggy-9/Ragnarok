using System;
using System.Collections.Generic;
using DeadLetterOffice.Core;
using DeadLetterOffice.Letter;
using UnityEngine;

namespace DeadLetterOffice.State
{
    public class SaveManager : MonoBehaviour
    {
        private const string SAVE_KEY = "DLO_Save";

        [SerializeField] private GameStateSO _gameState;
        [SerializeField] private LetterSO[] _allLetters;
        [SerializeField] private CollectibleSO[] _allCollectibles;

        private void Awake()
        {
            if (_gameState == null)
            {
                Debug.LogError("[SaveManager] GameStateSO is required.");
                enabled = false;
                return;
            }

            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<SaveManager>();
        }

        public void Save(string currentScene, int currentChapter, int currentBeat, float playTime)
        {
            SaveData data = new()
            {
                currentScene = currentScene,
                currentChapter = currentChapter,
                currentBeat = currentBeat,
                playTime = playTime,
                saveTime = DateTime.Now.ToString("O")
            };

            foreach (FlagSO flag in _gameState.GetAllFlags())
            {
                if (flag != null)
                {
                    data.unlockedFlags.Add(new FlagSaveEntry(flag.name, flag.Value));
                }
            }

            foreach (LetterSO letter in _gameState.CollectedLetters)
            {
                if (letter != null)
                {
                    data.collectedLetterIds.Add(letter.name);
                }
            }

            foreach (CollectibleSO item in _gameState.CollectedItems)
            {
                if (item != null)
                {
                    data.collectedItemIds.Add(item.name);
                }
            }

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public bool Load(out SaveData data)
        {
            data = null;

            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                return false;
            }

            try
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                data = JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveManager] Load failed. Starting new game. {exception.Message}");
                return false;
            }

            if (data == null)
            {
                return false;
            }

            RestoreFlags(data);
            RestoreCollections(data);
            return true;
        }

        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
        }

        public void ResetGame()
        {
            DeleteSave();
            if (_gameState != null)
            {
                _gameState.ResetRuntimeState();
            }
        }

        private void RestoreFlags(SaveData data)
        {
            foreach (FlagSO flag in _gameState.GetAllFlags())
            {
                if (flag != null && data.TryGetFlagValue(flag.name, out bool savedValue))
                {
                    flag.RestoreValue(savedValue);
                }
            }
        }

        private void RestoreCollections(SaveData data)
        {
            List<LetterSO> letters = new();
            List<CollectibleSO> items = new();

            foreach (string letterId in data.collectedLetterIds)
            {
                LetterSO letter = FindByName(_allLetters, letterId);
                if (letter != null)
                {
                    letters.Add(letter);
                }
            }

            foreach (string itemId in data.collectedItemIds)
            {
                CollectibleSO item = FindByName(_allCollectibles, itemId);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            _gameState.RestoreCollections(letters, items);
        }

        private static T FindByName<T>(IEnumerable<T> assets, string assetName) where T : UnityEngine.Object
        {
            if (assets == null || string.IsNullOrEmpty(assetName))
            {
                return null;
            }

            foreach (T asset in assets)
            {
                if (asset != null && asset.name == assetName)
                {
                    return asset;
                }
            }

            return null;
        }
    }
}
