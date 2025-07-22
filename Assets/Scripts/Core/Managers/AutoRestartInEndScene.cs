using System.Collections;
using UnityEngine;

namespace Core.Managers
{
    public class AutoRestartInEndScene : MonoBehaviour
    {
        [SerializeField] private float restartDelay = 7f;

        private void Start()
        {
            StartCoroutine(RestartAfterDelay());
        }

        private IEnumerator RestartAfterDelay()
        {
            yield return new WaitForSeconds(restartDelay);
            
            // Find the SceneManager in the scene (or globally)
            SceneManager sceneManager = FindObjectOfType<SceneManager>();
            if (sceneManager != null)
            {
                Debug.Log("Auto-restarting game from End Scene...");
                sceneManager.RestartGame();
            }
            else
            {
                Debug.LogError("SceneManager not found in End Scene.");
            }
        }
    }
}