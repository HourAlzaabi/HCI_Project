using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    public TMP_Text timerText;

    [Header("Timer Colors")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;

    [Header("Warning")]
    public float warningStartTime = 270f; // last 30 sec of a 5 min timer
    public bool pulseWhenWarning = true;
    public float pulseSpeed = 5f;

    void Update()
    {
        if (LevelManager.Instance == null || timerText == null) return;

        float time = LevelManager.Instance.levelTimer;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");

        if (time >= warningStartTime)
        {
            if (pulseWhenWarning)
            {
                timerText.color = Color.Lerp(
                    normalColor,
                    warningColor,
                    Mathf.PingPong(Time.unscaledTime * pulseSpeed, 1f)
                );
            }
            else
            {
                timerText.color = warningColor;
            }
        }
        else
        {
            timerText.color = normalColor;
        }
    }
}