using System;
using System.Collections.Generic;
using Core.Managers;
using Gameplay.Objects;
using InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;

namespace Gameplay.VacuumCleaner
{

    public class VacuumCleaner : MonoBehaviour
    {
        [Header("Suction Properties")] [SerializeField]
        private float range;

        [SerializeField] private float suctionForce;
        [SerializeField] private float sphereRadius;
        public float SphereRadius => sphereRadius;
        private Collider _collider;

        [SerializeField] private LayerMask suckableLayers;

        [Header("Emission Properties")] [SerializeField]
        private float emitForce;

        [SerializeField] private Transform emitOrigin;
        private readonly Stack<SuckableObject> _suckedObjects = new Stack<SuckableObject>();

        [Header("MovementProperties")] [SerializeField]
        private Transform vacuumBase;

        [SerializeField] private Transform vacuumHead;
        [SerializeField] private float rotationSpeed;
        private float _headRotationHorizontal;
        private float _headRotationVertical;

        private SuckableObject _currentSuckTarget;
        private bool _hasReleasedSinceLastSuck = true;
        private InputSystem_Actions _inputActions;
        private bool _isVacuumingHeld;
        private Camera mainCamera;
        private bool _canVacuum;
        
        [Header("ParticleSystem")] 
        [SerializeField] private ParticleSystem _vacuumParticles;
        [SerializeField] private ParticleSystem _ejectParticles;

        public SuckableObject LastSuckedObject => _suckedObjects?.Count > 0 ? _suckedObjects.Peek() : null;

        public int SuckedObjectsCount => _suckedObjects.Count;

        private void Awake()
        {
            _inputActions = InputSystemInstance.Instance;
            mainCamera = Camera.main;
            _collider = GetComponent<Collider>();
            _collider.enabled = false;
        }

        private void OnEnable()
        {
            _inputActions.Player.Vaccum.performed += OnVacuumStarted;
            _inputActions.Player.Vaccum.canceled += OnVacuumCanceled;
            _inputActions.Player.Eject.performed += OnEjection;
            EventManager.StartFadeEnded += ChangeCanVacuumFlag;
            EventManager.CameraNeedToMove += ChangeCanVacuumFlag;
            EventManager.CameraFinishedMoving += ChangeCanVacuumFlag;


        }

        private void OnDisable()
        {
            _inputActions.Player.Vaccum.performed -= OnVacuumStarted;
            _inputActions.Player.Vaccum.canceled -= OnVacuumCanceled;
            _inputActions.Player.Eject.performed -= OnEjection;
            _inputActions.Disable();
            EventManager.StartFadeEnded -= ChangeCanVacuumFlag;
            EventManager.CameraNeedToMove -= ChangeCanVacuumFlag;
            EventManager.CameraFinishedMoving -= ChangeCanVacuumFlag;
        }

        private void ChangeCanVacuumFlag()
        {
            _canVacuum = !_canVacuum;
        }

        void Update()
        {
            if (_canVacuum)
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            HandleSuction();
        }

        private void OnVacuumStarted(InputAction.CallbackContext context)
        {
            SoundManager.Instance.PlayVacuumSound("start");
            _isVacuumingHeld = true;
            _vacuumParticles.Play();
            _collider.enabled = true;
        }

        private void OnVacuumCanceled(InputAction.CallbackContext context)
        {
            SoundManager.Instance.PlayVacuumSound("stop");
            _isVacuumingHeld = false;
            _vacuumParticles.Stop();
            _collider.enabled = false;
            _currentSuckTarget?.HandleStoppedSuction();
        }

        async private void OnEjection(InputAction.CallbackContext context)
        {
            _ejectParticles.Play();
            EmitSuckedObject();
            await Task.Delay(400);
            _ejectParticles.Stop();
        }

        private void HandleSuction()
        {
            if (_isVacuumingHeld)
            {
                if (_hasReleasedSinceLastSuck && _currentSuckTarget == null)
                {
                    TryStartSucking();
                    _hasReleasedSinceLastSuck = false;
                }

                if (_currentSuckTarget != null)
                {
                    ContinueSucking(_currentSuckTarget);
                }
            }
            else
            {
                if (_currentSuckTarget != null)
                {
                    _currentSuckTarget.IsBeingSucked = false;
                    _currentSuckTarget = null;
                }

                _hasReleasedSinceLastSuck = true;
            }
        }

        private void TryStartSucking()
        {
            Vector3 dir = mainCamera.transform.forward;
            Vector3 origin = mainCamera.transform.position;
            //Vector3 dir = -vacuumHead.forward;
            //Debug.DrawRay(vacuumHead.position, dir * range, Color.red, 0.1f);
            //if (Physics.SphereCast(transform.position, sphereRadius, dir, out RaycastHit hit, range, suckableLayers))
            Debug.DrawRay(origin, dir * range, Color.red, 0.1f);
            if (Physics.SphereCast(origin, sphereRadius, dir, out RaycastHit hit, range, suckableLayers))
            {
                SuckableObject suckableObject = hit.collider.GetComponent<SuckableObject>();
                if (suckableObject != null && suckableObject.CanBeSucked)
                {
                    _currentSuckTarget = suckableObject;
                    suckableObject.IsBeingSucked = true;
                    suckableObject.VacuumPosition = transform.position;
                    Debug.Log("Sucking object: " + _currentSuckTarget);
                }
                else
                {
                    Debug.Log("No SuckableObject found on the hit object");
                }
            }
        }

        private void ContinueSucking(SuckableObject target)
        {
            if (target == null) return;
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 pullDirection = (mainCamera.transform.position - rb.position).normalized;
                rb.AddForce(pullDirection * (suctionForce * Time.deltaTime), ForceMode.VelocityChange);
                Debug.DrawLine(rb.position, rb.position + pullDirection * 2, Color.cyan, 0.1f);
            }
        }

        private void EmitSuckedObject()
        {
            if (_suckedObjects.Count == 0) return;
            var obj = _suckedObjects.Pop();
            var spawnOffset = mainCamera.transform.forward * 5f;
            var position =  mainCamera.transform.position + spawnOffset;
            obj.transform.position = position;
            obj.OnEmission();
            var rb = obj.Rigidbody;
            Vector3 dir = mainCamera.transform.forward;
            rb?.AddForce(dir * emitForce, ForceMode.Impulse);
            //rb?.AddForce(-emitOrigin.forward * emitForce, ForceMode.Impulse);
            Debug.DrawRay(position, dir * 2f, Color.yellow, 1f);
            //Debug.DrawRay(position, vacuumHead.forward * 2f, Color.yellow, 1f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Suckable"))
            {
                var obj = other.GetComponent<SuckableObject>();
                var visualObj = obj?.VisualTarget;
                if (obj == null || !obj.CanBeSucked) return;
                _suckedObjects.Push(other.GetComponent<SuckableObject>());
                other.gameObject.SetActive(false);
                visualObj?.SetActive(false);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.6f);
        }
    }
}
