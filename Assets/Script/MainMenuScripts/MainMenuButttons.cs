using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject optionsPanel;
    public GameObject mainButtonsGroup;
    public GameObject pauseMenuPanel;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string firstLevelName = "wodbeyl";
    public string gameOverSceneName = "GameOver";

    // Static variables survive scene changes
    public static bool isRetrying = false;
    public static bool isLongReturning = false; // The Passport

    void Start()
    {
        // Forces the game to UNFREEZE every time a scene starts
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Auto-load logic for the "Retry" button feature
        if (isRetrying && SceneManager.GetActiveScene().name == mainMenuSceneName)
        {
            StartCoroutine(AutoLoadLevelSequence());
        }
    }

    // --- 1. THE LONG RETURN (The Scenic Route) ---
    public void LongReturnToMenu()
    {
        Time.timeScale = 1f;
        isRetrying = false;

        // We set the passport to TRUE so the next scene knows to take us home
        isLongReturning = true;

        // Just load the scene. The "AutoReturn" script in the GameOver scene will do the rest.
        SceneManager.LoadScene(gameOverSceneName);
    }

    // --- 2. THE NORMAL RETURN (Instant) ---
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        isRetrying = false;
        isLongReturning = false; // Safety reset
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // --- NAVIGATION & RETRY ---

    public void PlayGame()
    {
        isRetrying = false;
        isLongReturning = false;
        SceneManager.LoadScene(firstLevelName);
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        isRetrying = true;
        isLongReturning = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private IEnumerator AutoLoadLevelSequence()
    {
        isRetrying = false;
        yield return new WaitForSecondsRealtime(0.2f);
        SceneManager.LoadScene(firstLevelName);
    }

    // --- UI HELPERS ---

    public void ResumeGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenOptions() => ToggleOptions(true);
    public void BackFromOptions() => ToggleOptions(false);

    private void ToggleOptions(bool showOptions)
    {
        if (optionsPanel != null) optionsPanel.SetActive(showOptions);
        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(!showOptions);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}