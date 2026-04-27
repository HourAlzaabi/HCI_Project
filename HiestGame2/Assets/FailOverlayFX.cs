using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FailOverlayFX : MonoBehaviour
{
    public Image redOverlay;
    public Image blueOverlay;

    [Header("Animation")]
    public float fadeDuration = 0.6f;
    public float maxRedAlpha = 0.35f;
    public float maxBlueAlpha = 0.25f;
    public bool pulse = true;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.08f;

    private bool isRunning = false;

    private void Awake()
    {
        SetAlpha(redOverlay, 0f);
        SetAlpha(blueOverlay, 0f);
    }

    private void Update()
    {
        if (!isRunning || !pulse) return;

        float redA = maxRedAlpha + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
        float blueA = maxBlueAlpha + Mathf.Sin(Time.unscaledTime * pulseSpeed + 0.8f) * pulseAmount;

        SetAlpha(redOverlay, Mathf.Clamp01(redA));
        SetAlpha(blueOverlay, Mathf.Clamp01(blueA));
    }

    public void PlayFailOverlay()
    {
        StopAllCoroutines();
        StartCoroutine(FadeInOverlays());
    }

    private IEnumerator FadeInOverlays()
    {
        isRunning = false;

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);

            SetAlpha(redOverlay, Mathf.Lerp(0f, maxRedAlpha, t));
            SetAlpha(blueOverlay, Mathf.Lerp(0f, maxBlueAlpha, t));

            yield return null;
        }

        SetAlpha(redOverlay, maxRedAlpha);
        SetAlpha(blueOverlay, maxBlueAlpha);

        isRunning = true;
    }

    private void SetAlpha(Image image, float alpha)
    {
        if (image == null) return;

        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}