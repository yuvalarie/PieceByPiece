using System;
using Core.Managers;
using InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Gameplay.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private Transform feetTransform;
        [SerializeField] private float groundDistance;
        [SerializeField] private LayerMask groundMask;
        
        private Vector2 _moveInput;
        private CharacterController _controller;
        private InputSystem_Actions _inputActions;
        private bool _canMove;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _inputActions = InputSystemInstance.Instance;
        }
        
        private void OnEnable()
        {
            _inputActions.Player.Move.performed += OnMovePreformed;
            _inputActions.Player.Move.canceled += OnMoveCanceled;
            _inputActions.Player.Cheats.performed += OnCheatsPerformed;
            EventManager.StartFadeEnded += ChangeMoveFlag;
            EventManager.CameraNeedToMove += ChangeMoveFlag;
            EventManager.CameraFinishedMoving += ChangeMoveFlag;

        }
        
        private void OnDisable()
        {
            _inputActions.Player.Move.performed -= OnMovePreformed;
            _inputActions.Player.Move.canceled -= OnMoveCanceled;
            _inputActions.Player.Cheats.performed -= OnCheatsPerformed;
            _inputActions.Disable();
            EventManager.StartFadeEnded -= ChangeMoveFlag;
            EventManager.CameraNeedToMove -= ChangeMoveFlag;
            EventManager.CameraFinishedMoving -= ChangeMoveFlag;

        }

        private void ChangeMoveFlag()
        {
            _canMove = !_canMove;
        }

        void Update()
        {
            if (_canMove)
            {
                HandleMovement();
            }
        }

        private void HandleMovement()
        {
            if (!IsGrounded()) return;
           var move = new Vector3(_moveInput.x, 0, _moveInput.y);
           if (move != Vector3.zero)
           {
               SoundManager.Instance.PlayStep();
           }
           move = transform.TransformDirection(move);
           _controller.Move(move * (speed * Time.deltaTime));
        }

        private bool IsGrounded()
        {
            return Physics.CheckSphere(feetTransform.position, groundDistance, groundMask);
        }

        private void OnMovePreformed(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }
        
        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            _moveInput = Vector2.zero;
        }

        private void OnCheatsPerformed(InputAction.CallbackContext context)
        {
            EventManager.Instance.OnGameOver();
        }
    }
}
