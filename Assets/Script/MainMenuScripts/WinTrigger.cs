using UnityEngine;
using UnityEngine.SceneManagement; // Added just in case we need a direct scene load fallback

public class WinTrigger : MonoBehaviour
{
    [Header("Victory Page Settings")]
    [Tooltip("The index array slot for the 6th page inside the book system list (Index 5 = Page 6)")]
    [SerializeField] private int requiredPageIndex = 5;

    [Header("Scene Settings")]
    [Tooltip("The exact name of your Victory scene in your Build Settings.")]
    [SerializeField] private string winSceneName = "Win"; // Hardcoded to match your exact scene name: "Win"

    private void OnTriggerEnter(Collider other)
    {
        // 1. Check if the object entering the gate trigger space is tagged as the Player character
        if (other.CompareTag("Player"))
        {
            // 2. Safely verify that the global BookManager system exists in the level memory
            if (BookManager.Instance != null)
            {
                // 3. Ask BookManager if Page 6 (index 5) has been picked up yet
                // KEEPING THIS COMPLETELY UNTOUCHED: Player cannot rush to win!
                bool hasSixthPage = BookManager.Instance.IsPageUnlocked(requiredPageIndex);

                if (hasSixthPage)
                {
                    Debug.Log("[Victory Gate] 6th Page checked and verified! Transporting to Win scene...");
                    TriggerWinScene();
                }
                else
                {
                    // The player touched the door but is missing the required 6th book leaf asset
                    Debug.Log("[Locked Gate] The exit remains sealed. You must locate the 6th page first!");
                    DisplayMissingPageWarning();
                }
            }
            else
            {
                Debug.LogError("[Win Trigger System Error] Could not locate the BookManager.Instance in this scene! Is it missing from your scene hierarchy?");
            }
        }
    }

    private void TriggerWinScene()
    {
        // Find our MainMenu background manager instance to process the Win level cross-fade load
        MainMenu menuManager = FindAnyObjectByType<MainMenu>();

        if (menuManager != null)
        {
            // Try to pass the correct scene name string to your menu manager function if it accepts arguments
            // Example: menuManager.GoToWinScene("Win");
            menuManager.GoToWinScene();
        }
        else
        {
            Debug.LogWarning("[Win Trigger] Could not find a MainMenu component inside this scene. Forcing a direct scene switch fallback to: " + winSceneName);

            // BULLETPROOF FALLBACK: If MainMenu isn't passing the string correctly, 
            // this direct line guarantees it loads your scene named "Win"!
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(winSceneName);
        }
    }

    private void DisplayMissingPageWarning()
    {
        // If you have a custom UI text popup asset to show messages to the player, 
        // you can place your text display triggers here!
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification("The exit remains sealed. You must locate the 6th page first!");
        }
    }
}