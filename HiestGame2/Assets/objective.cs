using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class Objective : MonoBehaviour
{
    [Header("References")]
    public Image targetImage;

    [Header("Animation")]
    public float startOffsetY = 300f;
    public float slideDuration = 0.6f;
    public float visibleDuration = 3f;
    public float fadeOutDuration = 0.5f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 targetPos;
    private Vector2 startPos;
    private Coroutine currentRoutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (targetImage == null)
            targetImage = GetComponent<Image>();

        targetPos = rectTransform.anchoredPosition;
        startPos = targetPos + new Vector2(0f, startOffsetY);

        rectTransform.anchoredPosition = startPos;
        canvasGroup.alpha = 0f;

        if (targetImage != null)
            targetImage.enabled = false;
    }

    public void Play()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PlayRoutine());
    }
    void Start()
    {
        Play();
    }

    private IEnumerator PlayRoutine()
    {
        if (targetImage != null)
            targetImage.enabled = true;

        rectTransform.anchoredPosition = startPos;
        canvasGroup.alpha = 0f;

        float time = 0f;

        while (time < slideDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / slideDuration);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            canvasGroup.alpha = t;

            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(visibleDuration);

        time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeOutDuration);

            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        canvasGroup.alpha = 0f;

        if (targetImage != null)
            targetImage.enabled = false;

        currentRoutine = null;
    }
}