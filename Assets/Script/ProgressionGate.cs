using UnityEngine;

public class ProgressionGate : MonoBehaviour
{
    [Header("Gate Configuration")]
    [Tooltip("The Enemy GameObject or Spawner that needs to be hidden until the page condition is met.")]
    [SerializeField] private GameObject objectToGate;

    [Tooltip("The page index required to unlock this object. (0 = Page 1, 1 = Page 2, 2 = Page 3, etc.)")]
    [SerializeField] private int requiredPageIndex = 0;

    [Tooltip("How often (in seconds) the script checks your logbook for updates.")]
    [SerializeField] private float checkInterval = 0.5f;

    private bool isUnlocked = false;
    private string uniqueSaveKey;

    void Start()
    {
        // Default target link to itself if left unassigned in editor slots
        if (objectToGate == null)
        {
            objectToGate = this.gameObject;
        }

        // Create a unique key using the object's name and position in the world
        uniqueSaveKey = "ProgGate_" + gameObject.name + "_" + transform.position.ToString();

        // UNIQUE MEMORY CHECK: If PlayerPrefs remembers this specific gate was already unlocked in a past visit...
        if (PlayerPrefs.GetInt(uniqueSaveKey, 0) == 1)
        {
            Debug.Log($"[ProgressionGate] {gameObject.name} was already unlocked permanently in a previous scene visit. Vaporizing to prevent duplicate drops!");
            Destroy(objectToGate);
            if (objectToGate != this.gameObject)
            {
                Destroy(this.gameObject);
            }
            return; // Exit out instantly!
        }

        // --- Your Exact Original Logic Below ---
        EvaluateGate();

        if (!isUnlocked)
        {
            InvokeRepeating(nameof(EvaluateGate), checkInterval, checkInterval);
        }
    }

    void EvaluateGate()
    {
        if (BookManager.Instance == null) return;

        if (BookManager.Instance.IsPageUnlocked(requiredPageIndex))
        {
            UnlockTarget();
        }
        else
        {
            // Your exact original line that keeps them safely hidden until the condition matches!
            if (objectToGate.activeSelf)
            {
                objectToGate.SetActive(false);
            }
        }
    }

    private void UnlockTarget()
    {
        isUnlocked = true;

        // Save to the computer's memory that THIS specific object has finished its job completely
        PlayerPrefs.SetInt(uniqueSaveKey, 1);
        PlayerPrefs.Save();

        objectToGate.SetActive(true);
        Debug.Log($"[ProgressionGate] Target page index {requiredPageIndex} collected! Activating object: {objectToGate.name}");

        CancelInvoke(nameof(EvaluateGate));
    }
}