using Unity.VisualScripting;
using UnityEngine;

namespace InputSystem
{
    public class InputSystemInstance : MonoBehaviour
    {
        private void Awake()
        {
            Instance = new InputSystem_Actions();
            Instance.Enable();
        }

        public static InputSystem_Actions Instance { get; private set; }
    }
}