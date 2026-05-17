using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class book : MonoBehaviour
{
    [SerializeField] float pageSpeed = 1.5f;
    [SerializeField] List<Transform> pages;

    // This list keeps track of which pages the player has found
    public List<bool> unlockedPages;

    int index = -1;
    bool rotate = false;
    [SerializeField] GameObject backButton;
    [SerializeField] GameObject forwardButton;

    private void Start()
    {
        // Initialize the unlocked list if it's empty
        if (unlockedPages.Count == 0)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                // Set first page (index 0) to true, others to false
                unlockedPages.Add(i == 0);
            }
        }
        InitialState();
    }

    public void InitialState()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] != null)
            {
                pages[i].transform.rotation = Quaternion.identity;
                // Only show the page if it is unlocked
                pages[i].gameObject.SetActive(unlockedPages[i]);
            }
        }

        // Stacking logic only for unlocked pages
        for (int i = pages.Count - 1; i >= 0; i--)
        {
            if (pages[i] != null && unlockedPages[i])
                pages[i].SetAsLastSibling();
        }

        index = -1;
        if (backButton != null) backButton.SetActive(false);

        // Forward button only shows if the NEXT page is unlocked
        CheckForwardButton();
    }

    // Function to call when you pick up a page drop
    public void UnlockPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < unlockedPages.Count)
        {
            unlockedPages[pageIndex] = true;

            // Debug here to make sure this code is actually running!
            Debug.Log("Page " + pageIndex + " is now set to TRUE");

            // Refresh the objects
            InitialState();
        }
    }

    private void CheckForwardButton()
    {
        if (forwardButton == null) return;

        int nextIndex = index + 1;
        // Show forward button only if the next page exists AND is unlocked
        if (nextIndex < pages.Count && unlockedPages[nextIndex])
        {
            forwardButton.SetActive(true);
        }
        else
        {
            forwardButton.SetActive(false);
        }
    }

    public void RotateForward()
    {
        int nextIndex = index + 1;
        if (rotate || nextIndex >= pages.Count || !unlockedPages[nextIndex]) { return; }

        index++;
        float angle = 180;
        pages[index].SetAsLastSibling();

        if (backButton != null) backButton.SetActive(true);
        CheckForwardButton();

        StartCoroutine(Rotate(angle, true));
    }

    public void RotateBack()
    {
        if (rotate || index < 0) { return; }

        float angle = 0;
        pages[index].SetAsLastSibling();

        if (forwardButton != null) forwardButton.SetActive(true);
        if (index - 1 == -1 && backButton != null) backButton.SetActive(false);

        StartCoroutine(Rotate(angle, false));
    }

    IEnumerator Rotate(float angle, bool forward)
    {
        float value = 0f;
        while (true)
        {
            rotate = true;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
            value += Time.deltaTime * pageSpeed;

            if (index < 0 || index >= pages.Count) { rotate = false; yield break; }

            pages[index].rotation = Quaternion.Slerp(pages[index].rotation, targetRotation, value);

            if (Quaternion.Angle(pages[index].rotation, targetRotation) < 0.1f)
            {
                pages[index].rotation = targetRotation;
                if (!forward) { index--; }
                rotate = false;
                break;
            }
            yield return null;
        }
    }

    public void ResetState()
    {
        if (rotate && index >= 0 && index < pages.Count)
        {
            float finalAngle = (pages[index].rotation.eulerAngles.y > 90) ? 180 : 0;
            pages[index].rotation = Quaternion.Euler(0, finalAngle, 0);
        }
        StopAllCoroutines();
        rotate = false;
    }
}