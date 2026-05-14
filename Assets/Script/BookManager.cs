using UnityEngine;
using UnityEngine.SceneManagement; // Added for scene loading logic

public class BookManager : MonoBehaviour
{
    public static BookManager Instance { get; private set; }

    public GameObject bookUI;
    public GameObject BookPrompt;

    public bool isBookOpen = false;
    public bool hasBook = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // Ensure the BookManager travels between scenes
            DontDestroyOnLoad(gameObject);
        }
    }

    // --- SCENE TRANSITION FIX ---
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When entering the Cave, force the book to a closed state
        // This fixes buttons becoming unresponsive due to "ghost" states
        CloseBook();
    }
    // ----------------------------

    void Update()
    {
        if (hasBook && !isBookOpen && !InventorySystem.Instance.isOpen && !CraftingSystem.Instance.isOpen)
        {
            BookPrompt.SetActive(true);
        }
        else
        {
            BookPrompt.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.E) && hasBook)
        {
            if (isBookOpen) CloseBook();
            else OpenBook();
        }
    }

    public void CollectBook()
    {
        hasBook = true;
    }

    public void OpenBook()
    {
        bookUI.SetActive(true);
        isBookOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.DisableSelection();
        }
    }

    public void CloseBook()
    {
        // Find the 'book' script on your UI
        book pageScript = bookUI.GetComponentInChildren<book>();
        if (pageScript != null)
        {
            pageScript.ResetState(); // This stops the Coroutine and unlocks 'rotate'
        }

        bookUI.SetActive(false);
        isBookOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.EnableSelection();
        }
    }
}