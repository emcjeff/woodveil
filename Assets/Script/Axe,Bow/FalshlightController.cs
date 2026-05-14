using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public static FlashlightController Instance;

    [Header("Settings")]
    public GameObject lightSource; // Drag your Spotlight here
    public bool hasHelmet = false; // Player must find it first
    private bool isOn = false;

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

    void Start()
    {
        // Ensure the light starts OFF
        if (lightSource != null)
        {
            lightSource.SetActive(false);
        }
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

            // Optional: Add a click sound here
            // AudioSource.PlayClipAtPoint(clickSound, transform.position);
        }
    }

    // Call this function from your Interaction script when picking up the helmet
    public void PickUpHelmet()
    {
        hasHelmet = true;
        Debug.Log("Helmet Picked Up! You can now press 'F' to use the light.");
    }
}