using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class book : MonoBehaviour
{
    [Header("Book Settings")]
    [SerializeField] private float pageSpeed = 1.5f;
    [SerializeField] private List<Transform> pages;

    [Header("Live Checklist (Check/Uncheck boxes to test live!)")]
    [Tooltip("You can manually check/uncheck these boxes during Play Mode to test page visibility instantly!")]
    public List<bool> unlockedPages;

    [Header("Navigation Buttons")]
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject forwardButton;

    private int index = -1;
    private bool rotate = false;

    private void Start()
    {
        InitialState();
    }

    // Called automatically by Unity Editor when you toggle fields in the Inspector
    private void OnValidate()
    {
        if (pages != null && (unlockedPages == null || unlockedPages.Count != pages.Count))
        {
            SyncInspectorChecklistSize();
        }
    }

    private void Update()
    {
        // If you are playing and manually checking/unchecking boxes in the inspector,
        // instantly push those values over to update the real GameObjects and BookManager state.
        if (Application.isPlaying && BookManager.Instance != null)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                if (i < unlockedPages.Count && i < BookManager.Instance.unlockedPages.Count)
                {
                    if (BookManager.Instance.unlockedPages[i] != unlockedPages[i])
                    {
                        BookManager.Instance.unlockedPages[i] = unlockedPages[i];
                        RefreshPageVisibility();
                    }
                }
            }
        }
    }

    public void SyncAndRenderLayout()
    {
        InitialState();
    }

    public void InitialState()
    {
        index = -1;
        if (BookManager.Instance == null) return;

        SyncInspectorChecklistSize();

        // Download data from global BookManager into our local interactive checklist
        for (int i = 0; i < pages.Count; i++)
        {
            unlockedPages[i] = BookManager.Instance.IsPageUnlocked(i);
        }

        RefreshPageVisibility();
    }

    public void RefreshPageVisibility()
    {
        if (BookManager.Instance == null) return;

        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] != null)
            {
                pages[i].transform.rotation = Quaternion.identity;
                bool isUnlocked = BookManager.Instance.IsPageUnlocked(i);
                pages[i].gameObject.SetActive(isUnlocked);
            }
        }

        // Handle UI overlay stacking hierarchies
        for (int i = pages.Count - 1; i >= 0; i--)
        {
            if (pages[i] != null && BookManager.Instance.IsPageUnlocked(i))
            {
                pages[i].SetAsLastSibling();
            }
        }

        if (backButton != null) backButton.SetActive(false);
        CheckForwardButton();
    }

    private void SyncInspectorChecklistSize()
    {
        if (unlockedPages == null) unlockedPages = new List<bool>();
        while (unlockedPages.Count < pages.Count) unlockedPages.Add(unlockedPages.Count == 0);
        while (unlockedPages.Count > pages.Count) unlockedPages.RemoveAt(unlockedPages.Count - 1);
    }

    public void UnlockPage(int pageIndex)
    {
        if (BookManager.Instance != null)
        {
            BookManager.Instance.UnlockPageGlobal(pageIndex);
            InitialState();
        }
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
        InitialState();
    }
}