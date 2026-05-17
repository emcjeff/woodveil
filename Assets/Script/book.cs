using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class book : MonoBehaviour
{
    [Header("Book Settings")]
    [SerializeField] private float pageSpeed = 1.5f;
    [SerializeField] private List<Transform> pages;

    [Header("Progression Status")]
    [Tooltip("Match this size to your total page count. Set true for pages the player starts with, false for pages they must find.")]
    public List<bool> unlockedPages;

    [Header("Navigation Buttons")]
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject forwardButton;

    private int index = -1;
    private bool rotate = false;

    private void Start()
    {
        // SAFETY: If the list was left completely unassigned, initialize it dynamically
        if (unlockedPages == null || unlockedPages.Count == 0)
        {
            unlockedPages = new List<bool>();
            for (int i = 0; i < pages.Count; i++)
            {
                // Only the very first page sheet is active by default
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
                // Display the page if it is officially unlocked
                pages[i].gameObject.SetActive(unlockedPages[i]);
            }
        }

        // Stacking order priority logic for active pages
        for (int i = pages.Count - 1; i >= 0; i--)
        {
            if (pages[i] != null && unlockedPages[i])
                pages[i].SetAsLastSibling();
        }

        index = -1;
        if (backButton != null) backButton.SetActive(false);

        CheckForwardButton();
    }

    public void UnlockPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < unlockedPages.Count)
        {
            unlockedPages[pageIndex] = true;
            Debug.Log($"[Book System] Page Index {pageIndex} has been set to TRUE (Unlocked!)");

            // Refresh the pages instantly
            InitialState();
        }
    }

    private void CheckForwardButton()
    {
        if (forwardButton == null) return;

        int nextIndex = index + 1;
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

    private IEnumerator Rotate(float angle, bool forward)
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