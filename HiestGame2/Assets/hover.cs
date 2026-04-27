using UnityEngine;

public class hover : MonoBehaviour
{
    public float hoverAmount = 10f;   // How high it moves
    public float hoverSpeed = 2f;     // How fast it moves

    private RectTransform rectTransform;
    private Vector3 startPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        float newY = Mathf.Sin(Time.time * hoverSpeed) * hoverAmount;
        rectTransform.anchoredPosition = startPos + new Vector3(0, newY, 0);
    }
}
