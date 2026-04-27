using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public enum Language
    {
        English,
        Arabic,
        Spanish,
        French
    }

    public enum TextSize
    {
        Small,
        Medium,
        Large
    }

    [Header("Saved Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    public Language currentLanguage = Language.English;
    public TextSize currentTextSize = TextSize.Medium;

    [Header("Optional UI References")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private TMP_Dropdown textSizeDropdown;

    public static event Action OnSettingsChanged;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const string LanguageKey = "Language";
    private const string TextSizeKey = "TextSize";

    private bool isLoadingUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplyAllSettings();
    }

    private void Start()
    {
        HookupUI();
        LoadSettingsIntoUI();
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
        ApplyAllSettings();
        HookupUI();
        LoadSettingsIntoUI();
    }

    public void HookupUI()
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        if (languageDropdown != null)
        {
            languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        if (textSizeDropdown != null)
        {
            textSizeDropdown.onValueChanged.RemoveListener(OnTextSizeChanged);
            textSizeDropdown.onValueChanged.AddListener(OnTextSizeChanged);
        }
    }

    public void LoadSettingsIntoUI()
    {
        isLoadingUI = true;

        if (masterSlider != null)
            masterSlider.value = masterVolume;

        if (musicSlider != null)
            musicSlider.value = musicVolume;

        if (sfxSlider != null)
            sfxSlider.value = sfxVolume;

        if (languageDropdown != null)
        {
            languageDropdown.value = (int)currentLanguage;
            languageDropdown.RefreshShownValue();
        }

        if (textSizeDropdown != null)
        {
            textSizeDropdown.value = (int)currentTextSize;
            textSizeDropdown.RefreshShownValue();
        }

        isLoadingUI = false;
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        SaveSettings();
        ApplyAllSettings();
        Debug.Log($"[Settings] Master volume now: {masterVolume}, AudioListener.volume: {AudioListener.volume}");
    }

    public void SetLanguage(int languageIndex)
    {
        currentLanguage = (Language)languageIndex;
        SaveSettings();
        NotifyChanged();
        Debug.Log($"[Settings] Language now: {currentLanguage}");
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        SaveSettings();
        ApplyAllSettings();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        SaveSettings();
        ApplyAllSettings();
    }

    

    public void SetTextSize(int sizeIndex)
    {
        currentTextSize = (TextSize)sizeIndex;
        SaveSettings();
        NotifyChanged();
    }

    public float GetFinalMusicVolume()
    {
        return masterVolume * musicVolume;
    }

    public float GetFinalSfxVolume()
    {
        return masterVolume * sfxVolume;
    }

    public void ApplyAllSettings()
    {
        AudioBucket.ApplyAllBucketVolumes();
        NotifyChanged();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.SetInt(LanguageKey, (int)currentLanguage);
        PlayerPrefs.SetInt(TextSizeKey, (int)currentTextSize);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        currentLanguage = (Language)PlayerPrefs.GetInt(LanguageKey, 0);
        currentTextSize = (TextSize)PlayerPrefs.GetInt(TextSizeKey, 1);
    }

    private void NotifyChanged()
    {
        OnSettingsChanged?.Invoke();
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (isLoadingUI) return;
        SetMasterVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (isLoadingUI) return;
        SetMusicVolume(value);
    }

    private void OnSfxVolumeChanged(float value)
    {
        if (isLoadingUI) return;
        SetSfxVolume(value);
    }

    private void OnLanguageChanged(int index)
    {
        if (isLoadingUI) return;
        SetLanguage(index);
    }

    private void OnTextSizeChanged(int index)
    {
        if (isLoadingUI) return;
        SetTextSize(index);
    }
}