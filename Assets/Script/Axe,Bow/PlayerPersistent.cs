using UnityEngine;

public class PlayerPersistence : MonoBehaviour
{
    // Make sure this says PlayerPersistence, not PersistentUI!
    public static PlayerPersistence Instance;

    void Awake()
    {
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
}