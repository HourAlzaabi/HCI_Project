using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AudioBucket : MonoBehaviour
{
    public enum BucketType
    {
        Music,
        SFX
    }

    [SerializeField] private BucketType bucketType = BucketType.SFX;

    [Tooltip("Base volume before settings are applied.")]
    [Range(0f, 1f)]
    [SerializeField] private float baseVolume = 1f;

    private AudioSource audioSource;

    private static readonly List<AudioBucket> allBuckets = new List<AudioBucket>();

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (!allBuckets.Contains(this))
            allBuckets.Add(this);

        ApplyVolume();
    }

    private void OnDisable()
    {
        allBuckets.Remove(this);
    }

    public void ApplyVolume()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (SettingsManager.Instance == null)
            return;

        float finalVolume = baseVolume;

        if (bucketType == BucketType.Music)
            finalVolume *= SettingsManager.Instance.GetFinalMusicVolume();
        else
            finalVolume *= SettingsManager.Instance.GetFinalSfxVolume();

        audioSource.volume = finalVolume;
    }

    public static void ApplyAllBucketVolumes()
    {
        for (int i = 0; i < allBuckets.Count; i++)
        {
            if (allBuckets[i] != null)
                allBuckets[i].ApplyVolume();
        }
    }
}