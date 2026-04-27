using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance;

    public VideoPlayer videoPlayer;

    [Header("Main Menu")]
    public VideoClip mainMenuEnter;
    public VideoClip mainMenuLoop;
    public VideoClip mainMenuExit;

    [Header("Session Creation (Host/Join)")]
    public VideoClip sessionEnter;
    public VideoClip sessionLoop;
    public VideoClip sessionExit;

    [Header("Lobby")]
    public VideoClip lobbyEnter;
    public VideoClip lobbyLoop;
    public VideoClip lobbyExit;

    [Header("Level Select")]
    public VideoClip levelSelectEnter;
    public VideoClip levelSelectLoop;
    public VideoClip levelSelectExit;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Listen for scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Auto plays correct video when any scene loads
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenu":
                GoToMainMenu();
                break;
            case "SessionCreation":
                GoToSessionCreation();
                break;
            case "Lobby":
                GoToLobby();
                break;
            case "LevelSelect":
                GoToLevelSelect();
                break;
            default:
                // Level scenes - stop background video
                StopBackground();
                break;
        }
    }

    //  Core transition engine 

    public void Transition(
        VideoClip exitClip,
        VideoClip enterClip,
        VideoClip loopClip,
        System.Action onEnterComplete = null)
    {
        StartCoroutine(DoTransition(
            exitClip, enterClip, loopClip, onEnterComplete));
    }

    IEnumerator DoTransition(
        VideoClip exitClip,
        VideoClip enterClip,
        VideoClip loopClip,
        System.Action onEnterComplete)
    {
        if (exitClip != null)
            yield return StartCoroutine(PlayOnce(exitClip));

        if (enterClip != null)
            yield return StartCoroutine(PlayOnce(enterClip));

        onEnterComplete?.Invoke();

        if (loopClip != null)
            PlayLoop(loopClip);
    }

    IEnumerator PlayOnce(VideoClip clip)
    {
        if (clip == null) yield break;

        videoPlayer.clip = clip;
        videoPlayer.isLooping = false;
        videoPlayer.Play();

        // Wait for prepare
        yield return new WaitUntil(() => videoPlayer.isPrepared);

        // Wait for finish
        yield return new WaitUntil(() =>
            !videoPlayer.isPlaying ||
            videoPlayer.frame >= (long)videoPlayer.frameCount - 2);
    }

    void PlayLoop(VideoClip clip)
    {
        if (clip == null) return;
        videoPlayer.clip = clip;
        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }

    public void StopBackground()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();
    }

    //  Convenience methods 

    public void GoToMainMenu(System.Action onReady = null)
    {
        Transition(null, mainMenuEnter, mainMenuLoop, onReady);
    }

    public void GoToSessionCreation(System.Action onReady = null)
    {
        Transition(null, sessionEnter, sessionLoop, onReady);
    }

    public void GoToLobby(System.Action onReady = null)
    {
        Transition(null, lobbyEnter, lobbyLoop, onReady);
    }

    public void GoToLevelSelect(System.Action onReady = null)
    {
        Transition(null, levelSelectEnter, levelSelectLoop, onReady);
    }

    //  Transition WITH exit animation 
    // Call these from your scene scripts before loading next scene

    public void TransitionToSessionCreation(System.Action onReady = null)
    {
        Transition(mainMenuExit, sessionEnter, sessionLoop, onReady);
    }

    public void TransitionToLobby(System.Action onReady = null)
    {
        Transition(sessionExit, lobbyEnter, lobbyLoop, onReady);
    }

    public void TransitionToLevelSelect(System.Action onReady = null)
    {
        Transition(lobbyExit, levelSelectEnter, levelSelectLoop, onReady);
    }

    public void TransitionToMainMenu(System.Action onReady = null)
    {
        Transition(null, mainMenuEnter, mainMenuLoop, onReady);
    }
}