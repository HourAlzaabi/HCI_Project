using UnityEngine;
using UnityEngine.UI;

public class UIScrollingBackground : MonoBehaviour
{
    public float scrollSpeedX = 0.1f;
    public float scrollSpeedY = 0.1f;

    private RawImage rawImage; 
    private Vector2 offset;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        offset.x += scrollSpeedX * Time.deltaTime;
        offset.y += scrollSpeedY * Time.deltaTime;

        rawImage.uvRect = new Rect(offset, rawImage.uvRect.size);
    }
}