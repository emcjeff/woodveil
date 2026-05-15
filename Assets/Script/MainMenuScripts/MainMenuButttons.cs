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

    public static bool isRetrying = false;
    public static bool isLongReturning = false;

    private CanvasGroup gameplayCanvasGroup;

    void Awake()
    {
        // 1. Setup Cursor for Menu
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. Find the Canvas and its CanvasGroup
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            gameplayCanvasGroup = canvasObj.GetComponent<CanvasGroup>();

            // If it doesn't have a CanvasGroup yet, add one automatically!
            if (gameplayCanvasGroup == null)
            {
                gameplayCanvasGroup = canvasObj.AddComponent<CanvasGroup>();
            }

            // HIDE it for the menu without disabling the object
            SetCanvasVisible(false);
        }
    }

    void Update()
    {
        // 3. Keep fighting for the cursor in the Build menu
        if (SceneManager.GetActiveScene().name == mainMenuSceneName)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    // Helper function to show/hide UI smoothly
    void SetCanvasVisible(bool visible)
    {
        if (gameplayCanvasGroup != null)
        {
            gameplayCanvasGroup.alpha = visible ? 1f : 0f;
            gameplayCanvasGroup.interactable = visible;
            gameplayCanvasGroup.blocksRaycasts = visible;
        }
    }

    public void PlayGame()
    {
        // SHOW the UI just before loading the game
        SetCanvasVisible(true);

        isRetrying = false;
        isLongReturning = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelName);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        isRetrying = false;
        isLongReturning = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void LongReturnToMenu()
    {
        Time.timeScale = 1f;
        isRetrying = false;
        isLongReturning = true;
        SceneManager.LoadScene(gameOverSceneName);
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        isRetrying = true;
        isLongReturning = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator AutoLoadLevelSequence()
    {
        isRetrying = false;
        yield return new WaitForSecondsRealtime(0.2f);
        SceneManager.LoadScene(firstLevelName);
    }

    public void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(false);
    }

    public void BackFromOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}