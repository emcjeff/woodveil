using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels (Main Menu Scene Only)")]
    public GameObject optionsPanel;
    public GameObject mainButtonsGroup;
    public GameObject pauseMenuPanel;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string firstLevelName = "wodbeyl";
    public string gameOverSceneName = "GameOver";
    public string WinSceneName = "Win";

    public static bool isRetrying = false;
    public static bool isLongReturning = false;

    private CanvasGroup gameplayCanvasGroup;

    void Awake()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Time.timeScale = 1f;

        if (currentScene == mainMenuSceneName)
        {
            isLongReturning = false;
            isRetrying = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            ValidateMainMenuEventSystem();

            // SUCCESS HAND-OFF: The Main Menu has fully awoken!
            // Dismiss the persistent loading screen child panel using your inspector delay time.
            if (LoadingScreenOverlay.Instance != null)
            {
                Debug.Log("[MainMenu Awake] Main Menu scene loaded. Directing LoadingScreenOverlay to hide...");
                LoadingScreenOverlay.Instance.HideWithDelay();
            }
        }
        else if (currentScene == gameOverSceneName || currentScene == WinSceneName)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            PurgePersistentObjects();
        }

        // Locate layout elements safely without breaking scene management architectures
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            gameplayCanvasGroup = canvasObj.GetComponent<CanvasGroup>();
            if (gameplayCanvasGroup == null)
            {
                gameplayCanvasGroup = canvasObj.AddComponent<CanvasGroup>();
            }

            if (currentScene == mainMenuSceneName || currentScene == gameOverSceneName || currentScene == WinSceneName)
            {
                SetCanvasVisible(false);
            }
        }
    }

    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == mainMenuSceneName || currentScene == gameOverSceneName || currentScene == WinSceneName)
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
        // 1. Destroy player asset allocations
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) Destroy(playerObj);

        GameObject fallbackPlayer = GameObject.Find("PLAYER");
        if (fallbackPlayer != null) Destroy(fallbackPlayer);

        // 2. Destroy persistent gameplay HUD canvas layouts safely
        GameObject gameplayCanvas = GameObject.Find("Canvas");
        if (gameplayCanvas != null && gameplayCanvas.gameObject != this.gameObject)
        {
            if (gameplayCanvas.transform.parent == null)
            {
                Destroy(gameplayCanvas);
                Debug.Log("[System Clean] Persistent gameplay HUD Canvas purged.");
            }
        }

        // 3. Purge duplicate Input EventSystems
        UnityEngine.EventSystems.EventSystem currentSystem = UnityEngine.EventSystems.EventSystem.current;
        if (currentSystem != null)
        {
            Destroy(currentSystem.gameObject);
        }
        else
        {
            GameObject looseSystemObj = GameObject.Find("EventSystem");
            if (looseSystemObj != null && looseSystemObj.transform.parent == null)
            {
                Destroy(looseSystemObj);
            }
        }
    }

    private void ValidateMainMenuEventSystem()
    {
        UnityEngine.EventSystems.EventSystem[] activeSystems = FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);

        if (activeSystems.Length > 1)
        {
            for (int i = 1; i < activeSystems.Length; i++)
            {
                if (activeSystems[i] != null) Destroy(activeSystems[i].gameObject);
            }
        }
        else if (activeSystems.Length == 0)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
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

        if (InventorySystem.Instance != null) InventorySystem.Instance.ResetInventoryDataState();

        StoryIntro.ResetIntroPlaystate();
        SceneManager.LoadScene(firstLevelName);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        isRetrying = false;
        isLongReturning = true; // Set to true to route cleanly through our trash dump scene context

        // Draw the curtain mask over the view window before changing scene files
        if (LoadingScreenOverlay.Instance != null)
        {
            LoadingScreenOverlay.Instance.Show();
        }

        SceneManager.LoadScene(gameOverSceneName);
    }

    public void LongReturnToMenu()
    {
        Time.timeScale = 1f;
        isRetrying = false;
        isLongReturning = true;

        if (LoadingScreenOverlay.Instance != null)
        {
            LoadingScreenOverlay.Instance.Show();
        }

        SceneManager.LoadScene(gameOverSceneName);
    }

    public void GoToWinScene()
    {
        Time.timeScale = 1f;
        isRetrying = false;
        isLongReturning = true;

        DisableActiveGameplaySystems();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[Victory] Transitioning to Win Scene cleanly.");
        SceneManager.LoadScene(WinSceneName);
    }

    private void DisableActiveGameplaySystems()
    {
        if (SelectionManager.Instance != null) SelectionManager.Instance.enabled = false;
        if (BookManager.Instance != null) BookManager.Instance.enabled = false;
        if (InventorySystem.Instance != null) InventorySystem.Instance.enabled = false;

        MonoBehaviour mainCamScript = Camera.main != null ? Camera.main.GetComponent<MonoBehaviour>() : null;
        if (mainCamScript != null) mainCamScript.enabled = false;
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        isRetrying = true;
        isLongReturning = false;

        if (InventorySystem.Instance != null) InventorySystem.Instance.ResetInventoryDataState();

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