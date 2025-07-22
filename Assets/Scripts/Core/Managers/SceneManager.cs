using InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Managers
{
    public class SceneManager : MonoBehaviour
    {
        private InputSystem_Actions _inputActions;

        private SceneManager _instance;

        private void Awake()
        {
            _inputActions = InputSystemInstance.Instance;
            _inputActions.UI.Enable();
            if (_inputActions == null)
            {
                Debug.LogError("InputSystem_Actions instance is not initialized.");
            }
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void OnEnable()
        {
            EventManager.EndSequenceOver+= LoadEndScene;
            EventManager.ExitVideoFinished += OnStartPerformed;
            _inputActions.UI.StartAgain.performed += OnRestartPerformed;
        }
        
        private void OnDisable()
        {
            EventManager.EndSequenceOver -= LoadEndScene;
            EventManager.ExitVideoFinished -= OnStartPerformed;
            _inputActions.UI.StartAgain.performed -= OnRestartPerformed;
            _inputActions.UI.Disable();
        }

        private void LoadEndScene()
        {
            Debug.Log("Game Over! Loading End Scene.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("EndScene");
            _inputActions.UI.Enable();
        }
        
        private void OnRestartPerformed(InputAction.CallbackContext context)
        {
            RestartGame();
        }
        
        public void RestartGame()
        {
            Debug.Log("Restarting Game! Loading Opening Scene.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("OpeningScene");
        }
        
        private void OnStartPerformed()
        {
            StartGame();
            _inputActions.UI.Disable();
        }
        
        public void StartGame()
        {
            Debug.Log("Starting Game! Loading Game Scene.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
}