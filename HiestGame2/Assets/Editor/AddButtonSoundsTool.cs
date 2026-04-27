#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class AddButtonSoundsTool
{
    [MenuItem("Tools/Add ButtonClickSound to all Buttons in Scene")]
    static void AddToAll()
    {
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        int added = 0;
        foreach (Button b in buttons)
        {
            if (b.GetComponent<ButtonClickSound>() == null)
            {
                Undo.AddComponent<ButtonClickSound>(b.gameObject);
                added++;
            }
        }
        Debug.Log($"Added ButtonClickSound to {added} button(s).");
    }

    [MenuItem("Tools/Find Buttons Missing ButtonClickSound")]
    static void FindMissing()
    {
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        int missing = 0;
        foreach (Button b in buttons)
        {
            if (b.GetComponent<ButtonClickSound>() == null)
            {
                Debug.LogWarning($"Missing ButtonClickSound on: {b.name}", b.gameObject);
                missing++;
            }
        }
        Debug.Log($"Found {missing} button(s) without ButtonClickSound.");
    }
}
#endif