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
        // 2. MAIN MENU CLEANUP: 
        // When going back to the menu, we destroy this persistent copy.
        // This allows a fresh menu EventSystem to take over.
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Destroy(gameObject);
        }
    }
}