using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectRatio : MonoBehaviour
{
    [Tooltip("Target aspect ratio = width / height (e.g. 16:9 = 1.777...)")]
    public float targetAspect = 16f / 9f;

    private Camera _cam;

    void Start()
    {
        _cam = GetComponent<Camera>();
        UpdateViewport();
    }

    void Update()
    {
        if (Mathf.Abs((float)Screen.width / Screen.height - targetAspect) > 0.01f)
        {
            UpdateViewport();
        }
    }

    private void UpdateViewport()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float scaleHeight = screenAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            // Add letterbox (top/bottom bars)
            Rect rect = _cam.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            _cam.rect = rect;
        }
        else
        {
            // Add pillarbox (left/right bars)
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = _cam.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            _cam.rect = rect;
        }

        Debug.Log($"Updated camera viewport to maintain aspect ratio: {targetAspect:F2}");
    }
}