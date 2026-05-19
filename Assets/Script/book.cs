using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class book : MonoBehaviour
{
    [Header("Book Settings")]
    [SerializeField] private float pageSpeed = 1.5f;
    [SerializeField] private List<Transform> pages;

    [Header("Navigation Buttons")]
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject forwardButton;

    private int index = -1;
    private bool rotate = false;

    private void Start()
    {
        SyncAndRenderLayout();
    }

    public void SyncAndRenderLayout()
    {
        index = -1;
        if (BookManager.Instance == null) return;

        // Reset and sync all pages using BookManager's safe central state
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] != null)
            {
                pages[i].transform.rotation = Quaternion.identity;
                bool isUnlocked = BookManager.Instance.IsPageUnlocked(i);
                pages[i].gameObject.SetActive(isUnlocked);
            }
        }

        // Re-order sorting hierarchy layout right from the start
        UpdatePageHierarchyStacking();

        if (backButton != null) backButton.SetActive(false);
        CheckForwardButton();
    }

    private void CheckForwardButton()
    {
        if (forwardButton == null || BookManager.Instance == null) return;

        int nextIndex = index + 1;
        if (nextIndex < pages.Count && BookManager.Instance.IsPageUnlocked(nextIndex))
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
        if (BookManager.Instance == null || rotate || nextIndex >= pages.Count || !BookManager.Instance.IsPageUnlocked(nextIndex)) { return; }

        index++;
        float angle = 180;

        // Bring the moving page to the absolute front during flip animation transition
        pages[index].SetAsLastSibling();

        if (backButton != null) backButton.SetActive(true);
        CheckForwardButton();

        StartCoroutine(Rotate(angle, true));
    }

    public void RotateBack()
    {
        if (rotate || index < 0) { return; }

        float angle = 0;

        // Bring the moving page to the absolute front during flip animation transition
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

                if (!forward)
                {
                    index--;
                }

                // Recalculate sorting stack completely once page rests
                UpdatePageHierarchyStacking();

                rotate = false;
                break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// FIXED: Corrects layering order dynamically for both sides.
    /// Left side pile layout: Higher numbers render ON TOP of lower numbers (Page 2 covers Page 1).
    /// Right side stack layout: Lower numbers render ON TOP of higher numbers (Page 1 covers Page 2).
    /// </summary>
    private void UpdatePageHierarchyStacking()
    {
        // Loop backward from last page to first page to build the proper UI drawing queue stack
        for (int i = pages.Count - 1; i >= 0; i--)
        {
            if (pages[i] == null || !pages[i].gameObject.activeSelf) continue;

            if (i <= index)
            {
                // LEFT SIDE PAGES PILE:
                // We want higher index pages to cover lower index pages (e.g., Page 2 covers Page 1)
                // By making lower numbers Last Sibling first, then higher numbers Last Sibling after,
                // the higher numbers stack right on top.
                pages[i].SetAsLastSibling();
            }
            else
            {
                // RIGHT SIDE PAGES PILE:
                // We want lower index pages to cover higher index pages (e.g., Page 1 covers Page 2)
                // By sending higher numbers to First Sibling first, they drop to the back ground,
                // allowing the lower numbers to naturally stay in the front.
                pages[i].SetAsFirstSibling();
            }
        }

        // Safety override: The current page actively being flipped or resting open must stay on top of the piles
        if (index >= 0 && index < pages.Count && pages[index] != null)
        {
            pages[index].SetAsLastSibling();
        }

        // Also ensure the next page in line on the right side is visible on top of its pile
        int nextRightPage = index + 1;
        if (nextRightPage < pages.Count && pages[nextRightPage] != null && pages[nextRightPage].gameObject.activeSelf)
        {
            pages[nextRightPage].SetAsLastSibling();
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
        SyncAndRenderLayout();
    }
}