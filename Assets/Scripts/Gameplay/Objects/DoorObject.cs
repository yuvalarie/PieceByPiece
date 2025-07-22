using Core.Managers;
using UnityEngine;

namespace Gameplay.Objects
{
    public class DoorObject : MonoBehaviour
    {
        private Animator _animator;
        private bool _isDoorOpen = false;
        void Start()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogError("Animator component not found on DoorObject.");
            }
        }
        
        private void OnEnable()
        {
            Core.Managers.EventManager.OpenDoor += OpenDoor;
        }
        
        private void OnDisable()
        {
            Core.Managers.EventManager.OpenDoor -= OpenDoor;
        }

        public void OpenDoor()
        {
            if (_isDoorOpen) return;
            _isDoorOpen = true;
            if (_animator != null)
            {
                _animator.SetTrigger("OpenDoor");
                SoundManager.Instance.Play("doorOpen");
            }
            else
            {
                Debug.LogWarning("Animator is not set, cannot open door.");
            }
        }
    }
}
