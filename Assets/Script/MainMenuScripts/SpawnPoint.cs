using UnityEngine;

public class SpawnPointTrigger : MonoBehaviour
{
    void Start()
    {
        // Increase the delay slightly for the build (0.2s) 
        // to ensure the DontDestroyOnLoad objects have "landed"
        Invoke("InitializeLevel", 0.2f);
    }

    void InitializeLevel()
    {
        // 1. Find the Player (using Tag is safest)
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // 2. Find the Canvas (even if it is invisible/disabled)
        GameObject gameplayCanvas = null;

        // This search finds the Canvas even if it is inactive
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            if (c.name == "Canvas") // Make sure this matches your Hierarchy name exactly!
            {
                gameplayCanvas = c.gameObject;
                break;
            }
        }

        // 3. Execute the Wake-up Call
        if (gameplayCanvas != null)
        {
            gameplayCanvas.SetActive(true);
            Debug.Log("UI Restored.");
        }

        if (player != null)
        {
            // Stop any movement before teleporting to prevent the "Sky Bug"
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            player.transform.position = transform.position;
            player.transform.rotation = transform.rotation;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("Player Teleported to: " + transform.position);
        }
        else
        {
            Debug.LogError("SpawnPointTrigger: Could not find Player Tag!");
        }
    }
}