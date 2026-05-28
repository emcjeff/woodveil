using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string targetSceneName;
    public string destinationName; // Set this to "FromCave" or "FromWorld"

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Store the destination name so it persists across scenes
            PlayerPrefs.SetString("LastExit", destinationName);
            SceneManager.LoadScene(targetSceneName);
        }
    }
}