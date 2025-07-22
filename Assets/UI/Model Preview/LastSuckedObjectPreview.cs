using Gameplay.VacuumCleaner;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Model_Preview
{
    public class LastSuckedObjectPreview : MonoBehaviour
    {
        [SerializeField] private VacuumCleaner vacuumCleaner;
        [SerializeField] private Transform previewSpawnPoint;
        [SerializeField] private Transform uiParent;
        [SerializeField] private float scale;
        [SerializeField] private Vector3 rotation;
    
        private GameObject _previewObject;

        private void Update()
        {
            if (vacuumCleaner.SuckedObjectsCount == 0)
            {
                if (_previewObject != null)
                {
                    Destroy(_previewObject);
                    _previewObject = null;
                }
                return;
            }
            var last = vacuumCleaner.LastSuckedObject;
            if (last != null && (_previewObject == null || _previewObject.name != last.name + "_Preview"))
            {
                if (_previewObject != null)
                    Destroy(_previewObject);

                _previewObject = Instantiate(last.VisualTarget, previewSpawnPoint.position, Quaternion.Euler(rotation), uiParent);
                _previewObject.name = last.name + "_Preview";
                _previewObject.layer = LayerMask.NameToLayer("Preview");
                _previewObject.transform.localScale = Vector3.one;
                var renderers = _previewObject.GetComponentsInChildren<Renderer>();
                Bounds bounds = new Bounds(_previewObject.transform.position, Vector3.zero);
                foreach (Renderer r in renderers)
                {
                    bounds.Encapsulate(r.bounds);
                }
                float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                float targetSize = 1f; // Standard size you want
                float scaleFactor = (maxSize > 0) ? (targetSize / maxSize) : 1f;
                _previewObject.transform.localScale = Vector3.one * scaleFactor * scale;
                _previewObject.gameObject.SetActive(true);
                SetLayerRecursively(_previewObject, LayerMask.NameToLayer("Preview"));
            }
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}