using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI Control Panels")]
    public GameObject optionsPanel;
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLayoutLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLayoutLoaded;
    }

    private void Start()
    {
        FindActiveSceneOptionsPanel(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLayoutLoaded(Scene scene, LoadSceneMode mode)
    {
        FindActiveSceneOptionsPanel(scene.name);
    }

    /// <summary>
    /// Searches for the options panel UI assets inside the active scene layout.
    /// </summary>
    private void FindActiveSceneOptionsPanel(string sceneName)
    {
        isPaused = false;

        if (sceneName == "MainMenu" || sceneName == "GameOver" || sceneName == "Win")
        {
            optionsPanel = null;
            return;
        }

        // Look for the Canvas inside your active gameplay scenes
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            // Matches your options UI sub-panel naming structure
            Transform targetPanel = canvasObj.transform.Find("Options Panel");
            if (targetPanel != null)
            {
                optionsPanel = targetPanel.gameObject;
                optionsPanel.SetActive(false);
                Debug.Log("[Pause Manager] Options Panel linked successfully.");
            }
            else
            {
                // Fallback check: look through root level assets
                optionsPanel = GameObject.Find("Options Panel");
                if (optionsPanel != null) optionsPanel.SetActive(false);
            }
        }
    }

    void Update()
    {
        // Do not intercept input keys if there are no UI panels attached to toggle
        if (optionsPanel == null) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.DisableSelection();
        }
    }

    public void Resume()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // Check inventory, crafting, and book states safely before managing cursor states
        bool isInventoryOpen = (InventorySystem.Instance != null && InventorySystem.Instance.isOpen);
        bool isCraftingOpen = (CraftingSystem.Instance != null && CraftingSystem.Instance.isOpen);
        bool isBookOpen = (BookManager.Instance != null && BookManager.Instance.isBookOpen);

        if (!isInventoryOpen && !isCraftingOpen && !isBookOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (SelectionManager.Instance != null)
            {
                SelectionManager.Instance.EnableSelection();
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}