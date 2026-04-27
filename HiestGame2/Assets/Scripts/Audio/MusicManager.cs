using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Crossfade")]
    public float crossfadeDuration = 1.5f;

    [Header("Audio Sources (auto-created if not assigned)")]
    public AudioSource sourceA;
    public AudioSource sourceB;

    private AudioSource activeSource;
    private AudioSource inactiveSource;
    private AudioClip currentClip;
    private Coroutine crossfadeCoroutine;
    private bool isPaused;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();

        activeSource = sourceA;
        inactiveSource = sourceB;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void EnsureAudioSources()
    {
        if (sourceA == null)
        {
            sourceA = gameObject.AddComponent<AudioSource>();
            sourceA.loop = true;
            sourceA.playOnAwake = false;
        }
        if (sourceB == null)
        {
            sourceB = gameObject.AddComponent<AudioSource>();
            sourceB.loop = true;
            sourceB.playOnAwake = false;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Look for a SceneMusicTag in the new scene to know what to play
        SceneMusicTag tag = FindFirstObjectByType<SceneMusicTag>();

        if (tag != null && tag.musicClip != null)
        {
            PlayTrack(tag.musicClip);
        }
        // If no tag in scene, keep playing whatever was already playing
    }

    public void PlayTrack(AudioClip clip)
    {
        if (clip == null) return;
        if (clip == currentClip) return; // already playing this track

        currentClip = clip;

        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);

        crossfadeCoroutine = StartCoroutine(CrossfadeRoutine(clip));
    }

    IEnumerator CrossfadeRoutine(AudioClip newClip)
    {
        // Set up the inactive source with the new clip
        inactiveSource.clip = newClip;
        inactiveSource.volume = 0f;
        inactiveSource.Play();

        float targetVolume = GetTargetVolume();
        float startVolume = activeSource.volume;
        float t = 0f;

        while (t < crossfadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / crossfadeDuration;
            activeSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            inactiveSource.volume = Mathf.Lerp(0f, targetVolume, progress);
            yield return null;
        }

        activeSource.Stop();
        activeSource.volume = 0f;

        // Swap which source is active
        AudioSource temp = activeSource;
        activeSource = inactiveSource;
        inactiveSource = temp;

        // Apply pause state if needed (in case it changed during crossfade)
        if (isPaused)
            activeSource.Pause();

        crossfadeCoroutine = null;
    }

    float GetTargetVolume()
    {
        if (SettingsManager.Instance != null)
            return SettingsManager.Instance.GetFinalMusicVolume();
        return 1f;
    }

    public void PauseMusic()
    {
        isPaused = true;
        if (activeSource != null && activeSource.isPlaying)
            activeSource.Pause();
    }

    public void ResumeMusic()
    {
        isPaused = false;
        if (activeSource != null)
            activeSource.UnPause();
    }

    public void StopMusic()
    {
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = null;
        }
        if (sourceA != null) sourceA.Stop();
        if (sourceB != null) sourceB.Stop();
        currentClip = null;
    }

    public void RefreshVolume()
    {
        if (activeSource != null && !isPaused)
            activeSource.volume = GetTargetVolume();
    }
}