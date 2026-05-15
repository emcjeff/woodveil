using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // Required for changing scenes

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; set; }

    [Header("Player Health")]
    public float currentHealth;
    public float maxHealth;
    public float healthRegenAmount = 1.0f;
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    [Header("Scene Settings")]
    public string gameOverSceneName = "GameOver"; // Make sure this matches your scene name exactly!
    public GameObject uiCanvas; // Drag your persistent Canvas here

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // Since this script is usually on the Player, 
            // and you want the player to persist, ensure it doesn't destroy.
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthSlider != null) healthSlider.maxValue = maxHealth;
    }

    void Update()
    {
        if (currentHealth < maxHealth && currentHealth > 0)
        {
            currentHealth += healthRegenAmount * Time.deltaTime;
        }

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        if (currentHealth <= 0)
        {
            // This triggers the scene change. 
            // Once the scene "GameOver" loads, the scripts above will see it and self-destruct.
            SceneManager.LoadScene("GameOver");
        }
    }

    void TriggerGameOver()
    {
        Debug.Log("Health reached 0. Moving to Game Over scene.");

        // 1. Hide the persistent Gameplay UI so it doesn't block the Game Over screen
        if (uiCanvas != null)
        {
            uiCanvas.SetActive(false);
        }

        // 2. Load the Game Over scene
        SceneManager.LoadScene(gameOverSceneName);

        // 3. Optional: Destroy the player object so a fresh one spawns when restarting
        // If you want the player to stay, don't destroy it.
        // Destroy(gameObject); 
    }

    void UpdateUI()
    {
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (healthText != null) healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {maxHealth}";
    }
}