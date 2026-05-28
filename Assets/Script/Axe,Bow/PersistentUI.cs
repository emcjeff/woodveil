using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentUI : MonoBehaviour
{
    // We use a simple Awake for this one so you can put it on 
    // the Canvas and EventSystem separately.
    private void Awake()
    {
        // Survival command
        DontDestroyOnLoad(gameObject);

        // Safety: If you have a specific object that should only have ONE copy 
        // (like a secondary manager), you'd use a Singleton. 
        // For general UI, this line is enough to make it travel.
    }

    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == " " || currentScene == "GameOver")
        {
            Destroy(gameObject);
        }
    }
}