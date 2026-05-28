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

    // GLOBAL STATUS STATE TRACKER
    public static bool cameFromMenu = true;

    private CanvasGroup gameplayCanvasGroup;

    void Awake()
    {
        // FORCE time to unfreeze immediately before evaluating any transitions
        Time.timeScale = 1f;

        string currentScene = SceneManager.GetActiveScene().name;

        // If the player lands in any menu structure state context, firmly lock down the popup flag
        if (currentScene == mainMenuSceneName || currentScene == gameOverSceneName || currentScene == WinSceneName)
        {
            cameFromMenu = true;
            Debug.Log($"[MainMenu Awake] State context flagged: cameFromMenu = {cameFromMenu} via scene: {currentScene}");
        }

        if (currentScene == mainMenuSceneName)
        {
            // Reset transition cycles completely upon successfully landing in the Main Menu
            isLongReturning = false;

            // FAST RE-ROUTE FOR RETRIES
            if (isRetrying)
            {
                Debug.Log("[MainMenu Awake] Retry flag confirmed active! Bypassing landing choices and launching back into stage...");
                PlayGame();
                return;
            }

            isRetrying = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            ValidateMainMenuEventSystem();

            if (LoadingScreenOverlay.Instance != null)
            {
                Debug.Log("[MainMenu Awake] Standard Main Menu arrival. Signaling LoadingScreenOverlay curtain drop...");
                LoadingScreenOverlay.Instance.HideWithDelay();
            }
        }
        else if (currentScene == gameOverSceneName || currentScene == WinSceneName)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            PurgePersistentObjects();

            // AUTOMATIC BOUNCE TO MAIN MENU FOR RETRIES / RETURNS
            if (isRetrying || isLongReturning)
            {
                Debug.Log($"[Scene Handler] Active transition loop running (Retry={isRetrying}, LongReturn={isLongReturning}). Shifting scene payload to Main Menu...");
                SceneManager.LoadScene(mainMenuSceneName);
                return; // Stop running Awake processing loop for this frame
            }
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
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) Destroy(playerObj);

        GameObject fallbackPlayer = GameObject.Find("PLAYER");
        if (fallbackPlayer != null) Destroy(fallbackPlayer);

        GameObject gameplayCanvas = GameObject.Find("Canvas");
        if (gameplayCanvas != null && gameplayCanvas.gameObject != this.gameObject)
        {
            if (gameplayCanvas.transform.parent == null)
            {
                Destroy(gameplayCanvas);
                Debug.Log("[System Clean] Persistent gameplay HUD Canvas purged.");
            }
        }

        // FIXED: Do not destroy the event system if we are instantly reloading the main menu, 
        // otherwise the new scene will lose input detection capabilities.
        UnityEngine.EventSystems.EventSystem currentSystem = UnityEngine.EventSystems.EventSystem.current;
        if (currentSystem != null && !isLongReturning && !isRetrying)
        {
            Destroy(currentSystem.gameObject);
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
        Time.timeScale = 1f;

        if (InventorySystem.Instance != null) InventorySystem.Instance.ResetInventoryDataState();

        cameFromMenu = true;
        isRetrying = false;
        isLongReturning = false;

        StoryIntro.ResetIntroPlaystate();
        SceneManager.LoadScene(firstLevelName);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        isRetrying = false;
        isLongReturning = true;

        if (LoadingScreenOverlay.Instance != null)
        {
            LoadingScreenOverlay.Instance.Show();
        }

        if (BookManager.Instance != null)
        {
            BookManager.Instance.WipeAndResetProgressionSaveData();
        }

        SceneManager.LoadScene(mainMenuSceneName);
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

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void GoToWinScene()
    {
        Time.timeScale = 1f;
        
        // FIXED: Explicitly set these flags to false so the Win scene's Awake 
        // function knows to display the victory screen instead of kicking you to the menu!
        isRetrying = false;
        isLongReturning = false;

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

        if (LoadingScreenOverlay.Instance != null)
        {
            LoadingScreenOverlay.Instance.Show();
        }

        SceneManager.LoadScene(gameOverSceneName);
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