using UnityEngine;

namespace DeadLetterOffice.Scene
{
    public class SceneSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _spawnId = "Default";

        public string SpawnId => _spawnId;
    }
}
