using UnityEngine;
using UnityEngine.SceneManagement;

public class FlashlightController : MonoBehaviour
{
    public static FlashlightController Instance { get; private set; }

    [Header("Settings")]
    public GameObject lightSource;
    public bool hasHelmet = false;
    private bool isOn = false;

    [Header("UI References")]
    [Tooltip("The UI Group GameObject named 'HeadLamp' on your Canvas overlay.")]
    [SerializeField] private GameObject headlampUI;

    private void Awake()
    {
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

        // FIX: Hard-reset tracking variables on initialization
        ResetFlashlightState();
    }

    /// <summary>
    /// Resets item collection state when returning to the main menu system.
    /// </summary>
    public void ResetFlashlightState()
    {
        hasHelmet = false;
        isOn = false;
        if (lightSource != null) lightSource.SetActive(false);
        if (headlampUI != null) headlampUI.SetActive(false);
        Debug.Log("[Flashlight Manager] Equipment variables reset.");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (lightSource != null) lightSource.SetActive(false);
        FindAndSyncHeadlampUI();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndSyncHeadlampUI();
    }

    void Update()
    {
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

    public void PickUpHelmet()
    {
        hasHelmet = true;
        Debug.Log("Helmet Picked Up! You can now press 'F' to use the light.");

        if (headlampUI != null)
        {
            headlampUI.SetActive(true);
        }
    }

    private void FindAndSyncHeadlampUI()
    {
        // Force look up if target reference is null or lost during scene load
        if (headlampUI == null)
        {
            Transform[] matches = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (Transform t in matches)
            {
                // Verify the transform belongs to a valid running scene context to exclude editor assets
                if (t.name == "HeadLamp" && t.gameObject.scene.IsValid())
                {
                    headlampUI = t.gameObject;
                    break;
                }
            }
        }

        if (headlampUI != null)
        {
            headlampUI.SetActive(hasHelmet);
        }
    }
}