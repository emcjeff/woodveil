using UnityEngine;
using System.Collections;

public class LoadingScreenOverlay : MonoBehaviour
{
    private static LoadingScreenOverlay instance;
    public static LoadingScreenOverlay Instance => instance;

    [Header("References")]
    [Tooltip("Drag the full-screen black image panel child object here.")]
    [SerializeField] private GameObject visualPanel;

    [Header("Settings")]
    [Tooltip("Minimum time the loading screen stays visible (in seconds) before hiding.")]
    [SerializeField] private float loadingScreenDuration = 2.5f;

    private Coroutine hideRoutine;

    private void Awake()
    {
        // Singleton pattern to prevent the accidental double-suicide bug
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Start hidden by default when the game boots up
        if (visualPanel != null) visualPanel.SetActive(false);
    }

    /// <summary>
    /// Shows the loading curtain overlay overlay.
    /// </summary>
    public void Show()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (visualPanel != null)
        {
            visualPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Starts a timed delay sequence before turning off the loading screen panel.
    /// </summary>
    public void HideWithDelay()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }
        hideRoutine = StartCoroutine(WaitAndHideRoutine());
    }

    /// <summary>
    /// Instantly forces the loading screen overlay to hide without waiting.
    /// </summary>
    public void InstantHide()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (visualPanel != null)
        {
            visualPanel.SetActive(false);
        }
    }

    private IEnumerator WaitAndHideRoutine()
    {
        // Uses unscaled real system time clock cycles so it works even if Time.timeScale == 0
        yield return new WaitForSecondsRealtime(loadingScreenDuration);

        if (visualPanel != null)
        {
            visualPanel.SetActive(false);
        }

        hideRoutine = null;
    }
}