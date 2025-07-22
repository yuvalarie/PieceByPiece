using System;
using Core.Managers;
using InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gameplay.UI
{
    [RequireComponent(typeof(RawImage))]
    public class RawImageTransparencyLooper : MonoBehaviour
    {
        [SerializeField] private RawImage rawImage;
        [SerializeField] private float loopDuration = 2f; // Duration for full fade in and out (x seconds)
        private bool _startFading;
        private float _startTime;
        

        void Update()
        {
            if (_startFading)
            {
                if (loopDuration <= 0f) return;

                // Calculate elapsed time since fading started
                float elapsedTime = Time.time - _startTime;
                
                // Time loop from 0 to 1 to 0 (ping-pong pattern) - now always starts at 0
                float alpha = Mathf.PingPong(elapsedTime / (loopDuration / 2f), 1f);

                Color color = rawImage.color;
                color.a = alpha;
                rawImage.color = color;
            }
        }

        private void OnEnable()
        {
            EventManager.FinishedStartVideo += StartFading;
            EventManager.ExitVideoStart += DestroyRawImage;
        }

        private void OnDisable()
        {
            EventManager.FinishedStartVideo -= StartFading;
            EventManager.ExitVideoStart -= DestroyRawImage;
        }

        private void StartFading()
        {
            _startFading = true;
            _startTime = Time.time; // Store when fading begins
        }

        private void DestroyRawImage()
        {
            Destroy(gameObject);
        }
    }
}