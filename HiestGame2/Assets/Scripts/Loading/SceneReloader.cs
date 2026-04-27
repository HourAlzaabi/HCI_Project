using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    public static SceneReloader Instance;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ReloadViaIntermediate(string levelName)
    {
        StartCoroutine(DoReload(levelName));
    }

    IEnumerator DoReload(string levelName)
    {
        PhotonNetwork.LoadLevel("Loading");

        while (SceneManager.GetActiveScene().name != "Loading")
            yield return null;

        yield return new WaitForSecondsRealtime(0.3f);

        PhotonNetwork.LoadLevel(levelName);
    }
}