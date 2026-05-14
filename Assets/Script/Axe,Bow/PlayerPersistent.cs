using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistence : MonoBehaviour
{
    public static PlayerPersistence Instance;

    void Awake()
    {
        // SINGLETON LOGIC: Keeps the first Player alive, destroys any new ones
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // MENU CLEANUP: If the player accidentally exists in the Main Menu scene, kill it.
        // Make sure "MainMenu" matches your scene name exactly!
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Destroy(gameObject);
        }
    }
}