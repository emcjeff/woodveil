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

        // Sync visual structure directly using BookManager's safe central state
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] != null)
            {
                pages[i].transform.rotation = Quaternion.identity;
                bool isUnlocked = BookManager.Instance.IsPageUnlocked(i);
                pages[i].gameObject.SetActive(isUnlocked);
            }
        }

        for (int i = pages.Count - 1; i >= 0; i--)
        {
            if (pages[i] != null && BookManager.Instance.IsPageUnlocked(i))
                pages[i].SetAsLastSibling();
        }

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

                if (forward)
                {
                    pages[index].SetAsFirstSibling();
                }
                else
                {
                    index--;
                }

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