using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If the game just started and we haven't touched a trigger yet,
        // PlayerPrefs will be empty or "Default".
        string lastExit = PlayerPrefs.GetString("LastExit", "Default");

        if (lastExit != "Default")
        {
            GameObject spawnPoint = GameObject.Find(lastExit);
            if (spawnPoint != null)
            {
                // Move the player to the exit they just came through
                transform.position = spawnPoint.transform.position;
                transform.rotation = spawnPoint.transform.rotation;
            }
        }
        // If lastExit IS "Default", the script does nothing,
        // leaving you at your original starting position.
    }
}