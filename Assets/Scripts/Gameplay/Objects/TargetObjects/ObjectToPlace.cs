using System;
using System.Collections;
using Core.Managers;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization; // Make sure this is at the top of your file


namespace Gameplay.Objects.TargetObjects
{
    public class ObjectToPlace : MonoBehaviour
    {
        [SerializeField] private StatueStateMachine targetStatueParent;
        [SerializeField] private SuckableObject targetObjectParent;
        [SerializeField] private GameObject targetObj;
        [SerializeField] private bool isLastObject; 
        [SerializeField] private bool isFirstObject;


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == targetStatueParent.gameObject)
            {
                // Stop any existing tweens to avoid conflict
                gameObject.transform.DOKill();

                // Animate position and rotation over 0.5 seconds
                float duration = 0.3f;
                Sequence seq = DOTween.Sequence();
                seq.Append(gameObject.transform.DOMove(targetObj.transform.position, duration).SetEase(Ease.InOutSine));
                seq.Join(gameObject.transform.DORotateQuaternion(targetObj.transform.rotation, duration)
                    .SetEase(Ease.InOutSine));

                // After the animation is complete, attach the joint
                seq.OnComplete(() =>
                {
                    FixedJoint newJoint = gameObject.AddComponent<FixedJoint>();
                    //newJoint.connectedBody = other.gameObject.GetComponent<Rigidbody>();
                    newJoint.connectedBody = targetObjectParent.Rigidbody;
                    newJoint.breakForce = Mathf.Infinity;
                    newJoint.breakTorque = Mathf.Infinity;
                    targetObjectParent.AddToJointedObjects(this.GetComponent<SuckableObject>());
                    if (!isLastObject)
                    {
                        EventManager.Instance.OnCameraNeedToMove();
                    }
                    targetStatueParent.OnCompleted();
                    if (isLastObject)
                    {
                        StartCoroutine(GameOverCoroutine());
                    }

                    if (isFirstObject) EventManager.Instance.OnOpenDoor();
                    
                });

            }
        }
        
        private IEnumerator GameOverCoroutine()
        {
            yield return new WaitForSeconds(1f);
            EventManager.Instance.OnGameOver();
        }
    }
}