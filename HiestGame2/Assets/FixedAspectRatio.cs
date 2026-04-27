using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class FixedAspectRatio : MonoBehaviour
{
    // Target aspect, e.g. 16:9
    public float targetAspect = 16f / 9f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateViewport();
    }

    void OnValidate()
    {
        if (cam == null) cam = GetComponent<Camera>();
        UpdateViewport();
    }

    void Update()
    {
        // In editor / resize, keep updating
        UpdateViewport();
    }

    void UpdateViewport()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            // Letterbox (black bars top/bottom)
            Rect rect = new Rect
            {
                width = 1f,
                height = scaleHeight,
                x = 0f,
                y = (1f - scaleHeight) * 0.5f
            };
            cam.rect = rect;
        }
        else
        {
            // Pillarbox (black bars left/right)
            float scaleWidth = 1f / scaleHeight;

            Rect rect = new Rect
            {
                width = scaleWidth,
                height = 1f,
                x = (1f - scaleWidth) * 0.5f,
                y = 0f
            };
            cam.rect = rect;
        }
    }
}