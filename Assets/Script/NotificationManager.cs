using System.Collections;
using UnityEngine;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("UI Reference")]
    public TextMeshProUGUI notificationText;
    public float displayDuration = 2.5f;

    private Coroutine currentFadeRoutine;

    void Awake()
    {
        // Simple singleton so any item can find this UI quickly
        if (Instance == null) Instance = this;
    }

    public void ShowNotification(string message)
    {
        if (notificationText == null) return;

        // If a notification is already running, stop it so we can overwrite it
        if (currentFadeRoutine != null)
        {
            StopCoroutine(currentFadeRoutine);
        }

        currentFadeRoutine = StartCoroutine(DisplayRoutine(message));
    }

    IEnumerator DisplayRoutine(string message)
    {
        // Set the text and turn the object ON
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        // Wait on screen
        yield return new WaitForSeconds(displayDuration);

        // Turn the text object OFF
        notificationText.text = "";
        notificationText.gameObject.SetActive(false);
    }
}