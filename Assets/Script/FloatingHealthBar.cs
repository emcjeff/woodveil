using UnityEngine;
using UnityEngine.UI;

public class floatingHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    // Call this to update the slider and show the bar
    public void updateHealthBar(float currentValue, float maxValue)
    {
        gameObject.SetActive(true); // Make it visible when updated
        slider.maxValue = maxValue;
        slider.value = currentValue;
    }

    // Call this to hide it (e.g., at the start of the game)
    public void HideBar()
    {
        gameObject.SetActive(false);
    }
}