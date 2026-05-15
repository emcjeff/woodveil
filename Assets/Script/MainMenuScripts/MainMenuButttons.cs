using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsPanel;
    public GameObject mainButtonsGroup;
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        // Ensures the mouse is visible so you can click the Restart/Menu buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlayGame()
    {
        // 1. Clear the saved exit point so the player starts fresh in the world
        PlayerPrefs.DeleteKey("LastExit");

        // 2. Load the first game scene (Wodbeyl)
        SceneManager.LoadScene("wodbeyl");
    }

    // --- NEW FUNCTION FOR GAME OVER ---
    public void ReturnToMainMenu()
    {
        // This takes the player back to the title screen
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(false);
    }

    public void BackFromOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}