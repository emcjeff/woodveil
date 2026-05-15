using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class EventSystemPersistence : MonoBehaviour
{
    public static EventSystemPersistence Instance;

    private void Awake()
    {
        // 1. SINGLETON CHECK: 
        // If an EventSystem already exists from a previous scene, delete this new one.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == " " || currentScene == "GameOver")
        {
            Destroy(gameObject);
        }
    }
}