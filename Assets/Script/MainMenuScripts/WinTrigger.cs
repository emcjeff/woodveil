using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    [Header("Victory Page Settings")]
    [Tooltip("The index array slot for the 6th page inside the book system list (Index 5 = Page 6)")]
    [SerializeField] private int requiredPageIndex = 5;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Check if the object entering the gate trigger space is tagged as the Player character
        if (other.CompareTag("Player"))
        {
            // 2. Safely verify that the global BookManager system exists in the level memory
            if (BookManager.Instance != null)
            {
                // 3. Ask BookManager if Page 6 (index 5) has been picked up yet
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
            menuManager.GoToWinScene();
        }
        else
        {
            Debug.LogError("[Win Trigger Setup Error] Could not find a MainMenu component inside this scene to trigger the Win scene swap!");
        }
    }

    private void DisplayMissingPageWarning()
    {
        // If you have a custom UI text popup asset to show messages to the player, 
        // you can place your text display triggers here!
        // Example: NotificationCanvas.Instance.ShowMessage("You need the 6th page to leave the cave.");
    }
}