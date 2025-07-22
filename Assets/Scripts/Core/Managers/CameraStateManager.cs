using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Core.Managers
{
    public class CameraStateManager : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private List<Vector3> cameraPositions = new List<Vector3>();           // World positions
        [SerializeField] private List<Quaternion> cameraRotations = new List<Quaternion>();     // World rotations (Quaternions)

        private int _cameraIndex;
        private Vector3 _currentCameraPosition;
        private Quaternion _currentCameraRotation;

        private void OnEnable()
        {
            EventManager.CameraNeedToMove += MoveCamera;
        }

        private void OnDisable()
        {
            EventManager.CameraNeedToMove -= MoveCamera;
        }

        private void MoveCamera()
        {
            _currentCameraPosition = mainCamera.transform.position;
            _currentCameraRotation = mainCamera.transform.rotation;

            if (_cameraIndex == 0)
            {
                Sequence seq = DOTween.Sequence();

                Vector3 firstPosition = cameraPositions[_cameraIndex];
                Quaternion firstRotation = cameraRotations[_cameraIndex];

                Vector3 secondPosition = cameraPositions[_cameraIndex + 1];
                Quaternion secondRotation = cameraRotations[_cameraIndex + 1];
                seq.AppendInterval(0.8f);
                seq.Append(mainCamera.transform.DOMove(firstPosition, 1.6f).SetEase(Ease.InOutSine));
                seq.Join(mainCamera.transform.DORotateQuaternion(firstRotation, 1.6f).SetEase(Ease.InOutSine));

                seq.AppendCallback(() =>
                {
                    EventManager.Instance.OnOpenDoor();
                    _cameraIndex++;
                });

                seq.AppendInterval(1f);

                seq.Append(mainCamera.transform.DOMove(secondPosition, 1.6f).SetEase(Ease.InOutSine));
                seq.Join(mainCamera.transform.DORotateQuaternion(secondRotation, 1.6f).SetEase(Ease.InOutSine));

                seq.AppendCallback(() =>
                {
                    EventManager.Instance.OnCameraMovedToObject();
                    _cameraIndex++;
                });

                seq.AppendInterval(1.6f);

                seq.Append(mainCamera.transform.DOMove(firstPosition, 1.6f).SetEase(Ease.InOutSine));
                seq.Join(mainCamera.transform.DORotateQuaternion(firstRotation, 1.6f).SetEase(Ease.InOutSine));

                seq.Append(mainCamera.transform.DOMove(_currentCameraPosition, 1.6f).SetEase(Ease.InOutSine));
                seq.Join(mainCamera.transform.DORotateQuaternion(_currentCameraRotation, 1.6f).SetEase(Ease.InOutSine));

                seq.OnComplete(() =>
                {
                    EventManager.Instance.OnCameraFinishedMoving();
                });
            }
            else
            {
                Debug.Log("camera is moving to index " +_cameraIndex);
                if (_cameraIndex >= cameraPositions.Count || _cameraIndex >= cameraRotations.Count)
                {
                    Debug.LogWarning("Camera index out of bounds.");
                    return;
                }

                Vector3 positionToReach = cameraPositions[_cameraIndex];
                Quaternion rotationToReach = cameraRotations[_cameraIndex];

                Sequence seq = DOTween.Sequence();

                seq.Append(mainCamera.transform.DOMove(positionToReach, 1.6f).SetEase(Ease.InOutSine));
                seq.Join(mainCamera.transform.DORotateQuaternion(rotationToReach, 1.6f).SetEase(Ease.InOutSine));

                seq.AppendCallback(() =>
                {
                    EventManager.Instance.OnCameraMovedToObject();
                    _cameraIndex++;
                });

                seq.AppendInterval(2f);

                seq.Append(mainCamera.transform.DOMove(_currentCameraPosition, 1.6f).SetEase(Ease.InOutSine));
                seq.Join(mainCamera.transform.DORotateQuaternion(_currentCameraRotation, 1.6f).SetEase(Ease.InOutSine));

                seq.OnComplete(() =>
                {
                    EventManager.Instance.OnCameraFinishedMoving();
                });
            }
        }
    }
}


