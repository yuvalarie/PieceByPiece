using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.VacuumCleaner
{
    public class VaccumSight : MonoBehaviour
    {
        [SerializeField] private VacuumCleaner vacuumCleaner;
        private float _sphereRadius;
        [SerializeField] private Image vacuumSightUI;
        [SerializeField] private Transform cameraTransform;
        private Camera _cam; 

        private void Start()
        {
            _sphereRadius = vacuumCleaner.SphereRadius;
            _cam = Camera.main;
        }
        
        void Update()
        {
    
            // Position in front of the camera
            Vector3 centerWorld = cameraTransform.position + cameraTransform.forward * 5f; // use cast distance
            Vector3 edgeWorld = centerWorld + cameraTransform.up * _sphereRadius;

            // Convert to screen space
            Vector3 screenCenter = _cam.WorldToScreenPoint(centerWorld);
            Vector3 screenEdge = _cam.WorldToScreenPoint(edgeWorld);

            float screenRadius = Vector3.Distance(screenCenter, screenEdge);

            // Set the UI element size (diameter = radius * 2)
            vacuumSightUI.rectTransform.sizeDelta = new Vector2(screenRadius * 2, screenRadius * 2);
        }
    }
    
    
}