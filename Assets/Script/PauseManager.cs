using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject optionsPanel;
    private bool isPaused = false;

    void Update()
    {
        // Using 'Q' as your master pause key
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        optionsPanel.SetActive(true);
        Time.timeScale = 0f; // Freezes game logic
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Ensure selection is disabled while paused
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.DisableSelection();
        }
    }

    public void Resume()
    {
        optionsPanel.SetActive(false);
        Time.timeScale = 1f; // Unfreezes game logic
        isPaused = false;

        // MASTER CURSOR CHECK:
        // Only lock the cursor if ALL other menus are closed.
        if (!InventorySystem.Instance.isOpen &&
            !CraftingSystem.Instance.isOpen &&
            !BookManager.Instance.isBookOpen)
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
            // If another menu is still open, keep the cursor free
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Reset time so the next scene isn't frozen
        SceneManager.LoadScene("MainMenu");
    }
}