using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class book : MonoBehaviour
{
    [SerializeField] float pageSpeed = 0.5f;
    [SerializeField] List<Transform> pages;
    int index = -1;
    bool rotate = false;
    [SerializeField] GameObject backButton;
    [SerializeField] GameObject forwardButton;

    private void Start()
    {
        InitialState();
    }

    public void InitialState()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].transform.rotation = Quaternion.identity;
        }

        if (pages.Count > 0)
        {
            pages[0].SetAsLastSibling();
        }

        index = -1; // Reset index to start
        backButton.SetActive(false);
        forwardButton.SetActive(true);
    }

    // --- FIX: SNAPS PAGE TO FINISH WHEN CLOSED ---
    public void ResetState()
    {
        if (rotate && index >= 0 && index < pages.Count)
        {
            // If we were mid-rotate, find out if we were going forward or back
            // and snap the rotation to the final destination immediately.
            float finalAngle = (pages[index].rotation.eulerAngles.y > 90) ? 180 : 0;
            pages[index].rotation = Quaternion.Euler(0, finalAngle, 0);
        }

        StopAllCoroutines();
        rotate = false;
    }

    public void RotateForward()
    {
        if (rotate || index >= pages.Count - 1) { return; }

        index++;
        float angle = 180;
        ForwardButtonActions();
        pages[index].SetAsLastSibling();
        StartCoroutine(Rotate(angle, true));
    }

    public void ForwardButtonActions()
    {
        if (backButton.activeInHierarchy == false)
        {
            backButton.SetActive(true);
        }
        if (index == pages.Count - 1)
        {
            forwardButton.SetActive(false);
        }
    }

    public void RotateBack()
    {
        if (rotate || index < 0) { return; }

        float angle = 0;
        pages[index].SetAsLastSibling();
        BackButtonActions();
        StartCoroutine(Rotate(angle, false));
    }

    public void BackButtonActions()
    {
        if (forwardButton.activeInHierarchy == false)
        {
            forwardButton.SetActive(true);
        }
        if (index - 1 == -1)
        {
            backButton.SetActive(false);
        }
    }

    IEnumerator Rotate(float angle, bool forward)
    {
        float value = 0f;
        while (true)
        {
            rotate = true;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
            value += Time.deltaTime * pageSpeed;

            // Safety check for the Error in image_3c801a.png
            if (index < 0 || index >= pages.Count) { rotate = false; yield break; }

            pages[index].rotation = Quaternion.Slerp(pages[index].rotation, targetRotation, value);

            float angleDifference = Quaternion.Angle(pages[index].rotation, targetRotation);

            if (angleDifference < 0.1f)
            {
                pages[index].rotation = targetRotation;
                if (forward == false) { index--; }
                rotate = false;
                break;
            }
            yield return null;
        }
    }
}