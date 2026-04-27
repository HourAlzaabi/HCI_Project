using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string levelSelectScene = "LevelSelect";

    [Header("UI Panels")]
    [SerializeField] private GameObject mainUI;
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private GameObject helpUI;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Singleton (prevents duplicates)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Start music if not already playing
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only allow music in MainMenu and LevelSelect
        bool allowed =
            scene.name == mainMenuScene ||
            scene.name == levelSelectScene;

        if (!allowed)
        {
            Destroy(gameObject);
        }
    }

    // ---------- SCENE ----------
    public void StartGame()
    {
        SceneManager.LoadScene(levelSelectScene);
    }

    public void BackToMainMenuScene()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    // ---------- UI ----------
    public void OpenSettings()
    {
        mainUI.SetActive(false);
        settingsUI.SetActive(true);
        helpUI.SetActive(false);
    }

    public void OpenHelp()
    {
        mainUI.SetActive(false);
        settingsUI.SetActive(false);
        helpUI.SetActive(true);
    }

    public void BackToMainUI()
    {
        mainUI.SetActive(true);
        settingsUI.SetActive(false);
        helpUI.SetActive(false);
    }
    [SerializeField] private float uiSoundVolume = 1.5f; // increase this

    public void PlayUISound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, uiSoundVolume);
        }
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}