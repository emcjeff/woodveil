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
            Transform targetPanel = canvasObj.transform.Find("Options Panel");
            if (targetPanel != null)
            {
                optionsPanel = targetPanel.gameObject;
                optionsPanel.SetActive(false);
                Debug.Log("[Pause Manager] Options Panel linked successfully.");
            }
            else
            {
                optionsPanel = GameObject.Find("Options Panel");
                if (optionsPanel != null) optionsPanel.SetActive(false);
            }
        }
    }

    void Update()
    {
        // Do not intercept input keys if there are no UI panels attached to toggle
        if (optionsPanel == null) return;

        // CHANGED: Listens for KeyCode.Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // SMART ESCAPE LOGIC: If a gameplay menu is open, don't pause—just let the player use Esc to close it!
            bool isInventoryOpen = (InventorySystem.Instance != null && InventorySystem.Instance.isOpen);
            bool isCraftingOpen = (CraftingSystem.Instance != null && CraftingSystem.Instance.isOpen);
            bool isBookOpen = (BookManager.Instance != null && BookManager.Instance.isBookOpen);

            if (isInventoryOpen || isCraftingOpen || isBookOpen)
            {
                Debug.Log("[Pause Manager] Escape pressed while a UI window was active. Letting system layers handle the close request instead of pausing.");
                return;
            }

            // Normal Pause/Resume processing
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

        // Check layout overlays cleanly before committing lock state profiles
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
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}