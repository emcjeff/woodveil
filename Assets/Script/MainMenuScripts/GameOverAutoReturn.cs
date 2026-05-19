using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverAutoReturn : MonoBehaviour
{
    [Header("Return Settings")]
    [Tooltip("How long to wait before automatically jumping back to the Main Menu scene.")]
    public float delay = 2.5f;

    void Start()
    {
        // If the MainMenu script says "isLongReturning", start the timer
        if (MainMenu.isLongReturning)
        {
            StartCoroutine(ReturnSequence());
        }
    }

    IEnumerator ReturnSequence()
    {
        yield return new WaitForSecondsRealtime(delay);

        // Reset the control flag passport safely
        MainMenu.isLongReturning = false;

        Debug.Log("[Auto Return] Triggering fresh reload of the core Main Menu scene workflow...");

        // FIX: Find the MainMenu manager component in the scene if it exists to call its clean routing method
        MainMenu menuManager = FindAnyObjectByType<MainMenu>();
        if (menuManager != null)
        {
            // This guarantees it uses whatever exact string name variable your MainMenu manager uses!
            menuManager.ReturnToMainMenu();
        }
        else
        {
            // Bulletproof fallback if no manager component is awake in the scene space
            SceneManager.LoadScene("MainMenu");
        }
    }
}