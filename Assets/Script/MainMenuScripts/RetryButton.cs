using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryButtonHandler : MonoBehaviour
{
    public void OnRetryButtonClicked()
    {
        // 1. CRITICAL: Unfreeze the game clock so animations, inputs, and UI can move again!
        Time.timeScale = 1f;

        // 2. Clear any lingering static flags so the book/mission papers know this is a fresh run
        MainMenu.cameFromMenu = true;

        // 3. Reset the cursor settings so it doesn't get stuck on screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 4. Force BookManager to clean its slate completely if it exists
        if (BookManager.Instance != null)
        {
            BookManager.Instance.WipeAndResetProgressionSaveData();
        }

        // 5. Load your gameplay scene (replace "wodbeyl" with your actual scene name if needed)
        SceneManager.LoadScene("wodbeyl");

        Debug.Log("[Retry System] Game state scrubbed cleanly. Reloading level scene context.");
    }
}