using System;
using System.Collections;
using InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using Random = UnityEngine.Random;

namespace Core.Managers
{
    public class VideoManager : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float minTimeBetweenVideos = 5f;
        [SerializeField] private float maxTimeBetweenVideos = 10f;
        [SerializeField] private float startDelay = .1f;

        [Header("Video Components")]
        [SerializeField] private VideoPlayer videoPlayer;

        [Header("Video Clips")]
        [SerializeField] private VideoClip startClip;
        [SerializeField] private VideoClip exitClip;
        [SerializeField] private VideoClip tutorialClip;
        [SerializeField] private VideoClip[] randomClips;

        private InputSystem_Actions _inputActions;
        private float _timeSinceLastVideo;
        private float _nextVideoDelay;
        private int _lastVideoIndex;
        private bool _startClipFinished;
        private bool _exitSequenceStarted; // Flag to prevent multiple presses

        private void OnEnable()
        {
            if (_inputActions != null)
            {
                _inputActions.UI.Start.performed += PlayExitClip;
            }
        }

        private void OnDisable()
        {
            if (_inputActions != null)
            {
                _inputActions.UI.Start.performed -= PlayExitClip;
            }
        }

        private void Start()
        {
            PlayStartClip();
            _inputActions = InputSystemInstance.Instance;
            _inputActions.UI.Enable();
            // Don't enable input listener here - will be enabled after start video finishes
        }

        private void Update()
        {
            if (_startClipFinished)
            {
                _timeSinceLastVideo += Time.deltaTime;
            }

            if (_timeSinceLastVideo >= _nextVideoDelay)
            {
                PlayRandomClip();
            }
        }
        
        private IEnumerator PlayStartClipDelayed()
        {
            yield return new WaitForSeconds(startDelay);
            videoPlayer.clip = startClip;
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnStartClipFinished;
        }

        private void PlayStartClip()
        {
            StartCoroutine(PlayStartClipDelayed());
        }

        private void OnStartClipFinished(VideoPlayer vp)
        {
            videoPlayer.loopPointReached -= OnStartClipFinished;
            _startClipFinished = true;
            _timeSinceLastVideo = 0f;
            _nextVideoDelay = Random.Range(minTimeBetweenVideos, maxTimeBetweenVideos);
            
            // Enable input listener only after start video finishes
            _inputActions.UI.Start.performed += PlayExitClip;
            
            EventManager.Instance.OnFinishedStartVideo();
        }

        private void PlayRandomClip()
        {
            int index;
            do
            {
                index = Random.Range(0, randomClips.Length);
            } while (index == _lastVideoIndex && randomClips.Length > 1);

            videoPlayer.clip = randomClips[index];
            _lastVideoIndex = index;
            _timeSinceLastVideo = 0f;
            _nextVideoDelay = Random.Range(minTimeBetweenVideos, maxTimeBetweenVideos);
            videoPlayer.Play();
        }

        private void PlayExitClip(InputAction.CallbackContext context)
        {
            // Prevent multiple presses during exit→tutorial sequence
            if (_exitSequenceStarted) return;
            
            _exitSequenceStarted = true;
            _startClipFinished = false;

            // Disable input listener to prevent further presses during exit AND tutorial
            _inputActions.UI.Start.performed -= PlayExitClip;

            if (videoPlayer.isPlaying)
            {
                StartCoroutine(WaitThenPlayExitThenTutorial());
            }
            else
            {
                PlayExitBeforeTutorial();
            }
        }

        private IEnumerator WaitThenPlayExitThenTutorial()
        {
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }

            PlayExitBeforeTutorial();
        }

        private void PlayExitBeforeTutorial()
        {
            EventManager.Instance.OnExitVideoStart();
            videoPlayer.clip = exitClip;
            videoPlayer.loopPointReached += OnExitFinished;
            videoPlayer.Play();
        }

        private void OnExitFinished(VideoPlayer vp)
        {
            videoPlayer.loopPointReached -= OnExitFinished;

            videoPlayer.clip = tutorialClip;
            videoPlayer.loopPointReached += OnTutorialVideoEnded;
            videoPlayer.Play();
        }

        private void OnTutorialVideoEnded(VideoPlayer vp)
        {
            videoPlayer.loopPointReached -= OnTutorialVideoEnded;
            EventManager.Instance.OnExitVideoFinished();
        }
    }
}
