using UnityEngine;
using UnityEngine.UI; // Required for using the Slider

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    // This is the missing method causing your error!
    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;

        gradient.Evaluate(1f); // Set the color to the max health color
    }

    // You likely need this one too for your TakeDamage method
    public void SetHealth(int health)
    {
        slider.value = health;

        fill.color = gradient.Evaluate(slider.normalizedValue); // Change color based on health percentage
    }
}