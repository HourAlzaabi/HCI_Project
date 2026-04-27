using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundSceneLoader : MonoBehaviour
{
    void Awake()
    {
        // Only load if not already loaded
        if (BackgroundManager.Instance == null)
        {
            SceneManager.LoadSceneAsync("Background", LoadSceneMode.Additive);
        }
    }
}