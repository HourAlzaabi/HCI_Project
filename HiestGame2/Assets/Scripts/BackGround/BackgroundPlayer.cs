using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class BackgroundPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    [Header("Enter - plays once on scene start")]
    public VideoClip enterClip;

    [Header("Loop - plays after enter or immediately")]
    public VideoClip loopClip;

    [Header("Exit - plays when leaving scene")]
    public VideoClip exitClip;

    void Start()
    {
        Debug.Log("BackgroundPlayer Start | videoPlayer: " + (videoPlayer != null)
            + " | loopClip: " + (loopClip != null)
            + " | enterClip: " + (enterClip != null));

        // Prepare first clip immediately on start
        VideoClip firstClip = enterClip != null ? enterClip : loopClip;
        if (firstClip != null)
        {
            videoPlayer.clip = firstClip;
            videoPlayer.Prepare();
            Debug.Log("Preparing: " + firstClip.name);
        }
        
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        if (enterClip != null)
        {
            yield return StartCoroutine(PlayOnce(enterClip));
        }

        if (loopClip != null)
        {
            yield return StartCoroutine(PrepareAndLoop(loopClip));
        }
    }

    IEnumerator PrepareAndLoop(VideoClip clip)
    {
        videoPlayer.clip = clip;
        videoPlayer.isLooping = true;
        videoPlayer.Prepare();

        // Wait for prepare with timeout
        float timeout = 5f;
        float elapsed = 0f;
        while (!videoPlayer.isPrepared && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared)
            Debug.LogWarning("Video prepare timed out: " + clip.name);

        videoPlayer.Play();
        Debug.Log("Now looping: " + clip.name);
    }

    public void PlayExitThen(System.Action onDone)
    {
        StartCoroutine(PlayExitCoroutine(onDone));
    }

    IEnumerator PlayExitCoroutine(System.Action onDone)
    {
        if (exitClip != null)
            yield return StartCoroutine(PlayOnce(exitClip));

        onDone?.Invoke();
    }

    IEnumerator PlayOnce(VideoClip clip)
    {
        if (clip == null) yield break;

        videoPlayer.clip = clip;
        videoPlayer.isLooping = false;
        videoPlayer.Prepare();

        // Wait for prepare with timeout
        float timeout = 5f;
        float elapsed = 0f;
        while (!videoPlayer.isPrepared && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        videoPlayer.Play();
        Debug.Log("Playing once: " + clip.name);

        // Wait for finish with timeout
        float duration = (float)clip.length + 1f;
        elapsed = 0f;
        while (videoPlayer.isPlaying && elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Finished: " + clip.name);
    }
}