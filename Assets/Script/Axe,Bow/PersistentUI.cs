using UnityEngine;

public class PersistentUI : MonoBehaviour
{
    // We removed the 'static Instance' part so you can put this 
    // on multiple objects (Player, Canvas, Book) without them killing each other.

    private void Awake()
    {
        // This is the only line you really need to survive the scene change
        DontDestroyOnLoad(gameObject);
    }
}