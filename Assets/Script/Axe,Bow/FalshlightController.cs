using UnityEngine;
using UnityEngine.SceneManagement; // NEW: Required for automatic scene transition checks

public class FlashlightController : MonoBehaviour
{
    public static FlashlightController Instance;

    [Header("Settings")]
    public GameObject lightSource; // Drag your Spotlight here
    public bool hasHelmet = false; // Player must find it first
    private bool isOn = false;

    [Header("UI References")]
    [Tooltip("The UI Group GameObject named 'HeadLamp' on your Canvas overlay.")]
    [SerializeField] private GameObject headlampUI;

    private void Awake()
    {
        // Keep the controller alive between scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Subscribe to Unity's scene loading loop to keep our UI linked dynamically
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Clean up subscription state safely if component is unloaded
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Ensure the light starts OFF
        if (lightSource != null)
        {
            lightSource.SetActive(false);
        }

        // Run an initial search setup frame
        FindAndSyncHeadlampUI();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Automatically look for the new UI Canvas whenever a new level/scene finishes loading!
        FindAndSyncHeadlampUI();
    }

    void Update()
    {
        // Only works if player has the helmet and presses 'F'
        if (hasHelmet && Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }
    }

    public void ToggleFlashlight()
    {
        if (lightSource != null)
        {
            isOn = !isOn;
            lightSource.SetActive(isOn);
        }
    }

    // Call this function from your Interaction script when picking up the helmet
    public void PickUpHelmet()
    {
        hasHelmet = true;
        Debug.Log("Helmet Picked Up! You can now press 'F' to use the light.");

        // Turn the UI display component ON immediately upon pickup item contact
        if (headlampUI != null)
        {
            headlampUI.SetActive(true);
        }
    }

    /// <summary>
    /// Helper framework that finds and assigns the HeadLamp UI, even if it's inactive or freshly spawned by a scene change.
    /// </summary>
    private void FindAndSyncHeadlampUI()
    {
        // If our current UI reference is missing or empty, search the open scene canvas for it
        if (headlampUI == null)
        {
            // Looks through all objects in the scene layout, including hidden/inactive ones, matching the name exactly
            Transform[] matches = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (Transform t in matches)
            {
                if (t.name == "HeadLamp" && t.gameObject.scene.IsValid())
                {
                    headlampUI = t.gameObject;
                    break;
                }
            }
        }

        // Sync the visibility to reflect whether the player owns the item or not!
        if (headlampUI != null)
        {
            headlampUI.SetActive(hasHelmet);
        }
    }
}