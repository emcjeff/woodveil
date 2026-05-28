using UnityEngine;

public class MenuParallaxUI : MonoBehaviour
{
    public float parallaxIntensity = 20f; // UI needs higher numbers (20-50)

    private Vector2 startPosition;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // Get mouse position relative to screen center
        float xOffset = (Input.mousePosition.x - (Screen.width / 2f)) / Screen.width;
        float yOffset = (Input.mousePosition.y - (Screen.height / 2f)) / Screen.height;

        // Use anchoredPosition for UI elements
        rectTransform.anchoredPosition = new Vector2(
            startPosition.x + (xOffset * parallaxIntensity),
            startPosition.y + (yOffset * parallaxIntensity)
        );
    }
}