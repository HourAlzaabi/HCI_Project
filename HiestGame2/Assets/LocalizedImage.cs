using UnityEngine;
using UnityEngine.UI;

public class LocalizedImage : MonoBehaviour
{
    [System.Serializable]
    public class LanguageSprite
    {
        public SettingsManager.Language language;
        public Sprite sprite;
    }

    [Header("Target")]
    [SerializeField] private Image targetImage;

    [Header("Sprites")]
    [SerializeField] private LanguageSprite[] localizedSprites;

    private void Reset()
    {
        targetImage = GetComponent<Image>();
    }

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        SettingsManager.OnSettingsChanged += RefreshImage;
        RefreshImage();
    }

    private void OnDisable()
    {
        SettingsManager.OnSettingsChanged -= RefreshImage;
    }

    public void RefreshImage()
    {
        if (targetImage == null || SettingsManager.Instance == null)
            return;

        SettingsManager.Language currentLanguage = SettingsManager.Instance.currentLanguage;

        for (int i = 0; i < localizedSprites.Length; i++)
        {
            if (localizedSprites[i].language == currentLanguage)
            {
                if (localizedSprites[i].sprite != null)
                    targetImage.sprite = localizedSprites[i].sprite;

                return;
            }
        }

        for (int i = 0; i < localizedSprites.Length; i++)
        {
            if (localizedSprites[i].language == SettingsManager.Language.English)
            {
                if (localizedSprites[i].sprite != null)
                    targetImage.sprite = localizedSprites[i].sprite;

                return;
            }
        }
    }
}