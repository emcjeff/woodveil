using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsPanel;
    public GameObject mainButtonsGroup;
    public string mainMenuSceneName = "MainMenu";

    public void PlayGame()
    {
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
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#endif
    }
}