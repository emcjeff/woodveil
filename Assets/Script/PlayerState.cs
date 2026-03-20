using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public static  PlayerState Instance {get; set;}
    // ---- Player Health ---- //
    public float currentHealth;
    public float maxHealth;
    

    // ---- Player Stamina ---- //
    public float currentStamina;
    public float maxStamina;

    private void Awake()
    {
    if (Instance!= null && Instance != this)
    {
        Destroy(gameObject);
    }
    else
    {
        Instance = this;
    }
    }

    private void Start ()
    {
        currentHealth = maxHealth; 
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            currentHealth -= 10;
        }
        if (currentHealth < 0)
    {
        currentHealth = 0;
    }
    }
}
