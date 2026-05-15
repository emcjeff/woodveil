using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; private set; }

    [Header("Player Health")]
    public float maxHealth = 100;
    public float currentHealth;
    public float healthRegenAmount = 1.0f;

    [Header("UI References (Auto-Filled)")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    [Header("Scene Settings")]
    public string gameOverSceneName = "GameOver";

    private void Awake()
    {
        // Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // THIS IS THE FIX: It looks for the UI every time a scene changes
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ReconnectUI());
    }

    IEnumerator ReconnectUI()
    {
        // Wait a tiny fraction for the new scene UI to initialize
        yield return new WaitForSeconds(0.05f);

        // 1. Find the Health Slider by name or tag if it's missing
        if (healthSlider == null)
        {
            GameObject sliderObj = GameObject.Find("HealthSlider"); // Ensure your Slider is named this!
            if (sliderObj != null) healthSlider = sliderObj.GetComponent<Slider>();
        }

        // 2. Find the Health Text
        if (healthText == null)
        {
            GameObject textObj = GameObject.Find("HealthText"); // Ensure your TMP Text is named this!
            if (textObj != null) healthText = textObj.GetComponent<TextMeshProUGUI>();
        }

        // 3. Update the UI values immediately
        UpdateUI();
    }

    void Update()
    {
        // Regenerate health if alive
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
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            TriggerGameOver();
        }
        UpdateUI();
    }

    void TriggerGameOver()
    {
        SceneManager.LoadScene(gameOverSceneName);
    }

    void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {maxHealth}";
        }
    }
}