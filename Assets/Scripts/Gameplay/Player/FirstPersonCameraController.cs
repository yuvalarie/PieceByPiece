using System;
using Core.Managers;
using InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Player
{
    public class FirstPersonCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerBody;

        [Header("Movement Settings")]
        [SerializeField] private float maxSpeed = 200f;
        [SerializeField] private float dampingTime = 0.1f;

        private InputSystem_Actions _inputActions;
        private Vector2 _lookInput;
        private Vector2 _currentVelocity = Vector2.zero;
        private Vector2 _velocityDampRef = Vector2.zero;

        private float _xRotation = 0f;
        private bool _canMove = false;

        private void Awake()
        {
            _inputActions = InputSystemInstance.Instance;
            _inputActions.UI.Disable();
        }

        private void OnEnable()
        {
            _inputActions.Player.Look.performed += OnLookPerformed;
            _inputActions.Player.Look.canceled += OnLookCanceled;
            EventManager.StartFadeEnded += ChangeMoveFlag;
            EventManager.CameraNeedToMove += ChangeMoveFlag;
            EventManager.CameraFinishedMoving += ChangeMoveFlag;
        }

        private void OnDisable()
        {
            _inputActions.Player.Look.performed -= OnLookPerformed;
            _inputActions.Player.Look.canceled -= OnLookCanceled;
            _inputActions.Disable();
            EventManager.StartFadeEnded -= ChangeMoveFlag;
            EventManager.CameraNeedToMove -= ChangeMoveFlag;
            EventManager.CameraFinishedMoving -= ChangeMoveFlag;
        }

        private void Update()
        {
            if (_canMove)
            {
                HandleMovement();
            }
        }

        private void HandleMovement()
        {
            Vector2 targetVelocity = new Vector2(_lookInput.x / (1+Math.Abs(_lookInput.x)), _lookInput.y / (1+Math.Abs(_lookInput.y))) * maxSpeed;

            // Smoothly damp toward target velocity
            _currentVelocity = Vector2.SmoothDamp(_currentVelocity, targetVelocity, ref _velocityDampRef, dampingTime);

            float lookX = _currentVelocity.x * Time.deltaTime;
            float lookY = _currentVelocity.y * Time.deltaTime;

            _xRotation -= lookY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * lookX);
        }

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
        }

        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            _lookInput = Vector2.zero;
        }

        private void ChangeMoveFlag()
        {
            _canMove = !_canMove;
        }
    }
}
