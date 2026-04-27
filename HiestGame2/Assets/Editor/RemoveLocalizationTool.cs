#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

public static class RemoveLocalizationTool
{
    [MenuItem("Tools/Localization/REMOVE All LocalizedTextSettings From All Scenes")]
    static void RemoveAll()
    {
        if (!EditorUtility.DisplayDialog(
            "Remove All Localization",
            "This will REMOVE every LocalizedTextSettings component from every scene in Build Settings AND reset the RTL flag on every TMP_Text. Cannot be undone.\n\nProceed?",
            "Yes, remove all",
            "Cancel"))
            return;

        int totalRemoved = 0;
        int totalRTLReset = 0;

        foreach (var sceneEntry in EditorBuildSettings.scenes)
        {
            if (string.IsNullOrEmpty(sceneEntry.path)) continue;

            EditorSceneManager.OpenScene(sceneEntry.path, OpenSceneMode.Single);

            // Remove all LocalizedTextSettings components
            LocalizedTextSettings[] all = Object.FindObjectsByType<LocalizedTextSettings>(FindObjectsSortMode.None);
            int sceneRemoved = 0;
            foreach (LocalizedTextSettings lts in all)
            {
                Object.DestroyImmediate(lts);
                sceneRemoved++;
                totalRemoved++;
            }

            // Reset RTL flag on every TMP_Text in the scene
            TMP_Text[] allTexts = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
            int sceneRTLReset = 0;
            foreach (TMP_Text t in allTexts)
            {
                if (t.isRightToLeftText)
                {
                    t.isRightToLeftText = false;
                    EditorUtility.SetDirty(t);
                    sceneRTLReset++;
                    totalRTLReset++;
                }
            }

            if (sceneRemoved > 0 || sceneRTLReset > 0)
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            Debug.Log("Scene " + Path.GetFileNameWithoutExtension(sceneEntry.path) +
                      ": removed " + sceneRemoved + " components, reset " +
                      sceneRTLReset + " RTL flags.");
        }

        Debug.Log("DONE. Total removed: " + totalRemoved + " components. Total RTL reset: " + totalRTLReset + " texts.");
    }
}
#endif