using UnityEngine;

public class BookManager : MonoBehaviour
{
    public GameObject bookUI;
    private bool isBookOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isBookOpen)
            {
                CloseBook();
            }
            else
            {
                OpenBook();
            }
        }
    }

    public void OpenBook()
    {
        bookUI.SetActive(true);
        isBookOpen = true;

        // Free the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Optional: Tell SelectionManager to stop highlighting things
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.DisableSelection();
    }

    public void CloseBook()
    {
        bookUI.SetActive(false);
        isBookOpen = false;

        // Lock the cursor back for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Re-enable crosshair/interaction
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.EnableSelection();
    }
}