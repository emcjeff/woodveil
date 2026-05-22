using UnityEngine;

public class SceneHandler : MonoBehaviour
{
    [Header("Fallback Settings")]
    [Tooltip("The exact name of the default spawn point GameObject used for Retries and Fresh Starts.")]
    [SerializeField] private string defaultStartPointName = "DefaultStartPoint";

    void Start()
    {
        // 1. Check if we have a saved exit point from scene-to-scene traversal
        string lastExit = PlayerPrefs.GetString("LastExit", "");
        GameObject arrivalPoint = null;

        if (!string.IsNullOrEmpty(lastExit))
        {
            // Try to find the transition gate arrival point (e.g., "ArrivalFromCave")
            arrivalPoint = GameObject.Find(lastExit);
        }

        // 2. CRITICAL RETRY FIX: If no transition exit was found, locate the level's default start spawn
        if (arrivalPoint == null)
        {
            arrivalPoint = GameObject.Find(defaultStartPointName);
            Debug.Log($"[SceneHandler] No travel gate key found. Defaulting arrival destination to: '{defaultStartPointName}'");
        }

        // 3. Move the player if an arrival point was successfully resolved
        if (arrivalPoint != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();

                // 4. Disable CharacterController to safely override coordinate transforms
                if (cc != null) cc.enabled = false;

                player.transform.position = arrivalPoint.transform.position;
                player.transform.rotation = arrivalPoint.transform.rotation;

                if (cc != null) cc.enabled = true;

                Debug.Log($"[SceneHandler] Player successfully positioned at: {arrivalPoint.name}");

                // Clear the PlayerPrefs so normal non-transition reloads run smoothly
                PlayerPrefs.SetString("LastExit", "");
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogWarning("[SceneHandler] Player object not found! Check your 'Player' tag settings.");
            }
        }
        else
        {
            Debug.LogError($"[SceneHandler] Emergency: Could not find '{defaultStartPointName}' or transition point in hierarchy! Spawning failed.");
        }
    }
}