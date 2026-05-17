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
    public string gameWinSceneName = "Win"; // Updated strictly to "Win" as requested!

    public static bool isRetrying = false;
    public static bool isLongReturning = false;

    private CanvasGroup gameplayCanvasGroup;

    void Awake()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // ONLY purge on GameOver or Win! Leave MainMenu alone
        if (currentScene == gameOverSceneName || currentScene == gameWinSceneName)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            PurgePersistentObjects();
        }
        else if (currentScene == mainMenuSceneName)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Safe-find the gameplay Canvas layer if it exists locally
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            gameplayCanvasGroup = canvasObj.GetComponent<CanvasGroup>();
            if (gameplayCanvasGroup == null)
            {
                gameplayCanvasGroup = canvasObj.AddComponent<CanvasGroup>();
            }

            if (currentScene == mainMenuSceneName || currentScene == gameOverSceneName || currentScene == gameWinSceneName)
            {
                SetCanvasVisible(false);
            }
        }
    }

    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == mainMenuSceneName || currentScene == gameOverSceneName || currentScene == gameWinSceneName)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    private void PurgePersistentObjects()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Destroy(playerObj);
            Debug.Log("[System Clean] Persistent PLAYER destroyed for screen safety.");
        }

        GameObject fallbackPlayer = GameObject.Find("PLAYER");
        if (fallbackPlayer != null) Destroy(fallbackPlayer);

        GameObject gameplayCanvas = GameObject.Find("Canvas");
        if (gameplayCanvas != null && gameplayCanvas.gameObject != this.gameObject)
        {
            if (gameplayCanvas.transform.parent == null)
            {
                Destroy(gameplayCanvas);
                Debug.Log("[System Clean] Persistent gameplay HUD Canvas destroyed.");
            }
        }
    }

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
        SetCanvasVisible(true);
        isRetrying = false;
        isLongReturning = false;
        Time.timeScale = 1f;

        // Reset your intro state so it plays fresh from the menu buttons
        StoryIntro.ResetIntroPlaystate();

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

    public void GoToWinScene()
    {
        Time.timeScale = 1f;
        isRetrying = false;
        isLongReturning = false;
        SceneManager.LoadScene(gameWinSceneName); // Loads the "Win" scene cleanly
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        isRetrying = true;
        isLongReturning = false;

        // Reset your intro state so it plays fresh when restarting after a game over
        StoryIntro.ResetIntroPlaystate();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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