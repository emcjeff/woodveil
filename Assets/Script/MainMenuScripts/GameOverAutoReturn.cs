using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverAutoReturn : MonoBehaviour
{
    public float delay = 2.5f;

    void Start()
    {
        // If the MainMenu script says "isLongReturning", start the timer
        if (MainMenu.isLongReturning)
        {
            StartCoroutine(ReturnSequence());
        }
    }

    IEnumerator ReturnSequence()
    {
        yield return new WaitForSecondsRealtime(delay);

        // Reset the passport
        MainMenu.isLongReturning = false;

        // Go back home
        SceneManager.LoadScene("MainMenu");
    }
}