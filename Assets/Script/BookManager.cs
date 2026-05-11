using UnityEngine;

public class BookManager : MonoBehaviour
{
    public static BookManager Instance { get; private set; }

    public GameObject bookUI;     // The actual book pages UI
    public GameObject BookPrompt; // The "E to Read" Sprite/Text group

    // This is now public so Inventory and Crafting scripts can check it
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
        }
    }

    void Update()
    {
        // 1. Show/Hide the "E to Read" icon
        if (hasBook && !isBookOpen && !InventorySystem.Instance.isOpen && !CraftingSystem.Instance.isOpen)
        {
            BookPrompt.SetActive(true);
        }
        else
        {
            BookPrompt.SetActive(false);
        }

        // 2. Only open when 'E' is pressed, NOT when collected
        if (Input.GetKeyDown(KeyCode.E) && hasBook)
        {
            if (isBookOpen) CloseBook();
            else OpenBook();
        }
    }

    public void CollectBook()
    {
        hasBook = true;
        // Ensure this function ONLY sets the boolean. 
        // If you have bookUI.SetActive(true) here, DELETE IT.
    }

    public void OpenBook()
    {
        bookUI.SetActive(true);
        isBookOpen = true;

        // UI Mouse Logic
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Turn off the crosshair/selection while reading
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.DisableSelection();
        }
    }

    public void CloseBook()
    {
        bookUI.SetActive(false);
        isBookOpen = false;

        // Return mouse to game control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Turn the crosshair/selection back on
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.EnableSelection();
        }
    }
}