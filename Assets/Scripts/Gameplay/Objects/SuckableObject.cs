using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Managers;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Objects
{
    public class SuckableObject : MonoBehaviour
    {
        [SerializeField] private GameObject visualTarget;
        [SerializeField] private List<SuckableObject> jointedObjects = new List<SuckableObject>();
        [SerializeField] private bool isJoint;
        private bool _needToStick = false;
    
        private const float CooldownDuration = 0.1f;
    
        private float _startDistance;
        private bool _distanceInitialized = false; 
        private bool isDisconnected = false;
        private Vector3 _originalScale;
        private Vector3 _visualTargetOriginalScale;
        private Rigidbody _rigidbody;
        private Collider _collider;
        private float _suctionCooldown;
        private FixedJoint[] _fixedJoint;
        public bool CanBeSucked => Time.time > _suctionCooldown;
        public bool IsBeingSucked { get; set; }
        public Vector3 VacuumPosition { get; set;}
        public Rigidbody Rigidbody => _rigidbody;
        public GameObject VisualTarget => visualTarget;

        private void Start()
        {
            _originalScale = transform.localScale;
            _visualTargetOriginalScale = visualTarget.transform.localScale;
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _fixedJoint = GetComponents<FixedJoint>();
        }

        private void Update()
        {
            if (IsBeingSucked)
            {
                HandleSuction();
            }
        }
    
        private void HandleSuction()
        {
            if (!_distanceInitialized)
            {
                _startDistance = Vector3.Distance(transform.position, VacuumPosition);
                _distanceInitialized = true;
            }
            // 1. Break joints of objects that have this object as their connected body
            foreach (var joint in jointedObjects.ToList())
            {
                if (joint == null) continue;
                
                // Handle the direct connection
                if (joint.TryGetComponent(out FixedJoint jointComponent))
                {
                    Destroy(jointComponent);
                    ScoreManager.Instance.BrokenJointPenalty();
                }
                if (joint._rigidbody != null && joint._rigidbody.isKinematic)
                {
                    joint._rigidbody.isKinematic = false;
                }
                //joint._rigidbody.isKinematic = false;
                
                // Recursively handle any objects connected to this joint
                MakeJointedNonKinematic(joint);
            }
            //StartCoroutine(ShakeWithPhysics());
            // 2. If this object has a joint, set its break force to 100
            foreach (var joint in _fixedJoint)
            {
                if (joint != null)
                {
                    joint.breakForce = 50;
                    Debug.Log("break force for " + gameObject.name + " set to " + joint.breakForce);
                }
            }
            //jointedObjects.Clear();
            // 3. Handle regular suction logic
            if (isJoint)
            {
                _rigidbody.isKinematic = false;
            }
            _rigidbody.isKinematic = false;
            // Scale visual based on distance
            var currentDistance = Vector3.Distance(transform.position, VacuumPosition);
            var scale = Mathf.Clamp01(currentDistance / _startDistance);
            visualTarget.transform.localScale = _visualTargetOriginalScale * scale;
            isDisconnected = true;
        }
        
        public IEnumerator ShakeWithPhysics(float duration = 0.1f, float strength = 0.08f, float frequency = 0.05f)
        {
            if (_fixedJoint == null) yield break;
            Rigidbody rb = GetComponent<Rigidbody>();
            Vector3 originalPosition = rb.position;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += frequency;
                Vector3 randomOffset = Random.insideUnitSphere * strength;
                rb.MovePosition(originalPosition + randomOffset);
                yield return new WaitForSeconds(frequency);
            }
            rb.MovePosition(originalPosition);
        }

        private static void MakeJointedNonKinematic(SuckableObject joint)
        {
            if (joint == null || !joint.enabled || !joint.isJoint) return;
            foreach (var secondaryJoint in joint.jointedObjects.ToList())
            {
                if (secondaryJoint._rigidbody != null)
                {
                    Debug.Log($"Making {secondaryJoint} non-kinematic");
                    secondaryJoint._rigidbody.isKinematic = false;
                }
                else
                {
                    Debug.LogWarning($"Rigidbody is null on {secondaryJoint.name}");
                }
                MakeJointedNonKinematic(secondaryJoint);
            }
            joint.isJoint = false;
        }
        
        private void CheckIfDisconnected()
        {
            _rigidbody.isKinematic = !isDisconnected;
            Debug.Log($"IsDisconnected: {isDisconnected}, Rigidbody isKinematic: {_rigidbody.isKinematic} for {gameObject.name}");
            foreach (var joint in jointedObjects)
            {
                if (joint != null && joint._rigidbody != null)
                {
                    joint.CheckIfDisconnected();
                }
            }
        }

        public void HandleStoppedSuction()
        {
            _distanceInitialized = false;
            transform.localScale = _originalScale;
            //wait one second before activating check if disconnected
            //StartCoroutine(WaitAndCheckDisconnected());
        }

        private IEnumerator WaitAndCheckDisconnected()
        {
            yield return new WaitForSeconds(1f);
            CheckIfDisconnected();
        }

        public void OnEmission()
        {
            _distanceInitialized = false;
            _needToStick = true;
            gameObject.SetActive(true);
            transform.rotation = Quaternion.identity;
            transform.localScale = _originalScale;
            visualTarget.SetActive(true);
            visualTarget.transform.rotation = Quaternion.identity;
            visualTarget.transform.localScale = _visualTargetOriginalScale;
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
            if (_collider != null)
            {
                _collider.enabled = true;
            }
            _suctionCooldown = Time.time + CooldownDuration;
        }

        public void AddToJointedObjects(SuckableObject suckableObject)
        {
            jointedObjects.Add(suckableObject);
        }

        public void setIsJoint(bool value)
        {
            isJoint = value;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (_needToStick && other.gameObject.layer == LayerMask.NameToLayer("SuckableItem"))
            {
                Debug.Log($"SuckableObject {gameObject.name} collided with {other.gameObject.name} and needs to stick.");
                _needToStick = false;
                // attach to the object that the suckable object is connected to in the FixedJoint
                SuckableObject suckableObject = other.gameObject.GetComponent<SuckableObject>();
                suckableObject.AddToJointedObjects(this);
                suckableObject.setIsJoint(true);
                suckableObject._rigidbody.isKinematic = true;
                
                FixedJoint joint = gameObject.AddComponent<FixedJoint>();
                _fixedJoint = new FixedJoint[1] { joint };
                joint.connectedBody = other.gameObject.GetComponent<Rigidbody>();
                joint.connectedBody = suckableObject._rigidbody;
                joint.breakForce = Mathf.Infinity;
                joint.breakTorque = Mathf.Infinity;
            }
        }
    }
}
