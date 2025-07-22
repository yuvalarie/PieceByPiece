using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Objects
{
    [Serializable]
    public class GameObjectMaterialDict
    {
        public GameObject gameObject;
        public Material availableMaterial;
        public Material completedMaterial;
    } 
    public enum StatueState
    {
        MissingPartUnavailable,
        MissingPartAvailable,
        Completed
    }
    
    public class StatueStateMachine : MonoBehaviour
    {
        public StatueState currentState;

        [SerializeField] private GameObject[] lights;
        [SerializeField] private List<GameObjectMaterialDict> statueMaterials;
        [SerializeField] private StatueStateMachine[] nextStatues;

        private void Start()
        {
            SetState(currentState);
        }

        private void SetState(StatueState newState)
        {
            currentState = newState;
            UpdateLayer();
            UpdateLights();
            UpdateMaterials();
        }

        private void UpdateMaterials()
        {
            switch (currentState)
            {
                case StatueState.MissingPartUnavailable:
                    UpdateAvailableMaterial();
                    break;
                case StatueState.MissingPartAvailable:
                    UpdateAvailableMaterial();
                    break;
                case StatueState.Completed:
                    UpdateCompletedMaterial();
                    break;
                default:
                    Debug.LogWarning($"Unknown statue state: {currentState}");
                    break;
            }
        }
        
        private void UpdateAvailableMaterial()
        {
            foreach (var pair in statueMaterials)
            {
                if (pair.gameObject != null && pair.availableMaterial != null)
                {
                    pair.gameObject.GetComponent<Renderer>().material = pair.availableMaterial;
                }
                else
                {
                    Debug.LogWarning($"GameObject or availableMaterial is null for {pair.gameObject.name}");
                }
            }
        }
        
        private void UpdateCompletedMaterial()
        {
            foreach (var pair in statueMaterials)
            {
                if (pair.gameObject != null && pair.completedMaterial != null)
                {
                    pair.gameObject.GetComponent<Renderer>().material = pair.completedMaterial;
                }
                else
                {
                    Debug.LogWarning($"GameObject or completedMaterial is null for {pair.gameObject.name}");
                }
            }
        }

        private void UpdateLights()
        {
            switch (currentState)
            {
                case StatueState.MissingPartUnavailable:
                    UpdateNotAvailableLight();
                    break;
                case StatueState.MissingPartAvailable:
                    UpdateAvailableLight();
                    break;
                case StatueState.Completed:
                    UpdateAvailableLight();
                    break;
                default:
                    Debug.LogWarning($"Unknown statue state: {currentState}");
                    break;
            }
        }
        
        private void UpdateNotAvailableLight()
        {
            if (lights == null || lights.Length == 0) return;
            foreach (var l in lights)
            {
                if (l != null)
                {
                    l.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("NotAvailableLight is null or empty.");
                }
            }
        }
        
        private void UpdateAvailableLight()
        {
            if (lights == null || lights.Length == 0) return;
            foreach (var l in lights)
            {
                if (l != null)
                {
                    l.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("AvailableLight is null or empty.");
                }
            }
        }
        
        private void UpdateLayer()
        {
            gameObject.layer = currentState switch
            {
                StatueState.MissingPartUnavailable or StatueState.MissingPartAvailable => LayerMask.NameToLayer(
                    "Default"),
                StatueState.Completed => LayerMask.NameToLayer("SuckableItem"),
                _ => gameObject.layer
            };
            SetLayerRecursively(gameObject, gameObject.layer);
        }
        
        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        // Call this when the missing part becomes available
        private void OnPartAvailable()
        {
            if (currentState == StatueState.MissingPartUnavailable)
                SetState(StatueState.MissingPartAvailable);
        }

        // Call this when the statue is completed
        public void OnCompleted()
        {
            if (currentState == StatueState.MissingPartAvailable)
                SetState(StatueState.Completed);
            foreach (var statue in nextStatues)
            {
                Debug.Log($"Statue {statue.name} is now available.");
                statue.OnPartAvailable();
            }
        }
    }
}