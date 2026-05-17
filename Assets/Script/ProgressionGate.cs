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

    void Start()
    {
        // Default target link to itself if left unassigned in editor slots
        if (objectToGate == null)
        {
            objectToGate = this.gameObject;
        }

        // Check conditions instantly at startup
        EvaluateGate();

        // If page has not been recovered yet, schedule loop checks to listen for pickup updates
        if (!isUnlocked)
        {
            InvokeRepeating(nameof(EvaluateGate), checkInterval, checkInterval);
        }
    }

    void EvaluateGate()
    {
        if (BookManager.Instance == null) return;

        // Query BookManager directly to see if the target page index reads TRUE
        if (BookManager.Instance.IsPageUnlocked(requiredPageIndex))
        {
            UnlockTarget();
        }
        else
        {
            // Lock the state if page is missing
            if (objectToGate.activeSelf)
            {
                objectToGate.SetActive(false);
            }
        }
    }

    private void UnlockTarget()
    {
        isUnlocked = true;

        // Wake up your hidden enemy or spawner trigger
        objectToGate.SetActive(true);
        Debug.Log($"[ProgressionGate] Target page index {requiredPageIndex} collected! Activating object: {objectToGate.name}");

        // Cancel the loop entirely to maintain flawless frame rate performance
        CancelInvoke(nameof(EvaluateGate));
    }
}

