#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System.Text;
using System.Collections.Generic;

public static class LocalizationCSVTool
{
    private const string CSV_PATH = "Assets/Localization/translations.csv";

    [MenuItem("Tools/Localization/Export All to CSV")]
    static void ExportToCSV()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(CSV_PATH));

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Scene,GameObject,English,Arabic,Spanish,French");

        int sceneCount = EditorBuildSettings.scenes.Length;
        int totalEntries = 0;

        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = EditorBuildSettings.scenes[i].path;
            if (string.IsNullOrEmpty(scenePath)) continue;

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);

            LocalizedTextSettings[] all = Object.FindObjectsByType<LocalizedTextSettings>(FindObjectsSortMode.None);
            foreach (LocalizedTextSettings lts in all)
            {
                string english = (lts.texts != null && lts.texts.Count > 0) ? lts.texts[0] : "";
                string arabic = (lts.texts != null && lts.texts.Count > 1) ? lts.texts[1] : "";
                string spanish = (lts.texts != null && lts.texts.Count > 2) ? lts.texts[2] : "";
                string french = (lts.texts != null && lts.texts.Count > 3) ? lts.texts[3] : "";

                csv.AppendLine(
                    Escape(sceneName) + "," +
                    Escape(lts.gameObject.name) + "," +
                    Escape(english) + "," +
                    Escape(arabic) + "," +
                    Escape(spanish) + "," +
                    Escape(french));

                totalEntries++;
            }
        }

        File.WriteAllText(CSV_PATH, csv.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();

        Debug.Log($"Exported {totalEntries} entries to {CSV_PATH}");
        EditorUtility.RevealInFinder(CSV_PATH);
    }

    [MenuItem("Tools/Localization/Import CSV to All")]
    static void ImportFromCSV()
    {
        if (!File.Exists(CSV_PATH))
        {
            Debug.LogError($"CSV not found at {CSV_PATH}. Run Export first.");
            return;
        }

        string[] lines = File.ReadAllLines(CSV_PATH, Encoding.UTF8);
        if (lines.Length < 2)
        {
            Debug.LogError("CSV is empty or missing entries.");
            return;
        }

        // Build a map: scene -> goName -> {en, ar, es, fr}
        Dictionary<string, Dictionary<string, string[]>> map =
            new Dictionary<string, Dictionary<string, string[]>>();

        for (int i = 1; i < lines.Length; i++) // skip header
        {
            string[] parts = ParseCSVLine(lines[i]);
            if (parts.Length < 6) continue;

            string scene = parts[0];
            string go = parts[1];
            string[] strings = { parts[2], parts[3], parts[4], parts[5] };

            if (!map.ContainsKey(scene))
                map[scene] = new Dictionary<string, string[]>();
            map[scene][go] = strings;
        }

        int updated = 0;

        foreach (string sceneName in map.Keys)
        {
            // Find scene path by name
            string scenePath = null;
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (Path.GetFileNameWithoutExtension(s.path) == sceneName)
                {
                    scenePath = s.path;
                    break;
                }
            }
            if (scenePath == null) continue;

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            LocalizedTextSettings[] all = Object.FindObjectsByType<LocalizedTextSettings>(FindObjectsSortMode.None);

            foreach (LocalizedTextSettings lts in all)
            {
                string goName = lts.gameObject.name;
                if (!map[sceneName].ContainsKey(goName)) continue;

                string[] strings = map[sceneName][goName];
                Undo.RecordObject(lts, "Import CSV");
                lts.texts = new List<string>(strings);
                EditorUtility.SetDirty(lts);
                updated++;
            }

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        Debug.Log($"Imported {updated} entries from {CSV_PATH}");
    }

    static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }

    static string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        StringBuilder current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else if (c == '"') inQuotes = false;
                else current.Append(c);
            }
            else
            {
                if (c == ',') { result.Add(current.ToString()); current.Clear(); }
                else if (c == '"') inQuotes = true;
                else current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result.ToArray();
    }
}
#endif