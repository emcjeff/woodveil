using UnityEngine;
using TMPro; // Required for TextMeshPro
using System.Collections;

public class CraftingUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float displayDuration = 3f;

    public void CraftAxe()
    {
        // Trigger the on-screen message
        StartCoroutine(ShowNotification("[Successfully Crafted Axe]"));
    }

    private IEnumerator ShowNotification(string message)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        // Wait for a few seconds, then hide the message
        yield return new WaitForSeconds(displayDuration);
        
        notificationText.gameObject.SetActive(false);
    }
}