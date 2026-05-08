using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    // Change 'slider' to 'Slider' (Capital S)
    [SerializeField] private Slider slider; 
    
    // Change 'Float' to 'float' (Lowercase f)
    public void UpdateHealthBar(float currentValue, float maxValue)
    {
         slider.value = currentValue / maxValue;
    }
}