using UnityEngine;

public class ButtonAudioManager : MonoBehaviour
{
    public static ButtonAudioManager Instance;

    [Header("Default Click Sound")]
    public AudioClip defaultClickClip;

    [Header("Audio Source")]
    public AudioSource audioSource;

    [Range(0f, 1f)]
    public float masterVolume = 0.7f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void PlayClick(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, masterVolume * volumeMultiplier);
    }
}