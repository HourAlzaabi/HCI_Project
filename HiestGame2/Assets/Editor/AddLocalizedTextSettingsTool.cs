#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

public static class AddLocalizedTextSettingsTool
{
    [MenuItem("Tools/Localization/Add LocalizedTextSettings to all TMP_Text")]
    static void AddToAll()
    {
        TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        int added = 0;
        int alreadyHad = 0;

        foreach (TMP_Text t in texts)
        {
            if (t.GetComponent<LocalizedTextSettings>() == null)
            {
                Undo.AddComponent<LocalizedTextSettings>(t.gameObject);
                added++;
            }
            else
            {
                alreadyHad++;
            }
        }

        Debug.Log($"Added LocalizedTextSettings to {added} text(s). {alreadyHad} already had it.");
    }

    [MenuItem("Tools/Localization/Pre-fill English Text from Existing Content")]
    static void PrefillEnglish()
    {
        TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        int filled = 0;

        foreach (TMP_Text t in texts)
        {
            LocalizedTextSettings lts = t.GetComponent<LocalizedTextSettings>();
            if (lts == null) continue;

            // Only pre-fill if the texts list is empty/missing
            if (lts.texts == null || lts.texts.Count == 0)
            {
                Undo.RecordObject(lts, "Prefill English text");
                lts.texts = new System.Collections.Generic.List<string>
                {
                    t.text,  // English (current text content)
                    "",      // Arabic (empty for you to fill)
                    "",      // Spanish
                    ""       // French
                };
                EditorUtility.SetDirty(lts);
                filled++;
            }
        }

        Debug.Log($"Pre-filled {filled} text(s) with English from current content. Now fill Arabic/Spanish/French.");
    }
}
#endif