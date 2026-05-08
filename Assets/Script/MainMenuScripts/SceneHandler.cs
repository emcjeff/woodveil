using UnityEngine;

public class SceneHandler : MonoBehaviour
{
    void Start()
    {
        // 1. Check if we have a saved exit point from the previous scene
        string lastExit = PlayerPrefs.GetString("LastExit", "");

        if (!string.IsNullOrEmpty(lastExit))
        {
            // 2. Try to find the object with that name (e.g., "ArrivalFromCave")
            GameObject arrivalPoint = GameObject.Find(lastExit);

            if (arrivalPoint != null)
            {
                // 3. Find the player
                GameObject player = GameObject.FindGameObjectWithTag("Player");

                if (player != null)
                {
                    CharacterController cc = player.GetComponent<CharacterController>();

                    // 4. IMPORTANT: Disable CharacterController before moving
                    if (cc != null) cc.enabled = false;

                    player.transform.position = arrivalPoint.transform.position;
                    player.transform.rotation = arrivalPoint.transform.rotation;

                    // 5. Re-enable it so the player can move again
                    if (cc != null) cc.enabled = true;

                    // Clear the PlayerPrefs so we don't teleport again on a normal reload
                    PlayerPrefs.SetString("LastExit", "");
                }
            }
        }
    }
}