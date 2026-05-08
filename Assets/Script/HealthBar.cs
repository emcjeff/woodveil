using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
        
    public TMP_Text healthCounter;

    public GameObject PlayerState;

    private float currentHealth, maxHealth;
    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    
    void Update()
    {
        currentHealth = PlayerState.GetComponent<PlayerState>().currentHealth;
        maxHealth = PlayerState.GetComponent<PlayerState>().maxHealth;
    
        float fillValue = currentHealth / maxHealth; //0 - 1
        slider.value = fillValue;

        healthCounter.text = currentHealth + "/" + maxHealth; 
    }
}
