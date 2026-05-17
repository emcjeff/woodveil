using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistence : MonoBehaviour
{
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
            return;
        }
    }

    private void OnEnable()
    {
        // Tell Unity to run "OnSceneLoaded" every time a scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Ignore spawning if we are back at the Main Menu
        if (scene.name == "MainMenu") return;

        // 2. Find the object named "SpawnPoint" with the tag "SpawnPoint"
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");

        if (spawnPoint != null)
        {
            // 3. Move the player to the spawn point position and rotation
            TeleportPlayer(spawnPoint.transform.position, spawnPoint.transform.rotation);

            // ========================================================
            // NEW CODE: DISCOVER ENEMIES AND LINK THE PLAYER REFERENCE
            // ========================================================
            
            // Link all 3D Slimes in the new scene
            SlimeMovement3D[] allSlimes = FindObjectsByType<SlimeMovement3D>(FindObjectsSortMode.None);
            foreach (SlimeMovement3D slime in allSlimes)
            {
                slime.player = this.transform;
            }

            // // Link all 3D Spiders in the new scene
            // SpiderMovement3D[] allSpiders = FindObjectsByType<SpiderMovement3D>(FindObjectsSortMode.None);
            // foreach (SpiderMovement3D spider in allSpiders)
            // {
            //     spider.player = this.transform;
            // }

            // Debug.Log($"📢 PLAYER: Broadcasted presence to {allSlimes.Length} Slimes and {allSpiders.Length} Spiders!");
            // ========================================================
        }
        else
        {
            Debug.LogWarning("No SpawnPoint found in " + scene.name);
        }
    }

    public void TeleportPlayer(Vector3 position, Quaternion rotation)
    {
        // If you are using a CharacterController, you must disable it before moving
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        transform.position = position;
        transform.rotation = rotation;

        if (cc != null) cc.enabled = true;

        Debug.Log("Player moved to SpawnPoint.");
    }

    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Destroy the player if we are in MainMenu OR GameOver
        if (currentScene == " " || currentScene == "GameOver")
        {
            Destroy(gameObject);
        }
    }
}