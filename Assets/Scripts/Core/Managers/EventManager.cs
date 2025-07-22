using System;
using UnityEngine;

namespace Core.Managers
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance {get; private set;}
        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        //public static event Action CalculateScore;
        public static event Action GameOver;

        public void OnGameOver()
        {
            //CalculateScore?.Invoke();
            GameOver?.Invoke();
        }
        
        public static event Action EndSequenceOver;

        public void OnEndSequenceOver()
        {
            EndSequenceOver?.Invoke();
        }
        public static event Action OpenDoor;
        
        public void OnOpenDoor()
        {
            OpenDoor?.Invoke();
        }
        public static event Action StartFadeEnded;

        public void OnStartFadeEnded()
        {
            StartFadeEnded?.Invoke();
        }
        
        public static event Action ExitVideoFinished;
        
        public void OnExitVideoFinished()
        {
            ExitVideoFinished?.Invoke();
        }
        
        public static event Action CameraNeedToMove;
        public void OnCameraNeedToMove()
        {
            CameraNeedToMove?.Invoke();
        }
        public static event Action CameraMovedToObject;
        public void OnCameraMovedToObject()
        {
            CameraMovedToObject?.Invoke();
        }
        public static event Action CameraFinishedMoving;
        public void OnCameraFinishedMoving()
        {
            CameraFinishedMoving?.Invoke();
        }

        public static event Action FinishedStartVideo;
        public void OnFinishedStartVideo()
        {
            FinishedStartVideo?.Invoke();
        }

        public static event Action ExitVideoStart;
        public void OnExitVideoStart()
        {
            ExitVideoStart?.Invoke();
        }

    }
}