using UnityEngine;
using UnityEngine.UI;

public class PauseBlurController : MonoBehaviour
{
    [Header("References")]
    public Camera sourceCamera;
    public RawImage blurDisplay;
    public Material blurMaterial;

    [Header("Blur Settings")]
    [Range(0.25f, 1f)] public float resolutionScale = 0.5f;
    [Range(0f, 8f)] public float blurSize = 2.5f;

    private RenderTexture captureRT;
    private RenderTexture tempRT;
    private Texture2D screenTexture;

    public void CaptureAndShowBlur()
    {
        if (sourceCamera == null || blurDisplay == null || blurMaterial == null)
            return;

        int width = Mathf.Max(2, Mathf.RoundToInt(Screen.width * resolutionScale));
        int height = Mathf.Max(2, Mathf.RoundToInt(Screen.height * resolutionScale));

        ReleaseTextures();

        captureRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        tempRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        screenTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

        RenderTexture currentActive = RenderTexture.active;
        RenderTexture currentTarget = sourceCamera.targetTexture;

        sourceCamera.targetTexture = captureRT;
        sourceCamera.Render();

        RenderTexture.active = captureRT;
        screenTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenTexture.Apply();

        sourceCamera.targetTexture = currentTarget;
        RenderTexture.active = currentActive;

        Graphics.Blit(screenTexture, tempRT);

        blurMaterial.SetFloat("_BlurSize", blurSize);

        Graphics.Blit(tempRT, captureRT, blurMaterial, 0);
        Graphics.Blit(captureRT, tempRT, blurMaterial, 1);

        blurDisplay.texture = tempRT;
        blurDisplay.material = null;
    }

    private void OnDestroy()
    {
        ReleaseTextures();
    }

    private void ReleaseTextures()
    {
        if (captureRT != null)
        {
            captureRT.Release();
            Destroy(captureRT);
            captureRT = null;
        }

        if (tempRT != null)
        {
            tempRT.Release();
            Destroy(tempRT);
            tempRT = null;
        }

        if (screenTexture != null)
        {
            Destroy(screenTexture);
            screenTexture = null;
        }
    }
}