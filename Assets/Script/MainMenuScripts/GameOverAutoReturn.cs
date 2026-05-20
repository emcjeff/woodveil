using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverAutoReturn : MonoBehaviour
{
    [Header("Return Settings")]
    [Tooltip("How long to wait inside this scene to clear data structures before jumping back to Main Menu.")]
    public float delay = 2.5f;

    void Start()
    {
        if (MainMenu.isLongReturning)
        {
            StartCoroutine(ReturnSequence());
        }
    }

    IEnumerator ReturnSequence()
    {
        // 1. Wait out the cleaning delay behind the dark curtain overlay
        yield return new WaitForSecondsRealtime(delay);

        // Reset the routing flag passport safely
        MainMenu.isLongReturning = false;

        Debug.Log("[Auto Return] Purge delay completed. Loading true Main Menu layout scene...");

        // 2. Load the MainMenu scene. (When it loads, it will automatically destroy this script safely)
        SceneManager.LoadScene("MainMenu");
    }
}