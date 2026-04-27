using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [Header("References")]
    public GameObject transitionRoot;
    public Image transitionImage;
    public Material circleTransitionMaterial;

    [Header("Timing")]
    public float closeDuration = 0.4f;
    public float openDuration = 0.6f;

    [Header("Radius")]
    public float closedRadius = 0f;
    public float openRadius = 1.8f;

    [Header("Scene Names")]
    public string levelSelectSceneName = "LevelSelect";

    private const string RadiusProperty = "_Radius";

    private Material runtimeMaterial;
    private bool isLoadingScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (transitionRoot == null && transitionImage != null)
            transitionRoot = transitionImage.gameObject;

        if (transitionImage != null && circleTransitionMaterial != null)
        {
            runtimeMaterial = new Material(circleTransitionMaterial);
            transitionImage.material = runtimeMaterial;
            runtimeMaterial.SetFloat(RadiusProperty, openRadius);
        }

        if (transitionRoot != null)
            transitionRoot.SetActive(false);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (ShouldPlayOpenForScene(currentScene.name))
        {
            if (transitionRoot != null)
                transitionRoot.SetActive(true);

            if (runtimeMaterial != null)
                runtimeMaterial.SetFloat(RadiusProperty, closedRadius);

            StartCoroutine(OpenTransition());
        }
        else
        {
            if (transitionRoot != null)
                transitionRoot.SetActive(false);

            if (runtimeMaterial != null)
                runtimeMaterial.SetFloat(RadiusProperty, openRadius);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();

        if (!ShouldPlayOpenForScene(scene.name))
        {
            if (transitionRoot != null)
                transitionRoot.SetActive(false);

            if (runtimeMaterial != null)
                runtimeMaterial.SetFloat(RadiusProperty, openRadius);

            isLoadingScene = false;
            return;
        }

        if (transitionRoot != null)
            transitionRoot.SetActive(true);

        if (runtimeMaterial != null)
            runtimeMaterial.SetFloat(RadiusProperty, closedRadius);

        StartCoroutine(OpenTransition());
    }

    private bool ShouldPlayOpenForScene(string sceneName)
    {
        return sceneName != levelSelectSceneName;
    }

    public void LoadSceneWithTransition(string sceneName)
    {
        if (isLoadingScene) return;
        StartCoroutine(TransitionToScene(sceneName));
    }

    public void LoadSceneWithTransition(int buildIndex)
    {
        if (isLoadingScene) return;
        StartCoroutine(TransitionToScene(buildIndex));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        isLoadingScene = true;
        yield return StartCoroutine(CloseTransition());
        yield return SceneManager.LoadSceneAsync(sceneName);
    }

    private IEnumerator TransitionToScene(int buildIndex)
    {
        isLoadingScene = true;
        yield return StartCoroutine(CloseTransition());
        yield return SceneManager.LoadSceneAsync(buildIndex);
    }

    private IEnumerator CloseTransition()
    {
        if (runtimeMaterial == null) yield break;

        if (transitionRoot != null)
            transitionRoot.SetActive(true);

        float time = 0f;
        runtimeMaterial.SetFloat(RadiusProperty, openRadius);

        while (time < closeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / closeDuration);
            t = EaseInOutCubic(t);

            float radius = Mathf.Lerp(openRadius, closedRadius, t);
            runtimeMaterial.SetFloat(RadiusProperty, radius);

            yield return null;
        }

        runtimeMaterial.SetFloat(RadiusProperty, closedRadius);
    }

    private IEnumerator OpenTransition()
    {
        if (runtimeMaterial == null) yield break;

        if (transitionRoot != null)
            transitionRoot.SetActive(true);

        float time = 0f;
        runtimeMaterial.SetFloat(RadiusProperty, closedRadius);

        while (time < openDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / openDuration);
            t = EaseInOutCubic(t);

            float radius = Mathf.Lerp(closedRadius, openRadius, t);
            runtimeMaterial.SetFloat(RadiusProperty, radius);

            yield return null;
        }

        runtimeMaterial.SetFloat(RadiusProperty, openRadius);

        if (transitionRoot != null)
            transitionRoot.SetActive(false);

        isLoadingScene = false;
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f
            ? 4f * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
}