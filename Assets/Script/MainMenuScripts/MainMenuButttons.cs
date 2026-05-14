using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsPanel;
    public GameObject mainButtonsGroup;
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        // Force the cursor to be visible and free when the menu loads
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void PlayGame()
    {
        // 1. Clear the saved exit point from previous play sessions
        // This prevents the player from teleporting to Cave coordinates in the World scene.
        PlayerPrefs.DeleteKey("LastExit");

        // 2. Load the first game scene
        SceneManager.LoadScene("wodbeyl");
    }

    public void GoToMainMenu()
    {
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

        // This helps you test the quit button while inside the Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}