using UnityEngine;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedTextSettings : MonoBehaviour
{
    [Header("Fonts")]
    [SerializeField] private TMP_FontAsset defaultFont;
    [SerializeField] private TMP_FontAsset arabicFont;

    [Header("Localized Text (Index = Language Enum)")]
    [Tooltip("Index 0 = English, 1 = Arabic, 2 = Spanish, 3 = French")]
    [TextArea]
    public List<string> texts;

    private TMP_Text textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        SettingsManager.OnSettingsChanged += ApplySettings;
        ApplySettings();
    }

    private void OnDisable()
    {
        SettingsManager.OnSettingsChanged -= ApplySettings;
    }

    private void ApplySettings()
    {
        if (SettingsManager.Instance == null || textComponent == null) return;
        ApplyLanguage();
        ApplyFontAndAlignment();
    }

    private void ApplyLanguage()
    {
        int index = (int)SettingsManager.Instance.currentLanguage;
        if (texts != null && index < texts.Count && !string.IsNullOrEmpty(texts[index]))
        {
            textComponent.text = texts[index];
        }
        else if (texts != null && texts.Count > 0)
        {
            textComponent.text = texts[0];
        }
    }

    private void ApplyFontAndAlignment()
    {
        bool isArabic = SettingsManager.Instance.currentLanguage == SettingsManager.Language.Arabic;

        if (isArabic)
        {
            if (arabicFont != null)
                textComponent.font = arabicFont;
            textComponent.isRightToLeftText = true;
        }
        else
        {
            if (defaultFont != null)
                textComponent.font = defaultFont;
            textComponent.isRightToLeftText = false;
        }

        textComponent.ForceMeshUpdate();
    }
}